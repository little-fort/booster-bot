using BoosterBot.Helpers;
using BoosterBot.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BoosterBot
{
    public partial class MainWindow : Window
    {
        private static string appSettingsPath = "appsettings.json";
        private static bool _updateAvailable = false;
        private static LocalizationManager _localizer;
        private static IConfiguration _configuration;

        public MainWindow()
        {
            InitializeComponent();
            InitializeLocalization(); 
            LoadAppSettings();
            HotkeyManager.Initialize(_localizer);
            Logger.OnLogUpdated += UpdateLogPanel;
            LanguageComboBox.SelectionChanged += LanguageComboBox_SelectionChanged;
            ModeComboBox.SelectionChanged += ModeComboBox_SelectionChanged;
            EventComboBox.SelectionChanged += EventComboBox_SelectionChanged;
            ConquestModeComboBox.SelectionChanged += ConquestModeComboBox_SelectionChanged;
            RoundComboBoxGeneral.SelectionChanged += RoundComboBoxGeneral_SelectionChanged;
        }
        private void Window_Closed(object sender, EventArgs e)
        {
            // 调用公开的 Stop 方法来清理
            HotkeyManager.Stop();
        }

        static void InitializeLocalization()
        {
            _configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();
            _localizer = new LocalizationManager(_configuration);
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
                UpdateLogPanel($"Language set to {selectedLanguage}");
            }
        }

        // 游戏模式选择事件
        private void ModeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedMode = (ModeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString()?.ToLower();

            // 根据选择的游戏模式显示/隐藏征服模式设置
            if (selectedMode == "conquest")
            {
                ConquestModeTitle.Visibility = Visibility.Visible;
                ConquestModeComboBox.Visibility = Visibility.Visible;
            }
            else
            {
                ConquestModeTitle.Visibility = Visibility.Collapsed;
                ConquestModeComboBox.Visibility = Visibility.Collapsed;
            }

            // 更新 appsettings.json 中的游戏模式，确保是小写字母
            if (!string.IsNullOrEmpty(selectedMode))
            {
                UpdateAppSettings("defaultRunSettings:GameMode", selectedMode);
                UpdateLogPanel($"Game mode set to {selectedMode}");
            }
        }

        // Event 模式选择事件
        private void EventComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedValue = EventComboBox.SelectedValue?.ToString();
            if (!string.IsNullOrEmpty(selectedValue))
            {
                UpdateAppSettings("eventModeActive", selectedValue);
                UpdateLogPanel($"Event mode set to {selectedValue}");
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
                    var GameMode = jsonObj["defaultRunSettings"]?["GameMode"]?.ToString();
                    if (GameMode == "ladder")
                        ModeComboBox.SelectedValue = "Ladder";
                    else if (GameMode == "conquest")
                        ModeComboBox.SelectedValue = "Conquest";
                    else if (GameMode == "event")
                        ModeComboBox.SelectedValue = "Event";

                    // 设置 Event 模式
                    var eventModeActive = jsonObj["eventModeActive"]?.ToString();
                    if (eventModeActive == "true")
                        EventComboBox.SelectedValue = "true";
                    else
                        EventComboBox.SelectedValue = "false";
                }
                catch (Exception ex)
                {
                    UpdateLogPanel("Error loading appsettings.json: " + ex.Message);
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
                UpdateLogPanel("Error updating appsettings.json: " + ex.Message);
            }
        }

        private void RoundComboBoxGeneral_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedTurns = (RoundComboBoxGeneral.SelectedItem as ComboBoxItem)?.Content.ToString();
            if (!string.IsNullOrEmpty(selectedTurns))
            {
                UpdateAppSettings("defaultRunSettings:maxRankedTurns", selectedTurns);
                UpdateLogPanel($"Max ranked turns set to {selectedTurns}");
            }
        }

        private void ConquestModeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedTier = (ConquestModeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            if (!string.IsNullOrEmpty(selectedTier))
            {
                UpdateAppSettings("defaultRunSettings:maxConquestTier", selectedTier.ToLower());
                UpdateLogPanel($"Conquest tier set to {selectedTier}");
            }
        }

        // 点击 Start 按钮时的事件
        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateLogPanel("BoosterBot started!");

            // 从 appsettings.json 获取游戏模式
            var GameMode = _configuration["defaultRunSettings:GameMode"]?.ToLower();  // 确保读取的是小写模式值

            // 确保在 appsettings.json 中选择的游戏模式值是合法的
            if (string.IsNullOrWhiteSpace(GameMode) || !new[] { "ladder", "conquest", "event" }.Contains(GameMode))
            {
                UpdateLogPanel("Error: Invalid mode selection.");
                return;
            }

            string? maxConquestTier = null;

            // 只有在征服模式下，maxConquestTier 才能有值
            if (GameMode == "conquest")
            {
                maxConquestTier = _configuration["defaultRunSettings:maxConquestTier"];  // 获取征服模式层级
            }

            // 获取其他配置项
            var maxTurns = int.TryParse(_configuration["defaultRunSettings:maxRankedTurns"], out int turns) ? turns : 0;  // 获取回合数
            var shouldSurrender = _configuration["defaultRunSettings:surrenderGame"] == "Yes";  // 是否放弃游戏

            // 其他配置项
            bool masked = bool.Parse(_configuration["defaultRunSettings:masked"] ?? "false");
            bool autoplay = bool.Parse(_configuration["defaultRunSettings:autoplay"] ?? "true");
            bool saveScreens = bool.Parse(_configuration["defaultRunSettings:saveScreens"] ?? "false");
            bool repair = bool.Parse(_configuration["defaultRunSettings:repair"] ?? "false");
            bool? verbose = bool.TryParse(_configuration["verboseLogs"], out var v) ? v : null;
            bool? downscaled = bool.TryParse(_configuration["downscaledMode"], out var ds) ? ds : null;
            bool? ltm = bool.TryParse(_configuration["eventModeActive"], out var lt) ? lt : null;
            double? scaling = double.TryParse(_configuration["scaling"], out var scale) ? scale : null;

            try
            {
                if (!Directory.Exists("screens"))
                    Directory.CreateDirectory("screens");

                if (!Directory.Exists("logs"))
                    Directory.CreateDirectory("logs");


                // 检查更新
                _updateAvailable = await UpdateChecker.CheckForUpdates();

                GameState selectedGameState = GameState.UNKNOWN;
                var retreat = maxTurns > 0 || repair ? maxTurns : 0;

                if (GameMode == "conquest")
                {
                    var maxTier = (ConquestModeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
                    if (!string.IsNullOrEmpty(maxTier))
                    {
                        switch (maxTier)
                        {
                            case "Proving Ground":
                                selectedGameState = GameState.CONQUEST_LOBBY_PG;
                                break;
                            case "Silver":
                                selectedGameState = GameState.CONQUEST_LOBBY_SILVER;
                                break;
                            case "Gold":
                                selectedGameState = GameState.CONQUEST_LOBBY_GOLD;
                                break;
                            case "infinite":
                                selectedGameState = GameState.CONQUEST_LOBBY_INFINITE;
                                break;
                            default:
                                selectedGameState = GameState.UNKNOWN;
                                break;
                        }

                        if (selectedGameState > GameState.CONQUEST_LOBBY_PG)
                        {
                            if (!ConfirmMaxTierSelection(selectedGameState))
                            {
                                return;
                            }
                        }
                    }
                    else
                    {
                        UpdateLogPanel("Error: No conquest tier selected.");
                        return;
                    }
                }

                var logPath = $"logs\\{GameMode}-log-{DateTime.Now:yyyyMMddHHmmss}.txt";
                var config = new BotConfig(_configuration, _localizer, (double)scaling, (bool)verbose, autoplay, saveScreens, logPath, (bool)ltm, (bool)downscaled);

                IBoosterBot bot = GameMode switch
                {
                    "conquest" => new ConquestBot(config, retreat, selectedGameState, shouldSurrender),
                    "ladder" => new LadderBot(config, retreat),
                    "event" => new EventBot(config, retreat),
                    "repair" => new RepairBot(config),
                    _ => throw new Exception(_localizer.GetString("Log_InvalidModeSelection"))
                };
                // 启动后台任务
                HotkeyManager.StartBackgroundTask();
                try
                {
                    bot.Run();
                }
                catch (Exception ex)
                {
                    UpdateLogPanel(_localizer.GetString("Log_FatalError"));
                    UpdateLogPanel(ex.Message);
                    UpdateLogPanel(ex.StackTrace);
                }
            }
            catch (Exception ex)
            {
                UpdateLogPanel("Error starting BoosterBot: " + ex.Message);
            }
        }

        // 确认最大层级选择
        private bool ConfirmMaxTierSelection(GameState maxState)
        {
            var result = MessageBox.Show($"Are you sure you want to select {maxState} as the maximum tier?", "Confirm Selection", MessageBoxButton.YesNo);
            return result == MessageBoxResult.Yes;
        }

        // 添加日志
        private string lastLogMessage = string.Empty; // 用于存储上一条日志消息
        private void UpdateLogPanel(string LogMessage)
        {
            // 创建一个新的 TextBox 来显示日志信息
            TextBox logTextBox = new TextBox
            {
                Text = LogMessage,
                Foreground = System.Windows.Media.Brushes.White,
                Margin = new Thickness(0, 2, 0, 2),
                IsReadOnly = true, // 禁止编辑
                Background = System.Windows.Media.Brushes.Transparent, // 透明背景
                BorderBrush = System.Windows.Media.Brushes.Transparent, // 不显示边框
                TextWrapping = TextWrapping.Wrap,
                FontSize = 12
            };
            // 如果当前日志与上一条日志相同，则跳过
            if (LogMessage == lastLogMessage)
                return;

            // 更新上一条日志为当前日志
            lastLogMessage = LogMessage;

            // 将新的日志消息追加到现有文本
            LogTextBox.AppendText(LogMessage + Environment.NewLine);

            // 滚动到最新日志
            LogScrollViewer.ScrollToEnd();
        }
    }
}