using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BoosterBot.Helpers;
using BoosterBot.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;

namespace BoosterBot
{
    public partial class MainWindow : Window
    {
        private CancellationTokenSource _globalCts = null!;
        private IBoosterBot? _currentBot;
        private bool _isRunning;
        private static string appSettingsPath = "appsettings.json";
        private static LocalizationManager _localizer = null!;
        private static IConfiguration _configuration = null!;
        private void CheckFrameworkPrerequisites()
        {
            var missingComponents = new Dictionary<string, string>();
            if (Environment.Version.Major < 6)
            {
                missingComponents.Add(".NET 6 Desktop Runtime", "https://dotnet.microsoft.com/download/dotnet/6.0");
            }
            if (!IsVCRedistInstalled())
            {
                missingComponents.Add("VC++ 2015-2022 Redistributable", "https://aka.ms/vs/17/release/vc_redist.x64.exe");
            }

            if (missingComponents.Count == 0) return;
            var errorMessage = new StringBuilder()
                .AppendLine("缺少必要组件，即将打开下载页面：")
                .AppendLine(string.Join("\n", missingComponents.Keys.Select(k => $"• {k}")))
                .AppendLine("\n安装完成后请重新启动程序");
            var result = MessageBox.Show(
                errorMessage.ToString(),
                "系统组件缺失",
                MessageBoxButton.OKCancel,
                MessageBoxImage.Warning
            );
            if (result == MessageBoxResult.OK)
            {
                foreach (var url in missingComponents.Values)
                {
                    try
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = url,
                            UseShellExecute = true
                        });
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"无法打开链接：{url}\n错误：{ex.Message}",
                            "打开链接失败",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }
                }
            }

            Application.Current.Shutdown();
        }
        private bool IsVCRedistInstalled()
        {
            const string keyPath = @"SOFTWARE\Microsoft\VisualStudio\14.0\VC\Runtimes\x64";
            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(keyPath);
                return key?.GetValue("Installed")?.ToString() == "1" &&
                       int.TryParse(key.GetValue("Major")?.ToString(), out var major) &&
                       major >= 14;
            }
            catch { return false; }
        }
        public MainWindow()
        {
            CheckFrameworkPrerequisites();
            InitializeComponent();
            _configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();
            _localizer = new LocalizationManager(_configuration);
            AddLog("Application initialized");
            AddLog($"当前进程架构：{(Environment.Is64BitProcess ? "x64" : "x86")}");
            try
            {
                var assembly = typeof(OpenCvSharp.Mat).Assembly;
                string baseDirectory = GetSafeBaseDirectory(assembly);
                var dllPath = Path.Combine(baseDirectory, "OpenCvSharpExtern.dll");
                dllPath = Path.GetFullPath(dllPath);
                AddLog($"尝试加载OpenCV DLL：{dllPath}");

                if (!File.Exists(dllPath))
                {
                    HandleMissingDll(dllPath);
                    return;
                }
                NativeLibrary.Load(dllPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"OpenCV初始化失败：{ex.Message}", "严重错误",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
            InitializeLocalization();
            LoadAppSettings();
            HotkeyManager.Initialize(_localizer, this.Dispatcher); 
            HotkeyManager.StopRequested += StopBot;
            HotkeyManager.PauseStateChanged += OnPauseStateChanged;
            _globalCts = new CancellationTokenSource();
            LanguageComboBox.SelectionChanged += LanguageComboBox_SelectionChanged;
            EventComboBox.SelectionChanged += EventComboBox_SelectionChanged;
            ConquestModeComboBox.SelectionChanged += ConquestModeComboBox_SelectionChanged;
            RoundComboBoxGeneral.SelectionChanged += RoundComboBoxGeneral_SelectionChanged;
            if (bool.Parse(_configuration["firstRun"] ?? "true"))
            {
                ShowTutorial();
                UpdateAppSettings("firstRun", "false");
            }
        }

        private string GetSafeBaseDirectory(Assembly assembly)
        {
            if (!string.IsNullOrEmpty(assembly.Location))
            {
                var dir = Path.GetDirectoryName(assembly.Location);
                if (!string.IsNullOrEmpty(dir) )return dir;
            }
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            if (!string.IsNullOrEmpty(baseDir)) return baseDir;
            return Environment.CurrentDirectory ?? string.Empty;
        }

        private void HandleMissingDll(string path)
        {
            var errorDetails = new StringBuilder()
                .AppendLine("关键依赖缺失！")
                .AppendLine($"路径：{path}")
                .AppendLine("可能原因：")
                .AppendLine("1. 未正确安装 VC++ 可再发行组件")
                .AppendLine("2. 杀毒软件误删文件")
                .AppendLine("3. 程序未完整部署")
                .ToString();

            Dispatcher.Invoke(() =>
            {
                MessageBox.Show(
    errorDetails,
    "错误提示",
    MessageBoxButton.OK,
    MessageBoxImage.Error);
            });

            throw new FileNotFoundException(errorDetails);
        }

        private void HandleInitializationError(Exception ex)
        {
            var errorMsg = $"初始化失败：{ex.Message}";

            // 记录详细错误
            Logger.LogError(ex.ToString());

            // 显示友好提示
            Dispatcher.Invoke(() =>
            {
                MessageBox.Show(
                    "程序启动失败，请联系技术支持\n错误代码：INIT_001",
                    "严重错误",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            });

            Application.Current.Shutdown();
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
                }
            });
        }
        private static void InitializeLocalization()
        {
            try
            {
                if (_configuration == null)
                {
                    _configuration = new ConfigurationBuilder()
                        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                        .Build();
                }
                _localizer = new LocalizationManager(_configuration);
            }
            catch (Exception)
            {
                Logger.Log(null!, "Log_InitializationFailed", "logs/error.log", true);
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
                // 获取本地化的语言名称
                var languageName = _localizer.GetString($"Language_{selectedLanguage}");

                UpdateAppSettings("appLanguage", selectedLanguage);
                UpdateAppSettings("gameLanguage", selectedLanguage);
                UpdateLogPanel(string.Format(
                    _localizer.GetString("LanguageSet"), // "语言设置为：{0}"
                    languageName
                ));
            }
        }
        private void ModeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedMode = (ModeComboBox.SelectedItem as ComboBoxItem)?.Tag?.ToString();
            if (string.IsNullOrEmpty(selectedMode)) return;
            var localizedMode = _localizer.GetString($"GameMode_{selectedMode}");
            UpdateLogPanel(string.Format(_localizer.GetString("GameModeSelected"), localizedMode));
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
            UpdateAppSettings("defaultRunSettings:GameMode", selectedMode);
        }
        private void ConquestModeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedTier = (ConquestModeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            if (!string.IsNullOrEmpty(selectedTier))
            {
                UpdateAppSettings("defaultRunSettings:maxConquestTier", selectedTier.ToLower());

                // 获取本地化的层级名称
                var localizedTier = _localizer.GetString($"ConquestTier_{selectedTier.Replace(" ", "")}");
                UpdateLogPanel(string.Format(
                    _localizer.GetString("ConquestTierSelected"), // "征服模式最大层级选择为：{0}"
                    localizedTier
                ));
            }
        }
        
        private void EventComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedValue = EventComboBox.SelectedValue?.ToString();
            if (!string.IsNullOrEmpty(selectedValue))
            {
                // 获取本地化的开关状态
                var status = _localizer.GetString(selectedValue.ToLower() == "true" ? "Enabled" : "Disabled");

                UpdateLogPanel(string.Format(
                    _localizer.GetString("EventModeSet"), // "限时活动模式：{0}"
                    status
                ));
            }
        }
        private void RoundComboBoxGeneral_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedTurns = (RoundComboBoxGeneral.SelectedItem as ComboBoxItem)?.Content.ToString();
            if (!string.IsNullOrEmpty(selectedTurns))
            {
                UpdateAppSettings("defaultRunSettings:maxRankedTurns", selectedTurns);

                // 使用本地化资源
                string localizedValue;
                if (selectedTurns == "0")
                {
                    localizedValue = _localizer.GetString("NoRetreat"); // "不撤退"
                }
                else
                {
                    localizedValue = string.Format(
                        _localizer.GetString("RetreatAfterTurns"), // "撤退前要完成{0}回合"
                        selectedTurns
                    );
                }

                UpdateLogPanel(string.Format(
                    _localizer.GetString("RoundSelectionLog"), // "选择撤退设置：{0}"
                    localizedValue
                ));
            }
        }
        private void LoadAppSettings()
        {
            if (File.Exists(appSettingsPath))
            {
                try
                {
                    LanguageComboBox.SelectionChanged -= LanguageComboBox_SelectionChanged;
                    ModeComboBox.SelectionChanged -= ModeComboBox_SelectionChanged;
                    EventComboBox.SelectionChanged -= EventComboBox_SelectionChanged;
                    var json = File.ReadAllText(appSettingsPath);
                    var jsonObj = JObject.Parse(json);
                    var appLanguage = jsonObj["appLanguage"]?.ToString();
                    foreach (ComboBoxItem item in LanguageComboBox.Items)
                    {
                        if (item.Tag?.ToString() == appLanguage)
                        {
                            LanguageComboBox.SelectedItem = item;
                            break;
                        }
                    }
                    var gameMode = jsonObj["defaultRunSettings"]?["GameMode"]?.ToString();
                    foreach (ComboBoxItem item in ModeComboBox.Items)
                    {
                        if (item.Tag?.ToString() == gameMode)
                        {
                            ModeComboBox.SelectedItem = item;
                            // 如果是征服模式，显示相关控件
                            if (gameMode == "conquest")
                            {
                                ConquestModeTitle.Visibility = Visibility.Visible;
                                ConquestModeComboBox.Visibility = Visibility.Visible;
                            }
                            break;
                        }
                    }
                    var eventModeActive = jsonObj["eventModeActive"]?.ToString();
                    foreach (ComboBoxItem item in EventComboBox.Items)
                    {
                        if (item.Tag?.ToString() == eventModeActive)
                        {
                            EventComboBox.SelectedItem = item;
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    UpdateLogPanel("Error loading appsettings.json: " + ex.Message);
                }
                finally
                {
                    LanguageComboBox.SelectionChanged += LanguageComboBox_SelectionChanged;
                    ModeComboBox.SelectionChanged += ModeComboBox_SelectionChanged;
                    EventComboBox.SelectionChanged += EventComboBox_SelectionChanged;
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
                    if (tempObj[k] == null)
                    {
                        tempObj[k] = new JObject();
                    }
                    tempObj = (JObject)tempObj[k]!;
                }
                tempObj[keys.Last()] = value;
                File.WriteAllText(appSettingsPath, jsonObj.ToString());
            }
            catch (Exception ex)
            {
                UpdateLogPanel("Error updating appsettings.json: " + ex.Message);
            }
        }
        
        private void ResetUIStateToDefaults()
        {
            LanguageComboBox.SelectedValue = "en-US"; 
            ModeComboBox.SelectedValue = "Ladder";
            EventComboBox.SelectedValue = "false";
            ConquestModeComboBox.SelectedValue = null;
            RoundComboBoxGeneral.SelectedValue = 0;
            UpdateLogPanel("UI reset to defaults.");
        }
        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckDependencies())
            {
                UpdateLogPanel("缺少必要依赖文件");
                return;
            }
            if ((ModeComboBox.SelectedItem as ComboBoxItem)?.Tag?.ToString() == "repair")
                return;
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
                    ResetUIStateToDefaults();
                    StartButton.Content = "Start";
                    return;
                }
                _isRunning = true;
                StartButton.Content = "Running";
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
                int maxTurns = 0;
                string configKey = "defaultRunSettings:maxRankedTurns";
                if (!int.TryParse(_configuration[configKey], out maxTurns) || maxTurns < 0)
                {
                    UpdateLogPanel($"Invalid value for '{configKey}'. Must be a non-negative integer. Defaulting to 0.");
                    maxTurns = 0;
                }
                bool autoplay = bool.Parse(_configuration["defaultRunSettings:autoplay"] ?? "true");
                bool saveScreens = bool.Parse(_configuration["defaultRunSettings:saveScreens"] ?? "false");
                bool repair = bool.Parse(_configuration["defaultRunSettings:repair"] ?? "false");
                bool? verbose = bool.TryParse(_configuration["verboseLogs"], out var v) ? v : null;
                bool? downscaled = bool.TryParse(_configuration["downscaledMode"], out var ds) ? ds : null;
                bool? ltm = bool.TryParse(_configuration["eventModeActive"], out var lt) ? lt : null;
                double? scaling = double.TryParse(_configuration["scaling"], out var scale) ? scale : null;
                if (!Directory.Exists("screens")) Directory.CreateDirectory("screens");
                if (!Directory.Exists("logs")) Directory.CreateDirectory("logs");
                GameState selectedGameState = GameState.UNKNOWN;
                var retreat = maxTurns > 0 || repair ? maxTurns : 0;
                if (GameMode == "conquest")
                {
                    var maxTier = (ConquestModeComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? string.Empty;
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
                            case "Infinite":
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
                        UpdateLogPanel("Error: Invalid conquest tier selection");
                        return;
                    }
                }
                var logPath = $"logs\\{GameMode}-log-{DateTime.Now:yyyyMMddHHmmss}.txt";
                var config = new BotConfig(_configuration, _localizer, scaling ?? 1.0, verbose ?? false, autoplay, saveScreens, logPath, ltm ?? false, downscaled ?? false);
                var surrender = (SurrenderComboBoxGeneral.SelectedItem as ComboBoxItem)?.Content.ToString() == "Yes";
                _currentBot = GameMode switch
                {
                    "conquest" => new ConquestBot(config, retreat, selectedGameState, surrender),
                    "ladder" => new LadderBot(config, retreat),
                    "event" => new EventBot(config, retreat),
                    _ => throw new Exception(_localizer.GetString("Log_InvalidModeSelection"))
                };
                _currentBot.OnLogMessage += AddLog;
                try
                {
                    await Task.Run(async () =>
                    {
                        if (_currentBot is LadderBot ladderBot)
                        {
                            await ladderBot.RunAsync(_globalCts.Token);
                        }
                        else
                        {
                            _currentBot.Run();
                        }
                    }, _globalCts.Token);
                }
                catch (OperationCanceledException)
                {
                    UpdateLogPanel("Operation was canceled by user");
                }
                catch (Exception ex)
                {
                    UpdateLogPanel($"Bot execution error: {ex.Message}");
                    Logger.Log(_localizer, ex.Message, _currentBot.GetLogPath(), true);
                    if (ex.StackTrace != null)
                        Logger.Log(_localizer, ex.StackTrace, _currentBot.GetLogPath(), true);
                }
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
            if (_currentBot != null)
            {
                _currentBot.OnLogMessage -= AddLog;
                _currentBot.Stop();
            }
            HotkeyManager.Stop();
            Dispatcher.Invoke(() =>
            {
                string currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "";
                string baseDirectory = Path.GetDirectoryName(currentDirectory) ?? "";
                string targetFileName = "BoosterBot.exe";
                var targetFolderPath = Directory.GetDirectories(baseDirectory)
                    .FirstOrDefault(dir => Directory.GetFiles(dir, "BoosterBot.exe").Any());
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
                StartButton.Content = "Start"; 
                StartButton.IsEnabled = true;
            });
        }
        private bool ConfirmMaxTierSelection(GameState maxState)
        {
            var result = MessageBox.Show($"Are you sure you want to select {maxState} as the maximum tier?", "Confirm Selection", MessageBoxButton.YesNo);
            return result == MessageBoxResult.Yes;
        }
        private string lastLogMessage = string.Empty;
        private void UpdateLogPanel(string resourceKey, params object[] args)
        {
            var localizedMessage = string.Format(_localizer.GetString(resourceKey), args);
            AddLog($"[SYSTEM] {localizedMessage}");
        }
        private void ShowTutorial()
        {
            var result = MessageBox.Show(
                "欢迎使用BoosterBot！\n\n请确保游戏窗口处于前台\n首次使用请查看教程！",
                "首次使用提示",
                MessageBoxButton.OKCancel,
                MessageBoxImage.Information);

            if (result == MessageBoxResult.OK)
            {
                Process.Start("explorer.exe", "Resources\\description.png");
            }
        }
        private void AddLog(string message)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.BeginInvoke(new Action(() => AddLog(message)));
                return;
            }

            LogTextBox.AppendText(message + Environment.NewLine);

            LogScrollViewer.ScrollToEnd();
        }
        private bool CheckDependencies()
        {
            var requiredDlls = new[]
            {
        "OpenCvSharpExtern.dll",
        "opencv_videoio_ffmpeg451_64.dll"
    };

            var missing = requiredDlls.Where(f => !File.Exists(f)).ToList();

            if (missing.Any())
            {
                UpdateLogPanel($"缺少必要文件：{string.Join(", ", missing)}");
                return false;
            }

            return true;
        }
    }
}