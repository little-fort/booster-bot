using BoosterBot.Helpers;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace BoosterBot;

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
    private const uint VK_Q = 0x51;
    private const uint VK_P = 0x50;

    // Hotkey IDs
    private const int HOTKEY_ID_EXIT = 1;
    private const int HOTKEY_ID_PAUSE = 2;

    private static volatile bool _isPaused = false;
    private static IntPtr _windowHandle;
    private static Thread _messageLoop;
    private static volatile bool _shouldRun = true;

    private static LocalizationManager _localizer;
    public static bool IsPaused => _isPaused;

    public static void Initialize(LocalizationManager localizer)
    {
        _localizer = localizer;

        // Create a message-only window to receive hotkey messages
        _messageLoop = new Thread(MessageLoop)
        {
            IsBackground = true,
            Name = "HotkeyMessageLoop"
        };
        _messageLoop.Start();

        AppDomain.CurrentDomain.ProcessExit += (s, e) => Cleanup();
    }

    private static void MessageLoop()
    {
        // Create a dummy window to receive messages
        _windowHandle = CreateMessageWindow();

        // Register hotkeys (Ctrl + Alt + Q for exit, Ctrl + Alt + P for pause)
        RegisterHotKey(_windowHandle, HOTKEY_ID_EXIT, MOD_CONTROL | MOD_ALT | MOD_NOREPEAT, VK_Q);
        RegisterHotKey(_windowHandle, HOTKEY_ID_PAUSE, MOD_CONTROL | MOD_ALT | MOD_NOREPEAT, VK_P);

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
                            Logger.Log(_localizer, "Log_ExitShortcut", "logs\\hotkey.txt");
                            Process.GetCurrentProcess().Kill();
                            break;
                        case HOTKEY_ID_PAUSE:
                            _isPaused = !_isPaused;
                            Logger.Log(_localizer, _isPaused ? "Log_PauseShortcut" : "Log_ResumeShortcut", "logs\\hotkey.txt");
                            break;
                    }
                }
                TranslateMessage(ref msg);
                DispatchMessage(ref msg);
            }
            Thread.Sleep(10); // Reduce CPU usage
        }
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