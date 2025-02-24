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
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace BoosterBot
{
    public partial class MainWindow : Window
    {
        private CancellationTokenSource _globalCts;
        private IBoosterBot _currentBot;
        private bool _isRunning;
        private static string appSettingsPath = "appsettings.json";
        private static bool _updateAvailable = false;
        private static LocalizationManager _localizer;
        private static IConfiguration _configuration;

        public MainWindow()
        {
            InitializeComponent();
            InitializeLocalization();
            LoadAppSettings();
            HotkeyManager.Initialize(_localizer, this.Dispatcher); // 传递当前窗口的Dispatcher
            HotkeyManager.StopRequested += StopBot;
            HotkeyManager.PauseStateChanged += OnPauseStateChanged;
            LanguageComboBox.SelectionChanged += LanguageComboBox_SelectionChanged;
            ModeComboBox.SelectionChanged += ModeComboBox_SelectionChanged;
            EventComboBox.SelectionChanged += EventComboBox_SelectionChanged;
            ConquestModeComboBox.SelectionChanged += ConquestModeComboBox_SelectionChanged;
            RoundComboBoxGeneral.SelectionChanged += RoundComboBoxGeneral_SelectionChanged;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            HotkeyManager.Pause();
        }
        private void OnPauseStateChanged(bool isPaused)
        {
            Dispatcher.Invoke(() =>
            {
                if (isPaused)
                {
                    StartButton.Content = "Pausing";
                }
                else
                {
                    StartButton.Content = "Resuming";
                    // 可选：稍后恢复为"Start" 当任务真正恢复时
                }
            });
        }
        private static void InitializeLocalization()
        {
            try
            {
                // 确保 _configuration 已初始化
                if (_configuration == null)
                {
                    _configuration = new ConfigurationBuilder()
                        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                        .Build();
                }

                // 初始化 LocalizationManager
                _localizer = new LocalizationManager(_configuration);
            }
            catch (Exception ex)
            {
                // 如果初始化失败，记录错误日志
                Logger.Log(null, "Log_InitializationFailed", "logs/error.log", true);
                _localizer = new LocalizationManager(new ConfigurationBuilder().Build()); // 使用空的 IConfiguration
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void Minimize_Click(object sender, RoutedEventArgs e) =>
            WindowState = WindowState.Minimized;

        private void Maximize_Click(object sender, RoutedEventArgs e) =>
            WindowState = WindowState == WindowState.Normal ? WindowState.Maximized : WindowState.Normal;

        private void Close_Click(object sender, RoutedEventArgs e) =>
            Close();

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

        private void ModeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedMode = (ModeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString()?.ToLower();
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
            if (!string.IsNullOrEmpty(selectedMode))
            {
                UpdateAppSettings("defaultRunSettings:GameMode", selectedMode);
                UpdateLogPanel($"Game mode set to {selectedMode}");
            }
        }

        private void EventComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedValue = EventComboBox.SelectedValue?.ToString();
            if (!string.IsNullOrEmpty(selectedValue))
            {
                UpdateAppSettings("eventModeActive", selectedValue);
                UpdateLogPanel($"Event mode set to {selectedValue}");
            }
        }

        private void LoadAppSettings()
        {
            if (File.Exists(appSettingsPath))
            {
                try
                {
                    var json = File.ReadAllText(appSettingsPath);
                    var jsonObj = JObject.Parse(json);
                    var appLanguage = jsonObj["appLanguage"]?.ToString();
                    if (appLanguage == "en-US")
                        LanguageComboBox.SelectedValue = "en-US";
                    else if (appLanguage == "zh-CN")
                        LanguageComboBox.SelectedValue = "zh-CN";

                    var gameLanguage = jsonObj["gameLanguage"]?.ToString();
                    if (gameLanguage == "en-US")
                        LanguageComboBox.SelectedValue = "en-US";
                    else if (gameLanguage == "zh-CN")
                        LanguageComboBox.SelectedValue = "zh-CN";

                    var GameMode = jsonObj["defaultRunSettings"]?["GameMode"]?.ToString();
                    if (GameMode == "ladder")
                        ModeComboBox.SelectedValue = "Ladder";
                    else if (GameMode == "conquest")
                        ModeComboBox.SelectedValue = "Conquest";
                    else if (GameMode == "event")
                        ModeComboBox.SelectedValue = "Event";

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

        private void ResetUIStateToDefaults()
        {
            // 恢复下拉框的默认选项
            LanguageComboBox.SelectedValue = "en-US";  // 设置为默认语言
            ModeComboBox.SelectedValue = "Ladder";  // 设置为默认模式
            EventComboBox.SelectedValue = "false";  // 设置事件模式为默认

            // 你可以在这里继续添加更多的下拉框恢复逻辑
            ConquestModeComboBox.SelectedValue = null;  // 清空征服模式选项
            RoundComboBoxGeneral.SelectedValue = null;  // 清空回合选项

            // 其他状态的恢复，比如按钮文本等
            UpdateLogPanel("UI reset to defaults.");
        }
        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                if (_isRunning)
                {
                    PauseBot();
                    StartButton.Content = "Pausing";
                    return;
                }

                if (StartButton.Content.ToString() == "Pausing")  
                {
                    ResetUIStateToDefaults();  // 调用一个方法恢复 UI 状态
                    StartButton.Content = "Start";  // 按钮文本恢复为 "Start"
                    return;
                }
                // 启动新脚本
                _isRunning = true;
                StartButton.Content = "Running";  // 更新按钮文本为 "Running"
                _globalCts = new CancellationTokenSource();

                var GameMode = _configuration["defaultRunSettings:GameMode"]?.ToLower();
                if (string.IsNullOrWhiteSpace(GameMode) || !new[] { "ladder", "conquest", "event" }.Contains(GameMode))
                {
                    UpdateLogPanel("Error: Invalid mode selection.");
                    return;
                }

                string? maxConquestTier = null;
                if (GameMode == "conquest")
                {
                    maxConquestTier = _configuration["defaultRunSettings:maxConquestTier"];
                }

                var maxTurns = int.TryParse(_configuration["defaultRunSettings:maxRankedTurns"], out int turns) ? turns : 0;
                var shouldSurrender = _configuration["defaultRunSettings:surrenderGame"] == "Yes";
                bool autoplay = bool.Parse(_configuration["defaultRunSettings:autoplay"] ?? "true");
                bool saveScreens = bool.Parse(_configuration["defaultRunSettings:saveScreens"] ?? "false");
                bool repair = bool.Parse(_configuration["defaultRunSettings:repair"] ?? "false");
                bool? verbose = bool.TryParse(_configuration["verboseLogs"], out var v) ? v : null;
                bool? downscaled = bool.TryParse(_configuration["downscaledMode"], out var ds) ? ds : null;
                bool? ltm = bool.TryParse(_configuration["eventModeActive"], out var lt) ? lt : null;
                double? scaling = double.TryParse(_configuration["scaling"], out var scale) ? scale : null;

                if (!Directory.Exists("screens")) Directory.CreateDirectory("screens");
                if (!Directory.Exists("logs")) Directory.CreateDirectory("logs");

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

                _currentBot = GameMode switch
                {
                    "conquest" => new ConquestBot(config, retreat, selectedGameState, shouldSurrender),
                    "ladder" => new LadderBot(config, retreat),
                    "event" => new EventBot(config, retreat),
                    "repair" => new RepairBot(config),
                    _ => throw new Exception(_localizer.GetString("Log_InvalidModeSelection"))
                };

                HotkeyManager.StartBackgroundTask();
                await Task.Run(() => _currentBot.RunAsync(_globalCts.Token), _globalCts.Token);
            }
            catch (OperationCanceledException)
            {
                UpdateLogPanel("Operation canceled");
            }
            catch (Exception ex)
            {
                UpdateLogPanel("Error starting BoosterBot: " + ex.Message);
            }
            finally
            {
                ResetUIState();
            }
        }

        private void PauseBot()
        {
            _globalCts?.Cancel();
            _currentBot?.Cancel();
            UpdateLogPanel("Bot pausing");
        }
        private void StopBot()
        {
            _globalCts?.Cancel();
            _currentBot?.Stop();
            HotkeyManager.Stop();

            Dispatcher.Invoke(() =>
            {
                UpdateLogPanel("Bot stopped");
                StartButton.Content = "Stopped";
                StartButton.IsEnabled = true;

                // 获取当前应用程序的目录
                string currentDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                string baseDirectory = Path.GetDirectoryName(currentDirectory);  // 当前目录的父目录
                string targetFileName = "BoosterBot.exe";

                // 查找包含BoosterBot.exe的文件夹
                var targetFolderPath = Directory.GetDirectories(baseDirectory)
                                                 .FirstOrDefault(dir => Directory.GetFiles(dir, targetFileName).Any());

                if (targetFolderPath != null)
                {
                    string appPath = Path.Combine(targetFolderPath, targetFileName);
                    if (File.Exists(appPath))
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = appPath,
                            UseShellExecute = true
                        });

                        Application.Current.Shutdown();
                    }
                    else
                    {
                        Console.WriteLine("文件未找到: " + appPath);
                    }
                }
                else
                {
                    Console.WriteLine("未找到目标文件夹包含BoosterBot.exe！");
                }
            });

        }

        private void ResetUIState()
        {
            Dispatcher.Invoke(() =>
            {
                _isRunning = false;
                StartButton.Content = "Start";  // 在任务完成或取消后，按钮文本变回 "Start"
                StartButton.IsEnabled = true;    // 使按钮恢复为可点击状态
            });
        }


        private bool ConfirmMaxTierSelection(GameState maxState)
        {
            var result = MessageBox.Show($"Are you sure you want to select {maxState} as the maximum tier?", "Confirm Selection", MessageBoxButton.YesNo);
            return result == MessageBoxResult.Yes;
        }

        private string lastLogMessage = string.Empty;
        private void UpdateLogPanel(string LogMessage)
        {
            // 添加线程调度检查
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => UpdateLogPanel(LogMessage));
                return;
            }
            TextBox logTextBox = new TextBox
            {
                Text = LogMessage,
                Foreground = System.Windows.Media.Brushes.White,
                Margin = new Thickness(0, 2, 0, 2),
                IsReadOnly = true,
                Background = System.Windows.Media.Brushes.Transparent,
                BorderBrush = System.Windows.Media.Brushes.Transparent,
                TextWrapping = TextWrapping.Wrap,
                FontSize = 12
            };
            if (LogMessage == lastLogMessage)
                return;

            lastLogMessage = LogMessage;
            LogTextBox.AppendText(LogMessage + Environment.NewLine);
            LogScrollViewer.ScrollToEnd();
        }
    }
}