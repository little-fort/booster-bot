using System.Runtime.InteropServices;
using System.Windows.Threading;
using BoosterBot.Helpers;

namespace BoosterBot
{
    public static class HotkeyManager
    {
        #region Win32 API
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [DllImport("user32.dll")]
        private static extern bool PeekMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg);

        [DllImport("user32.dll")]
        private static extern bool TranslateMessage(ref MSG lpMsg);

        [DllImport("user32.dll")]
        private static extern IntPtr DispatchMessage(ref MSG lpmsg);

        [DllImport("user32.dll")]
        private static extern IntPtr CreateWindowEx(uint dwExStyle, string lpClassName, string lpWindowName, uint dwStyle,
            int x, int y, int nWidth, int nHeight, IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);

        [DllImport("user32.dll")]
        private static extern bool DestroyWindow(IntPtr hWnd);

        private const uint MOD_CONTROL = 0x0002;
        private const uint MOD_ALT = 0x0001;
        private const uint VK_Q = 0x51;
        private const uint VK_P = 0x50;
        private const int HOTKEY_ID_STOP = 1;
        private const int HOTKEY_ID_PAUSE = 2;
        private const uint WM_HOTKEY = 0x0312;
        #endregion

        #region State Management
        private static IntPtr _windowHandle;
        private static Thread? _messageLoopThread; // 改为可空类型
        private static volatile bool _isRunning;
        private static LocalizationManager? _localizer; // 改为可空类型
        private static Dispatcher? _uiDispatcher; // 改为可空类型

        public static bool IsPaused { get; private set; }
        public static bool IsStopped { get; private set; }
        #endregion

        #region Events
        public static event Action<bool> PauseStateChanged = delegate { };
        public static event Action StopRequested = delegate { };

        #endregion

        #region Public Methods
        public static void Initialize(LocalizationManager localizer, Dispatcher uiDispatcher)
        {
            ArgumentNullException.ThrowIfNull(localizer);
            ArgumentNullException.ThrowIfNull(uiDispatcher);
            _localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
            _uiDispatcher = uiDispatcher ?? throw new ArgumentNullException(nameof(uiDispatcher));
            StartBackgroundTask();
        }

        public static void StartBackgroundTask()
        {
            if (_messageLoopThread?.IsAlive == true) return;

            _isRunning = true;
            _messageLoopThread = new Thread(MessageLoop)
            {
                IsBackground = true,
                Priority = ThreadPriority.Highest,

            };
            _messageLoopThread.Start();
        }

        public static void Stop()
        {
            IsStopped = true;
            CleanupResources();
        }

        public static void Pause()
        {
            IsPaused = true;
            PauseStateChanged?.Invoke(true);
        }

        public static void Resume()
        {
            IsPaused = false;
            PauseStateChanged?.Invoke(false);
        }
        #endregion

        #region Core Logic
        private static void MessageLoop()
        {
            try
            {
                _windowHandle = CreateMessageWindow();
                RegisterHotKeys();

                while (_isRunning)
                {
                    if (PeekMessage(out var msg, IntPtr.Zero, 0, 0, 1))
                    {
                        if (msg.message == WM_HOTKEY)
                        {
                            HandleHotkey(msg.wParam);
                        }
                        TranslateMessage(ref msg);
                        DispatchMessage(ref msg);
                    }
                    Thread.Sleep(10);
                }
            }
            finally
            {
                CleanupResources();
            }
        }

        private static void RegisterHotKeys()
        {
            if (!RegisterHotKey(_windowHandle, HOTKEY_ID_STOP, MOD_CONTROL | MOD_ALT, VK_Q))
                Logger.LogError("Failed to register STOP hotkey");

            if (!RegisterHotKey(_windowHandle, HOTKEY_ID_PAUSE, MOD_CONTROL | MOD_ALT, VK_P))
                Logger.LogError("Failed to register PAUSE hotkey");
        }

        private static void HandleHotkey(IntPtr hotkeyId)
        {
            if (_uiDispatcher == null)
            {
                Logger.LogError("UI Dispatcher not initialized");
                return;
            }
            switch ((int)hotkeyId)
            {
                case HOTKEY_ID_STOP:
                    _uiDispatcher.Invoke(() => StopRequested?.Invoke());
                    break;

                case HOTKEY_ID_PAUSE:
                    IsPaused = !IsPaused;
                    _uiDispatcher.Invoke(() => PauseStateChanged?.Invoke(IsPaused));
                    break;
            }
        }

        private static void CleanupResources()
        {
            try
            {
                _isRunning = false; // 确保消息循环退出

                if (_windowHandle != IntPtr.Zero)
                {
                    UnregisterHotKey(_windowHandle, HOTKEY_ID_STOP);
                    UnregisterHotKey(_windowHandle, HOTKEY_ID_PAUSE);
                    DestroyWindow(_windowHandle);
                    _windowHandle = IntPtr.Zero;
                }

                _messageLoopThread?.Join(1000); // 等待线程退出
            }
            catch (Exception ex)
            {
                Logger.LogError($"Cleanup failed: {ex.Message}");
            }
        }
        #endregion

        #region Win32 Structs
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
        #endregion

        private static IntPtr CreateMessageWindow()
            => CreateWindowEx(0, "STATIC", "HotkeyMsgWindow", 0, 0, 0, 0, 0, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
    }
}