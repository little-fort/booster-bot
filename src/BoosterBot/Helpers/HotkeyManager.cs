using BoosterBot.Helpers;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace BoosterBot
{
    public static class HotkeyManager
    {
        // Windows API declarations
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        // Modifier keys
        private const uint MOD_ALT = 0x0001;
        private const uint MOD_CONTROL = 0x0002;
        private const uint MOD_SHIFT = 0x0004;
        private const uint MOD_WIN = 0x0008;
        private const uint MOD_NOREPEAT = 0x4000;

        // Virtual key codes for Q and P
        private const uint VK_Q = 0x51;  // 'Q' key
        private const uint VK_P = 0x50;  // 'P' key

        // Hotkey IDs
        private const int HOTKEY_ID_EXIT = 1;
        private const int HOTKEY_ID_PAUSE = 2;

        private static volatile bool _isPaused = false;
        private static IntPtr _windowHandle;
        private static Thread _messageLoop;
        private static volatile bool _shouldRun = true;

        private static LocalizationManager _localizer;
        public static bool IsPaused => _isPaused;

        private static CancellationTokenSource _cancellationTokenSource;
        private static Task _backgroundTask;

        // Initialize hotkeys and message loop
        public static void Initialize(LocalizationManager localizer)
        {
            _localizer = localizer;

            // Start the message loop in a background thread
            _messageLoop = new Thread(MessageLoop)
            {
                IsBackground = true,
                Name = "HotkeyMessageLoop"
            };
            _messageLoop.Start();

            // Register Hotkeys (Ctrl + Alt + Q to exit, Ctrl + Alt + P to pause)
            RegisterHotKey(_windowHandle, HOTKEY_ID_EXIT, MOD_CONTROL | MOD_ALT | MOD_NOREPEAT, VK_Q);
            RegisterHotKey(_windowHandle, HOTKEY_ID_PAUSE, MOD_CONTROL | MOD_ALT | MOD_NOREPEAT, VK_P);

            AppDomain.CurrentDomain.ProcessExit += (s, e) => Cleanup();
        }

        private static void MessageLoop()
        {
            // Create a dummy window to receive messages
            _windowHandle = CreateMessageWindow();

            // Message loop
            while (_shouldRun)
            {
                if (PeekMessage(out MSG msg, _windowHandle, 0, 0, 0x0001))
                {
                    if (msg.message == WM_HOTKEY)
                    {
                        int id = msg.wParam.ToInt32();
                        switch (id)
                        {
                            case HOTKEY_ID_EXIT:
                                // Exit the program
                                _shouldRun = false;
                                Logger.Log(_localizer, "HotkeyExitPressed", "Hotkey: Exit pressed. Exiting application.");
                                Process.GetCurrentProcess().Kill();
                                break;

                            case HOTKEY_ID_PAUSE:
                                // Toggle pause state
                                _isPaused = !_isPaused;
                                string status = _isPaused ? "paused" : "resumed";
                                Logger.Log(_localizer, "HotkeyPausePressed", string.Format("Hotkey: Pause pressed. Task is {0}.", status));
                                break;
                        }
                    }
                }
                TranslateMessage(ref msg);
                DispatchMessage(ref msg);
            }
            Thread.Sleep(10);  // Reduce CPU usage
        }

        // Toggle pause state when hotkey is pressed
        private static void TogglePause()
        {
            _isPaused = !_isPaused;
            Logger.Log(_localizer, _isPaused ? "Log_PauseShortcut" : "Log_ResumeShortcut", "logs\\hotkey.txt");
        }

        // Stop all background tasks
        public static void StopBackgroundTask()
        {
            _cancellationTokenSource?.Cancel();
            _backgroundTask?.Wait();  // Wait for the task to finish
            Logger.Log(_localizer, "Log_BackgroundTaskStopped", "Background task has been stopped.");
        }

        // Start the background task
        public static void StartBackgroundTask()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _cancellationTokenSource.Token;

            _backgroundTask = MonitorBackgroundTaskAsync(cancellationToken);
            Logger.Log(_localizer, "Log_BackgroundTaskStarted", "Background task has been started.");
        }

        // Monitor background task with respect to the pause state
        private static async Task MonitorBackgroundTaskAsync(CancellationToken cancellationToken)
        {
            bool wasPaused = _isPaused;  // Track whether the task was previously paused
            bool isLogging = false;  // Control whether logging happens for task running

            while (!cancellationToken.IsCancellationRequested)
            {
                // If paused, wait for 500ms and continue
                if (_isPaused)
                {
                    if (!wasPaused)  // Log only when transitioning from running to paused
                    {
                        Logger.Log(_localizer, "Log_BackgroundTaskPaused", "Background task is paused.");
                        wasPaused = true;
                        isLogging = false;  // Stop logging background task running during pause
                    }
                    await Task.Delay(500);  // Simulate pause without blocking the UI
                    continue;
                }

                // If not paused, perform the background task
                if (wasPaused)  // Log only when transitioning from paused to running
                {
                    Logger.Log(_localizer, "Log_BackgroundTaskResumed", "Background task has resumed.");
                    wasPaused = false;
                    isLogging = true;  // Enable logging again
                }


                await Task.Delay(100);  // Delay to prevent tight loops
            }
        }


        public static void Stop()
        {
            Cleanup();
        }

        private static void Cleanup()
        {
            if (_windowHandle != IntPtr.Zero)
            {
                UnregisterHotKey(_windowHandle, HOTKEY_ID_EXIT);
                UnregisterHotKey(_windowHandle, HOTKEY_ID_PAUSE);
                DestroyWindow(_windowHandle);
            }
            _shouldRun = false;
        }

        // Additional Windows API methods and structures
        private const int WM_HOTKEY = 0x0312;

        [StructLayout(LayoutKind.Sequential)]
        private struct MSG
        {
            public IntPtr hwnd;
            public uint message;
            public IntPtr wParam;
            public IntPtr lParam;
            public uint time;
            public POINT pt;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;
        }

        [DllImport("user32.dll")]
        private static extern bool PeekMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg);

        [DllImport("user32.dll")]
        private static extern bool TranslateMessage([In] ref MSG lpMsg);

        [DllImport("user32.dll")]
        private static extern IntPtr DispatchMessage([In] ref MSG lpmsg);

        [DllImport("user32.dll")]
        private static extern IntPtr CreateWindowEx(
            uint dwExStyle, string lpClassName, string lpWindowName, uint dwStyle,
            int x, int y, int nWidth, int nHeight, IntPtr hWndParent, IntPtr hMenu,
            IntPtr hInstance, IntPtr lpParam);

        [DllImport("user32.dll")]
        private static extern bool DestroyWindow(IntPtr hWnd);

        private static IntPtr CreateMessageWindow()
            => CreateWindowEx(0, "STATIC", "HotkeyMessageWindow", 0, 0, 0, 0, 0, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
    }
}
