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
        SetCursorPos(x, y);
        Thread.Sleep(_rand.Next(30, 80));

        SendMouseEvent(MouseEventFlags.LeftDown);
        Thread.Sleep(_rand.Next(50, 100));
        SendMouseEvent(MouseEventFlags.LeftUp);
    }

    public void DragAndDrop(int fromX, int fromY, int toX, int toY, DragOptions? options = null)
    {
        options ??= new DragOptions();

        FocusGameWindow();
        SetCursorPos(fromX, fromY);
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

            SetCursorPos(x, y);
            Thread.Sleep(_rand.Next(
                Math.Max(1, options.StepDelayMs - 2),
                options.StepDelayMs + 3));
        }

        SetCursorPos(toX, toY);
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
