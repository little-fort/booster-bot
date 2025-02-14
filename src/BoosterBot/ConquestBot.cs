using BoosterBot.Helpers; // 引入辅助工具类
using BoosterBot.Models;  // 引入模型类
using BoosterBot.Resources; // 引入资源类
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoosterBot
{
    internal class ConquestBot : BaseBot
    {
        // 定义一个只读变量_maxTier，表示征服最高等级状态
        private readonly GameState _maxTier;

        // 构造函数，接收配置参数、撤退次数和征服最高等级
        public ConquestBot(BotConfig config, int retreat, GameState maxTier) : base(config, retreat)
        {
            _maxTier = maxTier;
            // 调试方法
        }

        // 调试模式，用于实时显示窗口位置和游戏状态
        public void Debug()
        {
            Console.WriteLine("************** DEBUG MODE **************\\n");
            while (true)
            {
                try
                {
                    // 获取窗口位置，记录当前游戏状态日志
                    _config.GetWindowPositions();

                    var print = _game.LogConquestGameState();
                    Console.Clear();
                    Console.WriteLine(DateTime.Now);

                    foreach (var line in print.Logs) Console.WriteLine(line); // 输出日志
                    foreach (var line in print.Results) Console.WriteLine(line); // 输出结果

                    Console.WriteLine();

                    // 倒计时显示，提示下一次扫描
                    for (int i = 4; i >= 0; i--)
                        for (int x = 9; x >= 0; x--)
                        {
                            var text = $"Re-scanning window contents in {i}.{x} seconds...";

                            // 将光标移动到最后一行的开头
                            Console.SetCursorPosition(0, Console.CursorTop);

                            // 写入新内容
                            Console.Write(text + new string(' ', Console.WindowWidth - text.Length - 1));

                            Thread.Sleep(100);
                        }

                    // 提示重新扫描窗口内容
                    var txt = $"Re-scanning window contents...";
                    Console.SetCursorPosition(0, Console.CursorTop);
                    Console.Write(txt + new string(' ', Console.WindowWidth - txt.Length - 1));
                }
                catch (Exception ex)
                {
                    // 捕获异常并输出错误信息
                    Console.WriteLine($"{Strings.Log_Error} {ex.Message}");
                    Thread.Sleep(5000);
                }
            }
        }

        // 主运行逻辑
        public override void Run()
        {
            Log("Conquest_Log_Start"); // 记录日志
            var attempts = 0; // 初始化尝试次数

            while (true)
            {
                attempts++;

                _config.GetWindowPositions(); // 获取窗口位置
                _game.ResetClick(); // 重置点击
                _game.ResetMenu(); // 重置菜单
                _game.ResetClick(); // 重置点击

                Thread.Sleep(300); // 等待

                _config.GetWindowPositions();
                var onMenu = Check(_game.CanIdentifyMainMenu); // 检查是否在主菜单
                if (onMenu)
                {
                    Log("Conquest_Log_Menu_Main");
                    NavigateToGameModes(); // 导航到游戏模式菜单
                    NavigateToConquestMenu(); // 导航到征服菜单
                    RunMatchLoop(); // 运行比赛循环
                }
                else
                {
                    if (attempts <= 2)
                    {
                        Log("Log_FailedMenuDetection");
                        _game.ResetClick();
                        Thread.Sleep(3000);
                    }
                    else
                    {
                        attempts = 0;
                        DetermineLoopEntryPoint(); // 确定循环入口点
                    }
                }
            }
        }

        // 导航到游戏模式菜单
        private void NavigateToGameModes()
        {
            Log("Conquest_Log_Menu_GameModes");
            SystemUtilities.Click(_config.GameModesPoint);
            Thread.Sleep(1000);
            SystemUtilities.Click(_config.GameModesPoint);
            Thread.Sleep(2000);
        }

        // 导航到征服菜单
        private void NavigateToConquestMenu()
        {
            Log("Conquest_Log_Menu");

            for (int x = 0; x < 3; x++)
            {
                SystemUtilities.Click(_config.ConquestBannerPoint);
                Thread.Sleep(1000);
            }
        }

        // 确定循环入口点
        private bool DetermineLoopEntryPoint(int attempts = 0)
        {
            Log("Log_LoopEntryPoint");
            var state = _game.DetermineConquestGameState(); // 确定当前游戏状态

            switch (state)
            {
                case GameState.MAIN_MENU:
                    Log("Conquest_Log_DetectedMain");
                    Run();
                    return true;
                case GameState.RECONNECT_TO_GAME:
                    Log("Log_DetectedReconnect");
                    _game.ClickPlay();
                    Thread.Sleep(4000);
                    return PlayMatch(); // 开始比赛
                case GameState.MID_MATCH:
                    Log("Log_DetectedActiveMatch");
                    return PlayMatch();
                case GameState.CONQUEST_LOBBY_PG:
                case GameState.CONQUEST_LOBBY_SILVER:
                case GameState.CONQUEST_LOBBY_GOLD:
                case GameState.CONQUEST_LOBBY_INFINITE:
                    Log("Conquest_Log_DetectedLobby");
                    SelectLobby(); // 选择大厅
                    return StartMatch(); // 开始比赛
                case GameState.CONQUEST_PREMATCH:
                    Log("Conquest_Log_DetectedPrematch");
                    return StartMatch();
                case GameState.CONQUEST_MATCHMAKING:
                    Log("Log_DetectedMatchmaking");
                    return WaitForMatchmaking(); // 等待匹配
                case GameState.CONQUEST_MATCH:
                    Log("Conquest_Log_DetectedActiveMatch");
                    return PlayMatch();
                case GameState.CONQUEST_ROUND_END:
                    Log("Conquest_Log_DetectedRoundEnd");
                    return ProgressRound(); // 进行下一回合
                case GameState.CONQUEST_MATCH_END:
                case GameState.CONQUEST_MATCH_END_REWARDS:
                    Log("Conquest_Log_DetectedMatchEnd");
                    return ExitMatch(); // 退出比赛
                case GameState.CONQUEST_POSTMATCH_LOSS_SCREEN:
                case GameState.CONQUEST_POSTMATCH_WIN_CONTINUE:
                case GameState.CONQUEST_POSTMATCH_WIN_TICKET:
                    Log("Conquest_Log_PostMatch");
                    return AcceptResult(); // 接受结果
                default:
                    if (attempts < 5)
                    {
                        _game.BlindReset();
                        return DetermineLoopEntryPoint(attempts + 1); // 再次尝试
                    }

                    Log("Log_LostBot");
                    Log("Log_LostBot_Restart");
                    Console.WriteLine();
                    Log("Menu_PressKeyToExit");
                    Console.ReadKey(); // 等待用户输入
                    Environment.Exit(0); // 退出程序
                    return false;
            }
        }

        private void RunMatchLoop()
        {
            // 开始日志记录：进入匹配循环
            Log("Log_Match_StartingLoop");
            var run = true;
            while (run)
            {
                // 尝试选择大厅，如果失败，跳转到相应的入口点
                if (!SelectLobby())
                {
                    DetermineLoopEntryPoint();
                    return;
                }

                // 验证“进入比赛”按钮是否存在，解决按钮消失的潜在问题
                Log("Conquest_Log_VerifyEntryButton");
                if (Check(_game.CanIdentifyConquestPlayBtn) || Check(() => _game.CanIdentifyConquestEntranceFee()))
                {
                    // 开始比赛并检查成功状态
                    var success = StartMatch();

                    // 如果未成功进入比赛，尝试调整入口点
                    if (!success)
                        success = DetermineLoopEntryPoint();

                    // 如果仍未成功，进行盲重置
                    if (!success)
                        _game.BlindReset();
                }
                else
                {
                    // 如果按钮不存在，盲重置并退出循环
                    _game.BlindReset();
                    run = false;
                }
            }
        }

        private bool SelectLobby()
        {
            // 等待加载完成
            Thread.Sleep(3000);
            var lobbyConfirmed = false;

            // 滑动界面，防止 UI 错位导致检测失误
            Log("Conquest_Log_LobbyReset");
            for (int x = 0; x < 3; x++)
            {
                var vertCenter = (_config.Window.Bottom - _config.Window.Top) / 2;
                SystemUtilities.Drag(
                    startX: _config.Window.Left + _config.Center + _config.Scale(250),
                    startY: _config.Window.Top + vertCenter,
                    endX: _config.Window.Left + _config.Center - _config.Scale(250),
                    endY: _config.Window.Top + vertCenter
                );
                Thread.Sleep(1000);
            }

            // 循环尝试确认合适的大厅
            Log("Conquest_Log_Menu_LobbyChoice", replace: [new("%VALUE%", _maxTier.ToString())]);
            for (int x = 0; x < 4 && !lobbyConfirmed; x++)
            {
                var selectedTier = _game.DetermineConquestLobbyTier();
                Log("Conquest_Log_Menu_SelectedTier", replace: [new("%VALUE%", selectedTier.ToString())]);
                Log("Conquest_Log_Check_Tickets", true);
                // 检查是否符合选中大厅的条件
                if ((selectedTier <= _maxTier && !Check(_game.CanIdentifyConquestNoTickets)) || selectedTier == GameState.CONQUEST_LOBBY_PG)
                    lobbyConfirmed = true;
                else
                {
                    // 若不符合条件，滑动到其他大厅
                    _config.GetWindowPositions();
                    var vertCenter = (_config.Window.Bottom - _config.Window.Top) / 2;
                    SystemUtilities.Drag(
                        startX: _config.Window.Left + _config.Center - _config.Scale(250),
                        startY: _config.Window.Top + vertCenter,
                        endX: _config.Window.Left + _config.Center + _config.Scale(250),
                        endY: _config.Window.Top + vertCenter
                    );
                    Thread.Sleep(1000);
                }
            }

            return lobbyConfirmed;
        }

        private bool StartMatch()
        {
            // 尝试进入比赛
            Log("Conquest_Log_EnteringLobby");
            _game.ClickPlay();
            Thread.Sleep(4000);

            // 再次点击确认按钮确保进入比赛
            Log("Log_Match_StartNew");
            _game.ClickPlay();
            Thread.Sleep(1000);
            _game.ClickPlay(); 
            // Press a second time just to be sure
            Thread.Sleep(1000);

            // 确认选择的牌组
            Log("Conquest_Log_ConfirmDeck");
            SystemUtilities.Click(_config.Window.Left + _config.Center + _config.Scale(100), _config.Window.Bottom - _config.Scale(345));
            Thread.Sleep(1000);

            // 等待匹配成功
            return WaitForMatchmaking();
        }

        private bool WaitForMatchmaking()
        {
            // 获取窗口位置以更新坐标
            _config.GetWindowPositions();

            // 初始化匹配计时器
            var mmTimer = new Stopwatch();
            mmTimer.Start();

            Log("Log_Check_Matchmaking", true);
            // 循环检测是否仍在匹配中
            while (Check(() => _game.CanIdentifyConquestMatchmaking()))
            {
                // 如果匹配时间超过10分钟，视为挂起，取消匹配
                if (mmTimer.Elapsed.TotalSeconds > 600)
                {
                    Log("Log_Check_Matchmaking_Hanged");
                    _game.ClickCancel();
                    return true;
                }

                // 每隔5秒检查一次匹配状态
                Log("Log_Matchmaking_Waiting", replace: [new("%ELAPSED%", mmTimer.Elapsed.ToString())]);
                Thread.Sleep(5000);
                _config.GetWindowPositions();
            }

            // 匹配成功，进入比赛
            return PlayMatch();
        }

        private bool PlayMatch()
        {
            // 开始日志记录：正在进行比赛
            Log("Log_Match_Playing");
            Thread.Sleep(1000);
            // 激活比赛状态
            var active = true;
            _game.ClickSnap();

            // 启动比赛计时器
            _matchTimer = new Stopwatch();
            _matchTimer.Start();

            var currentTurn = 0;

            // 主循环，控制比赛逻辑并限制最大比赛时间10分钟
            while (active && _matchTimer.Elapsed.Minutes < 10)
            {
                _config.GetWindowPositions();

                Log("Log_Check_ActiveMatch", true);
                // 检查比赛是否仍处于活跃状态
                if (!Check(_game.CanIdentifyActiveConquestMatch))
                {
                    var check = false;
                    // 尝试重新检测比赛状态，最多两次
                    for (int x = 1; x < 3 && !check; x++)
                    {
                        Log("Log_Check_ActiveMatch_Failed");
                        _config.GetWindowPositions();
                        _game.ResetClick();
                        check = Check(_game.CanIdentifyActiveConquestMatch);
                        Thread.Sleep(2000);
                    }

                    // 如果仍然无法检测到活跃比赛状态，退出比赛循环
                    active = check;
                }
                else
                {
                    // 如果当前回合数达到设定的回合限制，触发退赛逻辑
                    if (currentTurn++ >= _retreatAfterTurn)
                    {
                        Log("Log_Match_ReachedTurnLimit", replace: [new("%VALUE%", _retreatAfterTurn.ToString())]);
                        _game.ClickRetreat();
                        Thread.Sleep(1000);

                        Log("Conquest_Log_Match_Concede");
                        _game.ClickConcede();
                        Thread.Sleep(1000);
                    }
                    else
                    {
                        // 否则，继续进行比赛逻辑，执行出牌并结束回合
                        Log("Log_Match_PlayingCards", replace: [new("%VALUE%", currentTurn.ToString())]);
                        _game.PlayHand();
                        Thread.Sleep(1000);

                        Log("Log_Check_EnergyState", true);
                        // 检查是否有剩余能量，如果有，继续出牌
                        if (!Check(_game.CanIdentifyZeroEnergy))
                        {
                            Log("Log_Match_LeftoverEnergy");
                            _game.PlayHand();
                        }

                        // 结束当前回合
                        Log("Log_Match_EndTurn");
                        _game.ClickNext();
                        Thread.Sleep(1000);

                        _config.GetWindowPositions();

                        // 等待对方完成回合
                        Log("Log_Check_TurnState", true);
                        while (Check(() => _game.CanIdentifyMidTurn()))
                        {
                            Log("Log_Match_WaitingForTurn");
                            Thread.Sleep(4000);
                            _config.GetWindowPositions();
                        }
                    }
                }
            }

            _config.GetWindowPositions();

            // 如果比赛时间超过10分钟且可以退赛，则执行退赛逻辑
            Log("Log_Check_RetreatButton", true);
            if (_matchTimer.Elapsed.Minutes > 10 && Check(() => _game.CanIdentifyConquestRetreatBtn()))
            {
                Log("Conquest_Log_Match_Concede");
                _game.ClickRetreat();
                Thread.Sleep(3000);
            }

            // 检查是否需要退赛确认
            Log("Log_Check_Concede", true);
            if (Check(() => _game.CanIdentifyConquestConcede()))
                return ProgressRound();

            // 检查比赛是否结束，若已结束，则退出比赛
            Log("Log_Check_MatchEnd", true);
            if (Check(() => _game.CanIdentifyConquestMatchEnd()))
                return ExitMatch();

            return false;
        }

        private bool ProgressRound()
        {
            // 日志记录：检测到比赛回合结束
            Log("Conquest_Log_DetectedRoundEnd");
            _game.ClickNext();

            // 更新窗口位置以适应新的状态
            _config.GetWindowPositions();
            var waitTime = 0;
            Log("Log_Check_MatchState", true);
            // 等待进入下一回合或比赛结束
            while (!Check(_game.CanIdentifyActiveConquestMatch) && !Check(() => _game.CanIdentifyConquestMatchEnd()))
            {
                // 如果等待时间超过2秒，执行盲重置并返回循环入口
                if (waitTime >= 2000)
                {
                    Log("Conquest_Log_MaxWaitTimeReached", replace: [new("%VALUE%", "30")]);
                    _game.BlindReset();
                    return DetermineLoopEntryPoint();
                }

                // 每次等待1秒后刷新窗口位置
                Thread.Sleep(1000);
                waitTime += 1000;
                _config.GetWindowPositions();
            }

            // 成功进入下一回合或比赛继续
            return PlayMatch();
        }

        private bool ExitMatch()
        {
            // 日志记录：退出比赛
            Log("Log_Match_Exiting");
            // 更新窗口位置
            _config.GetWindowPositions();

            // 检查是否处于比赛后屏幕状态
            Log("Log_Check_PostMatchScreen", true);
            // 循环检查比赛后界面是否加载完成，并点击"下一步"按钮
            while (Check(() => _game.CanIdentifyConquestMatchEndNext1()) || Check(() => _game.CanIdentifyConquestMatchEndNext2()))
            {
                _game.ClickNext();
                Thread.Sleep(4000);
                _config.GetWindowPositions();
            }

            // 等待4秒进入比赛后菜单
            Log("Conquest_Log_Menu_WaitingPostMatch");
            Thread.Sleep(4000);

            var totalSleep = 0;
            // 检查是否已进入比赛后状态
            Log("Log_Check_PostMatchScreen", true);
            while (!Check(_game.CanIdentifyConquestLossContinue) && !Check(_game.CanIdentifyConquestWinNext) && !Check(_game.CanIdentifyConquestPlayBtn))
            {
                Thread.Sleep(2000);
                totalSleep += 2000;
                // 更新窗口位置并检查状态
                _config.GetWindowPositions();

                Log("Conquest_Log_Check_AnyLobby", true);
                // 如果检测到任何大厅状态，认为成功退出比赛
                if (totalSleep > 2500 && Check(_game.CanIdentifyAnyConquestLobby))
                {
                    Log("Conquest_Log_Menu_DetectedLobby");
                    return true;
                }

                // 如果等待超过4秒，返回循环入口以重新调整状态
                if (totalSleep > 4000)
                {
                    Log("Conquest_Log_MaxWaitTimeReached", replace: [new("%VALUE%", "30")]);
                    return DetermineLoopEntryPoint();
                }
            }

            // 如果检测到比赛结果界面，处理相关逻辑
            return AcceptResult();
        }

        private bool AcceptResult()
        {
            // 日志记录：处理比赛后结果
            Log("Conquest_Log_Match_ProcessingPostMatch");

            // 更新窗口位置
            _config.GetWindowPositions();
            Log("Conquest_Log_Check_Screens");
            // 检查是否需要继续点击"下一步"按钮来推进比赛后流程
            if (Check(_game.CanIdentifyConquestLossContinue) || Check(_game.CanIdentifyConquestWinNext))
            {
                Log("Log_ClickNext");
                _game.ClickPlay();
                Thread.Sleep(3000);
                _config.GetWindowPositions();
                // 递归调用以确保完全处理比赛后流程
                return AcceptResult();
            }
            else if (Check(() => _game.CanIdentifyConquestTicketClaim()))
            {
                // 检测到奖励领取界面，执行领取操作
                Log("Conquest_Log_Match_ProcessingPostMatch");
                _game.ClickClaim();
            }

            // 所有比赛后流程处理完成
            return true;
        }
    }
}
