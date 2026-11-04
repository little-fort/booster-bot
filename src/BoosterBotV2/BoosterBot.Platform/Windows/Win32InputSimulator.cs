using System.Runtime.InteropServices;
using BoosterBot.Core.Input;
using BoosterBot.Core.Pipeline;
using static BoosterBot.Platform.Windows.NativeMethods;

namespace BoosterBot.Platform.Windows;

public class Win32InputSimulator : IInputSimulator
{
    private readonly IWindowManager _windowManager;
    private readonly Random _rand = new();

    public Win32InputSimulator(IWindowManager windowManager)
    {
        _windowManager = windowManager;
    }

    public void Click(int x, int y)
    {
        FocusGameWindow();
        MoveCursor(x, y);
        Thread.Sleep(_rand.Next(30, 80));

        SendMouseEvent(MouseEventFlags.LeftDown);
        Thread.Sleep(_rand.Next(50, 100));
        SendMouseEvent(MouseEventFlags.LeftUp);
    }

    public void DragAndDrop(int fromX, int fromY, int toX, int toY, DragOptions? options = null)
    {
        options ??= new DragOptions();

        FocusGameWindow();
        MoveCursor(fromX, fromY);
        Thread.Sleep(_rand.Next(50, 100));

        SendMouseEvent(MouseEventFlags.LeftDown);
        Thread.Sleep(_rand.Next(100, 300));

        int steps = options.Steps;
        for (int i = 1; i <= steps; i++)
        {
            int x = fromX + (toX - fromX) * i / steps;
            int y = fromY + (toY - fromY) * i / steps;

            if (options.AddJitter)
            {
                x += _rand.Next(-2, 3);
                y += _rand.Next(-2, 3);
            }

            MoveCursor(x, y);
            Thread.Sleep(_rand.Next(
                Math.Max(1, options.StepDelayMs - 2),
                options.StepDelayMs + 3));
        }

        MoveCursor(toX, toY);
        Thread.Sleep(_rand.Next(100, 300));
        SendMouseEvent(MouseEventFlags.LeftUp);
    }

    public void SendKey(int virtualKeyCode)
    {
        FocusGameWindow();
        Thread.Sleep(200);

        ushort scanCode = (ushort)MapVirtualKey((uint)virtualKeyCode, MAPVK_VK_TO_VSC);

        var keyDown = new INPUT
        {
            type = INPUT_KEYBOARD,
            u = new InputUnion
            {
                ki = new KEYBDINPUT
                {
                    wVk = 0,
                    wScan = scanCode,
                    dwFlags = KEYEVENTF_SCANCODE,
                }
            }
        };

        var keyUp = new INPUT
        {
            type = INPUT_KEYBOARD,
            u = new InputUnion
            {
                ki = new KEYBDINPUT
                {
                    wVk = 0,
                    wScan = scanCode,
                    dwFlags = KEYEVENTF_SCANCODE | KEYEVENTF_KEYUP,
                }
            }
        };

        NativeMethods.SendInput(1, [keyDown], Marshal.SizeOf<INPUT>());
        Thread.Sleep(30);
        NativeMethods.SendInput(1, [keyUp], Marshal.SizeOf<INPUT>());
    }

    private void FocusGameWindow()
    {
        var window = _windowManager.FindGameWindow()
            ?? throw new InvalidOperationException("Game window not found");
        _windowManager.FocusWindow(window);
    }

    /// <summary>
    /// Moves the cursor by injecting a mouse-move event rather than calling SetCursorPos.
    /// SetCursorPos only repositions the cursor; Unity (Marvel Snap) does not treat that as
    /// pointer motion, so drags (card plays, carousel swipes) never register. Absolute
    /// coordinates are normalized to 0..65535 across the whole virtual desktop, which can
    /// start at negative coordinates when monitors sit left of/above the primary.
    /// </summary>
    private static void MoveCursor(int x, int y)
    {
        int left = GetSystemMetrics(SM_XVIRTUALSCREEN);
        int top = GetSystemMetrics(SM_YVIRTUALSCREEN);
        int width = GetSystemMetrics(SM_CXVIRTUALSCREEN);
        int height = GetSystemMetrics(SM_CYVIRTUALSCREEN);

        var input = new INPUT
        {
            type = INPUT_MOUSE,
            u = new InputUnion
            {
                mi = new MOUSEINPUT
                {
                    // Windows quantizes absolute coordinates internally (some pixels are unreachable),
                    // so this lands within ±1px of the target. Measured: max error 1px over 19,200
                    // samples on a 6400x1080 virtual desktop. Irrelevant for clicks and drags.
                    dx =(int)Math.Ceiling((x - left) * 65536.0 / width),
                    dy = (int)Math.Ceiling((y - top) * 65536.0 / height),
                    dwFlags = (uint)(MouseEventFlags.Move | MouseEventFlags.Absolute | MouseEventFlags.VirtualDesk),
                }
            }
        };

        NativeMethods.SendInput(1, [input], Marshal.SizeOf<INPUT>());
    }

    private static void SendMouseEvent(MouseEventFlags flags)
    {
        var input = new INPUT
        {
            type = INPUT_MOUSE,
            u = new InputUnion
            {
                mi = new MOUSEINPUT
                {
                    dwFlags = (uint)flags,
                }
            }
        };

        NativeMethods.SendInput(1, [input], Marshal.SizeOf<INPUT>());
    }
}
