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

        var rect = new Rect();
        NativeMethods.GetWindowRect(handle, ref rect);

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

        var rect = new Rect();
        NativeMethods.GetWindowRect(handle, ref rect);
        return rect;
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
