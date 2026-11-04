using System.Diagnostics;
using BoosterBot.Core.Models;
using BoosterBot.Core.Pipeline;

namespace BoosterBot.Platform.Windows;

public class Win32WindowManager : IWindowManager
{
    private readonly string[] _processNames;
    private IntPtr _cachedHandle = IntPtr.Zero;
    private DateTime _handleValidatedAt = DateTime.MinValue;
    private static readonly TimeSpan HandleCacheDuration = TimeSpan.FromSeconds(10);

    public Win32WindowManager(string[]? processNames = null)
    {
        _processNames = processNames ?? ["SNAP", "SnapCN", "streaming_client"];
    }

    public WindowInfo? FindGameWindow()
    {
        var handle = GetWindowHandle();
        if (handle == IntPtr.Zero)
            return null;

        var rect = GetClientScreenRect(handle);
        var processName = GetProcessName(handle);

        return new WindowInfo(handle, processName ?? "unknown", rect, 1.0);
    }

    public void FocusWindow(WindowInfo window)
    {
        NativeMethods.SetForegroundWindow(window.Handle);
    }

    public bool IsWindowVisible(WindowInfo window)
    {
        return NativeMethods.IsWindowVisible(window.Handle);
    }

    public Rect GetWindowPosition()
    {
        var handle = GetWindowHandle();
        if (handle == IntPtr.Zero)
            throw new InvalidOperationException("Game window not found");

        return GetClientScreenRect(handle);
    }

    // Unity cannot create render targets above 16384px; tiny sizes are never intended
    private const int MinClientSize = 100;
    private const int MaxClientSize = 16384;
    private static readonly TimeSpan ResizeTimeout = TimeSpan.FromSeconds(3);

    /// <summary>
    /// Resizes the game window so its client area (game pixels) is exactly width x height,
    /// keeping the window's position. Windowed mode only. Returns the resulting client area
    /// in screen coordinates, which can differ from the request if Windows constrains the size.
    /// </summary>
    /// <remarks>
    /// Uses a single SetWindowPos call, the same API manual tools such as Scott's Window Resizing
    /// Utility use. Guards: the size and window frame are validated before the call (an earlier
    /// ad-hoc script that sent a height in the millions crashed the client), the game must be
    /// responsive, requests are posted asynchronously so a hung game can't block the caller, and
    /// the method waits for the resize to land and re-checks responsiveness before returning.
    /// </remarks>
    public Rect ResizeClient(int width, int height)
    {
        if (width < MinClientSize || height < MinClientSize || width > MaxClientSize || height > MaxClientSize)
            throw new ArgumentOutOfRangeException(nameof(width), $"Client size must be {MinClientSize}–{MaxClientSize}px per side (got {width}x{height})");

        var handle = GetWindowHandle();
        if (handle == IntPtr.Zero)
            throw new InvalidOperationException("Game window not found");

        // Fullscreen is a borderless popup the game sizes itself; only a framed window can be resized
        uint style = (uint)NativeMethods.GetWindowLong(handle, NativeMethods.GWL_STYLE);
        if ((style & NativeMethods.WS_CAPTION) != NativeMethods.WS_CAPTION || (style & NativeMethods.WS_THICKFRAME) == 0)
            throw new InvalidOperationException("The game is not in windowed mode (turn off Fullscreen in its settings)");

        if (!IsResponding(handle))
            throw new InvalidOperationException("The game is not responding; not resizing");

        // A minimized or maximized window ignores size changes until restored
        if (NativeMethods.IsIconic(handle) || NativeMethods.IsZoomed(handle))
        {
            NativeMethods.ShowWindowAsync(handle, NativeMethods.SW_RESTORE);
            var restore = Stopwatch.StartNew();
            while ((NativeMethods.IsIconic(handle) || NativeMethods.IsZoomed(handle)) && restore.Elapsed < ResizeTimeout)
                Thread.Sleep(100);
            if (NativeMethods.IsIconic(handle) || NativeMethods.IsZoomed(handle))
                throw new InvalidOperationException("The game window could not be restored from minimized/maximized");
        }

        // The frame (title bar + borders) is the difference between the window and client sizes
        var window = new Rect();
        var client = new Rect();
        NativeMethods.GetWindowRect(handle, ref window);
        NativeMethods.GetClientRect(handle, ref client);
        int frameWidth = window.Width - client.Width, frameHeight = window.Height - client.Height;
        if (client.Width <= 0 || client.Height <= 0 || frameWidth is < 0 or > 200 || frameHeight is < 0 or > 200)
            throw new InvalidOperationException(
                $"Unexpected window geometry (window {window.Width}x{window.Height}, client {client.Width}x{client.Height}); not resizing");

        if (!NativeMethods.SetWindowPos(handle, IntPtr.Zero, 0, 0, width + frameWidth, height + frameHeight,
                NativeMethods.SWP_NOMOVE | NativeMethods.SWP_NOZORDER | NativeMethods.SWP_NOACTIVATE | NativeMethods.SWP_ASYNCWINDOWPOS))
            throw new InvalidOperationException($"SetWindowPos failed (Win32 error {System.Runtime.InteropServices.Marshal.GetLastWin32Error()})");

        // The request was posted; wait for the game to apply it before anything else touches the window
        var elapsed = Stopwatch.StartNew();
        do
        {
            Thread.Sleep(100);
            NativeMethods.GetClientRect(handle, ref client);
        }
        while ((client.Width != width || client.Height != height) && elapsed.Elapsed < ResizeTimeout);

        if (!IsResponding(handle))
            throw new InvalidOperationException("The game stopped responding after the resize");

        return GetClientScreenRect(handle);
    }

    // WM_NULL round-trip with a timeout: false if the window's thread is hung
    private static bool IsResponding(IntPtr handle) =>
        NativeMethods.SendMessageTimeout(handle, NativeMethods.WM_NULL, IntPtr.Zero, IntPtr.Zero,
            NativeMethods.SMTO_ABORTIFHUNG, 2000, out _) != IntPtr.Zero;

    /// <summary>Describes the game window for diagnostics: mode, client area, and DPI.</summary>
    public string DescribeWindow()
    {
        var handle = GetWindowHandle();
        if (handle == IntPtr.Zero)
            return "Game window not found";

        uint style = (uint)NativeMethods.GetWindowLong(handle, NativeMethods.GWL_STYLE);
        bool windowed = (style & NativeMethods.WS_CAPTION) == NativeMethods.WS_CAPTION;
        var client = GetClientScreenRect(handle);
        return $"{(windowed ? "windowed" : "fullscreen (borderless)")}, client {client.Width}x{client.Height} at ({client.Left},{client.Top}), "
             + $"DPI {NativeMethods.GetDpiForWindow(handle)}{(NativeMethods.IsIconic(handle) ? ", minimized" : "")}{(NativeMethods.IsZoomed(handle) ? ", maximized" : "")}";
    }

    /// <summary>
    /// Makes this process per-monitor DPI aware so window coordinates are physical pixels.
    /// Without it, Windows reports another process's window geometry scaled to 96 DPI on
    /// monitors above 100% scaling (DPI virtualization), which would skew capture, input, and
    /// resizing. Avalonia does this itself when the GUI starts; non-GUI entry points must call it
    /// before creating any windows. Throws if the process ends up anything but per-monitor aware
    /// (the call fails if awareness was already fixed, which is fine only when it is compatible).
    /// </summary>
    public static void EnablePerMonitorDpiAwareness()
    {
        NativeMethods.SetProcessDpiAwarenessContext(NativeMethods.DpiAwarenessPerMonitorV2);

        int awareness = NativeMethods.GetAwarenessFromDpiAwarenessContext(NativeMethods.GetThreadDpiAwarenessContext());
        if (awareness != NativeMethods.DPI_AWARENESS_PER_MONITOR_AWARE)
            throw new InvalidOperationException($"Could not make the process per-monitor DPI aware (awareness {awareness}); window coordinates would be scaled");
    }

    /// <summary>
    /// The window's client area in screen coordinates. In windowed mode the game has a title bar
    /// and borders; only the client area holds game pixels, so capture and click mapping use it.
    /// (Borderless fullscreen has no non-client area, so client == window there.)
    /// </summary>
    private static Rect GetClientScreenRect(IntPtr handle)
    {
        var client = new Rect();
        NativeMethods.GetClientRect(handle, ref client);
        var origin = new NativeMethods.POINT();
        NativeMethods.ClientToScreen(handle, ref origin);
        return new Rect { Left = origin.X, Top = origin.Y, Right = origin.X + client.Width, Bottom = origin.Y + client.Height };
    }

    private IntPtr GetWindowHandle()
    {
        if (_cachedHandle != IntPtr.Zero && DateTime.UtcNow - _handleValidatedAt < HandleCacheDuration)
        {
            try
            {
                if (NativeMethods.IsWindowVisible(_cachedHandle))
                    return _cachedHandle;
            }
            catch { }
        }

        _cachedHandle = IntPtr.Zero;

        foreach (var name in _processNames)
        {
            var processes = Process.GetProcessesByName(name);
            foreach (var proc in processes)
            {
                if (proc.MainWindowHandle != IntPtr.Zero)
                {
                    _cachedHandle = proc.MainWindowHandle;
                    _handleValidatedAt = DateTime.UtcNow;
                    return _cachedHandle;
                }
            }
        }

        return IntPtr.Zero;
    }

    private string? GetProcessName(IntPtr handle)
    {
        foreach (var name in _processNames)
        {
            var processes = Process.GetProcessesByName(name);
            foreach (var proc in processes)
            {
                if (proc.MainWindowHandle == handle)
                    return proc.ProcessName;
            }
        }
        return null;
    }
}
