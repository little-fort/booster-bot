using BoosterBot.Helpers;
using BoosterBot;
using BoosterBot.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;
using System;
using System.Collections.Generic;

namespace BoosterBot
{
    public partial class MainWindow : Window
    {
        private string appSettingsPath = "appsettings.json";
        private LocalizationManager _localizer;
        private IConfiguration _configuration;
        private bool _updateAvailable = false;

        // 用于追踪已记录的日志
        private HashSet<string> loggedMessages = new HashSet<string>();

        public MainWindow()
        {
            InitializeComponent();
            InitializeLocalization(); // 初始化本地化
            LoadAppSettings(); // 初始化时加载配置

            // 初始化 UI 组件的事件
            LanguageComboBox.SelectionChanged += LanguageComboBox_SelectionChanged;
            ModeComboBox.SelectionChanged += ModeComboBox_SelectionChanged;
            EventComboBox.SelectionChanged += EventComboBox_SelectionChanged;
            ConquestModeComboBox.SelectionChanged += ConquestModeComboBox_SelectionChanged;
            RoundComboBoxGeneral.SelectionChanged += RoundComboBoxGeneral_SelectionChanged;
        }

        // 窗口拖动事件
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        // 最小化窗口
        private void Minimize_Click(object sender, RoutedEventArgs e) =>
            WindowState = WindowState.Minimized;

        // 最大化或恢复窗口
        private void Maximize_Click(object sender, RoutedEventArgs e) =>
            WindowState = WindowState == WindowState.Normal ? WindowState.Maximized : WindowState.Normal;

        // 关闭窗口
        private void Close_Click(object sender, RoutedEventArgs e) =>
            Close();

        // 语言选择事件
        private void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedLanguage = LanguageComboBox.SelectedValue?.ToString();
            if (!string.IsNullOrEmpty(selectedLanguage))
            {
                UpdateAppSettings("appLanguage", selectedLanguage);
                UpdateAppSettings("gameLanguage", selectedLanguage);
                AddLog($"Language set to {selectedLanguage}");
            }
        }

        // 游戏模式选择事件
        private void ModeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedMode = (ModeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            if (selectedMode == "Conquest")
            {
                ConquestModeTitle.Visibility = Visibility.Visible;
                ConquestModeComboBox.Visibility = Visibility.Visible;
            }
            else
            {
                ConquestModeTitle.Visibility = Visibility.Collapsed;
                ConquestModeComboBox.Visibility = Visibility.Collapsed;
            }

            // 更新 appsettings.json 中的游戏模式
            UpdateAppSettings("defaultRunSettings:gameMode", selectedMode?.ToLower());
            AddLog($"Game mode set to {selectedMode}");
        }

        // Event 模式选择事件
        private void EventComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedValue = EventComboBox.SelectedValue?.ToString();
            if (!string.IsNullOrEmpty(selectedValue))
            {
                UpdateAppSettings("eventModeActive", selectedValue);
                AddLog($"Event mode set to {selectedValue}");
            }
        }

        // 点击 Start 按钮时的事件
        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            // 最小化窗口
            //WindowState = WindowState.Minimized;

            AddLog("BoosterBot started!");

            // 解析 UI 选择
            bool masked = true; // 默认启用 masked 模式
            bool autoplay = true;
            bool saveScreens = false;
            bool repair = false;
            bool? verbose = null;
            bool? downscaled = null;
            bool? ltm = null;
            double? scaling = null;
            bool shouldSurrender = (SurrenderComboBoxGeneral.SelectedItem as ComboBoxItem)?.Content.ToString() == "Yes";

            // 从 UI 获取游戏模式
            string? gameMode = (ModeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString()?.ToLower();
            string? maxConquestTier = (ConquestModeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            int? maxTurns = int.Parse((RoundComboBoxGeneral.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "0");

           
            // 启动 bot
            try
            {
                if (!Directory.Exists("screens"))
                    Directory.CreateDirectory("screens");

                if (!Directory.Exists("logs"))
                    Directory.CreateDirectory("logs");
                else
                    PurgeLogs();

                // 检查更新
                _updateAvailable = await UpdateChecker.CheckForUpdates();

                // 解析 appsettings.json 配置
                verbose ??= bool.Parse(_configuration["verboseLogs"] ?? "false");
                downscaled ??= bool.Parse(_configuration["downscaledMode"] ?? "false");
                ltm ??= bool.Parse(_configuration["eventModeActive"] ?? "false");
                scaling ??= double.Parse(_configuration["scaling"] ?? "1.0");

                if (!string.IsNullOrWhiteSpace(_configuration["defaultRunSettings:enabled"]) && bool.Parse(_configuration["defaultRunSettings:enabled"] ?? "false"))
                {
                    gameMode ??= _configuration["defaultRunSettings:gameMode"];
                    maxConquestTier ??= _configuration["defaultRunSettings:maxConquestTier"];
                    maxTurns ??= int.Parse(_configuration["defaultRunSettings:maxRankedTurns"] ?? "0");
                }

                // 处理游戏模式
                var mode = 0;
                if (repair)
                    mode = 9;
                else if (!string.IsNullOrWhiteSpace(gameMode))
                    mode = gameMode.ToLower() switch
                    {
                        "c" => 1,
                        "conquest" => 1,
                        "l" => 2,
                        "ladder" => 2,
                        "r" => 2,
                        "ranked" => 2,
                        _ => 0 // 默认值
                    };

                GameState maxTier = GameState.UNKNOWN;
                if (mode == 1)
                {
                    if (string.IsNullOrWhiteSpace(maxConquestTier))
                        maxTier = GameState.CONQUEST_LOBBY_PG; // 默认值
                    else
                        maxTier = maxConquestTier.ToLower() switch
                        {
                            "pg" => GameState.CONQUEST_LOBBY_PG,
                            "proving grounds" => GameState.CONQUEST_LOBBY_PG,
                            "s" => GameState.CONQUEST_LOBBY_SILVER,
                            "silver" => GameState.CONQUEST_LOBBY_SILVER,
                            "g" => GameState.CONQUEST_LOBBY_GOLD,
                            "gold" => GameState.CONQUEST_LOBBY_GOLD,
                            "i" => GameState.CONQUEST_LOBBY_INFINITE,
                            "infinite" => GameState.CONQUEST_LOBBY_INFINITE,
                            _ => GameState.CONQUEST_LOBBY_PG // 默认值
                        };
                }

                var retreat = maxTurns > 0 || repair ? maxTurns : 0; // 默认值

                // 初始化 bot
                var type = (GameMode)mode;
                var logPath = $"logs\\{type.ToString().ToLower()}-log-{DateTime.Now.ToString("yyyyMMddHHmmss")}.txt";
                var config = new BotConfig(_configuration, _localizer, (double)scaling, (bool)verbose, autoplay, saveScreens, logPath, (bool)ltm, (bool)downscaled);
                IBoosterBot bot = mode switch
                {
                    1 => new ConquestBot(config, retreat ?? 0, maxTier, shouldSurrender),  // 新增 shouldSurrender 参数
                    2 => new LadderBot(config, retreat ?? 0),
                    3 => new EventBot(config, retreat ?? 0),
                    9 => new RepairBot(config),
                    _ => throw new Exception(_localizer.GetString("Log_InvalidModeSelection"))
                };

                // 运行 bot
                try
                {
                    bot.Run();
                }
                catch (Exception ex)
                {
                    AddLog(_localizer.GetString("Log_FatalError"));
                    AddLog(ex.Message);
                    AddLog(ex.StackTrace);
                }
            }
            catch (Exception ex)
            {
                AddLog("Error starting BoosterBot: " + ex.Message);
            }
        }

        // 添加日志
        private void AddLog(string logMessage)
        {
            // 构造日志的格式
            string formattedMessage = $"[{DateTime.Now:HH:mm:ss}] {logMessage}";

            // 如果该日志没有被记录过，才添加
            if (!loggedMessages.Contains(formattedMessage))
            {
                // 标记日志为已记录
                loggedMessages.Add(formattedMessage);

                // 创建并配置 TextBox 控件来显示日志
                var logTextBox = new TextBox
                {
                    Text = formattedMessage,
                    Foreground = System.Windows.Media.Brushes.White,
                    Background = System.Windows.Media.Brushes.Transparent,
                    BorderThickness = new Thickness(0),
                    IsReadOnly = true, // 设置为只读
                    TextWrapping = TextWrapping.Wrap,
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                    HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                    FontSize = 12
                };

                // 将日志添加到 LogPanel 中
                LogPanel.Children.Add(logTextBox);
            }
        }

        // 加载 appsettings.json 配置
        private void LoadAppSettings()
        {
            if (File.Exists(appSettingsPath))
            {
                try
                {
                    var json = File.ReadAllText(appSettingsPath);
                    var jsonObj = JObject.Parse(json);

                    // 设置程序语言
                    var appLanguage = jsonObj["appLanguage"]?.ToString();
                    if (appLanguage == "en-US")
                        LanguageComboBox.SelectedValue = "en-US";
                    else if (appLanguage == "zh-CN")
                        LanguageComboBox.SelectedValue = "zh-CN";

                    // 设置游戏语言
                    var gameLanguage = jsonObj["gameLanguage"]?.ToString();
                    if (gameLanguage == "en-US")
                        LanguageComboBox.SelectedValue = "en-US";
                    else if (gameLanguage == "zh-CN")
                        LanguageComboBox.SelectedValue = "zh-CN";

                    // 设置游戏模式
                    var gameMode = jsonObj["defaultRunSettings"]?["gameMode"]?.ToString();
                    if (gameMode == "ranked")
                        ModeComboBox.SelectedValue = "ranked";
                    else if (gameMode == "conquest")
                        ModeComboBox.SelectedValue = "conquest";
                    else if (gameMode == "event")
                        ModeComboBox.SelectedValue = "event";

                    // 设置 Event 模式
                    var eventModeActive = jsonObj["eventModeActive"]?.ToString();
                    if (eventModeActive == "true")
                        EventComboBox.SelectedValue = "true";
                    else
                        EventComboBox.SelectedValue = "false";
                }
                catch (Exception ex)
                {
                    AddLog("Error loading appsettings.json: " + ex.Message);
                }
            }
        }

        // 更新 appsettings.json 配置
        private void UpdateAppSettings(string key, string value)
        {
            try
            {
                var json = File.ReadAllText(appSettingsPath);
                var jsonObj = JObject.Parse(json);
                var keys = key.Split(':');
                var tempObj = jsonObj;

                foreach (var k in keys.Take(keys.Length - 1))
                {
                    tempObj = (JObject)tempObj[k];
                }

                tempObj[keys.Last()] = value;
                File.WriteAllText(appSettingsPath, jsonObj.ToString());
            }
            catch (Exception ex)
            {
                AddLog("Error updating appsettings.json: " + ex.Message);
            }
        }

        private void RoundComboBoxGeneral_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedTurns = (RoundComboBoxGeneral.SelectedItem as ComboBoxItem)?.Content.ToString();
            if (!string.IsNullOrEmpty(selectedTurns))
            {
                UpdateAppSettings("defaultRunSettings:maxRankedTurns", selectedTurns);
                AddLog($"Max ranked turns set to {selectedTurns}");
            }
        }

        private void ConquestModeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedTier = (ConquestModeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            if (!string.IsNullOrEmpty(selectedTier))
            {
                UpdateAppSettings("defaultRunSettings:maxConquestTier", selectedTier.ToLower());
                AddLog($"Conquest tier set to {selectedTier}");
            }
        }

        // 清除过期日志
        private void PurgeLogs()
        {
            var logDirectory = new DirectoryInfo("logs");
            var logFiles = logDirectory.GetFiles("*.txt")
                                       .OrderByDescending(f => f.LastWriteTime)
                                       .Skip(10) // 保留最近的 10 个日志
                                       .ToList();

            foreach (var file in logFiles)
            {
                file.Delete();
            }
        }

        // 初始化本地化
        private void InitializeLocalization()
        {
            _configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();
            _localizer = new LocalizationManager(_configuration);
        }

        private void LogMessage(string message)
        {
            // 确保调用你的 AddLog 方法，不重复添加相同内容
            if (!string.IsNullOrEmpty(message))
            {
                AddLog(message);
            }
        }
    }
}
