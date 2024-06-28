using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Drawing;
using System.Windows.Forms;

namespace BoosterBot
{
    public class MouseUtilities
    {
        [Flags]
        public enum MouseEventFlags
        {
            MOVE = 0x0001,
            LEFTDOWN = 0x0002,
            LEFTUP = 0x0004,
            ABSOLUTE = 0x8000
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct INPUT
        {
            public uint type;
            public InputUnion u;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct InputUnion
        {
            [FieldOffset(0)]
            public MOUSEINPUT mi;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [DllImport("user32.dll")]
        private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        private const int INPUT_MOUSE = 0;

        public static void SendMouseEvent(MouseEventFlags flags, int x = 0, int y = 0)
        {
            INPUT[] inputs = new INPUT[1];

            inputs[0] = new INPUT
            {
                type = INPUT_MOUSE,
                u = new InputUnion
                {
                    mi = new MOUSEINPUT
                    {
                        dx = x,
                        dy = y,
                        dwFlags = (uint)flags,
                        mouseData = 0,
                        time = 0,
                        dwExtraInfo = IntPtr.Zero
                    }
                }
            };

            SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
        }

        public static void MoveCard(Point card, Point loc, Point reset)
        {
            var rand = new Random();

            // Click on card:
            Cursor.Position = new Point(card.X, card.Y);
            SendMouseEvent(MouseEventFlags.LEFTDOWN);

            Thread.Sleep(rand.Next(100, 300));

            // Smooth drag to location:
            int steps = 50; // Number of steps for smooth movement
            int xStep = (loc.X - card.X) / steps;
            int yStep = (loc.Y - card.Y) / steps;

            for (int i = 1; i <= steps; i++)
            {
                Cursor.Position = new Point(card.X + (xStep * i), card.Y + (yStep * i));
                Thread.Sleep(rand.Next(5, 10));
            }

            // Ensure the cursor is precisely at the target location
            Cursor.Position = new Point(loc.X, loc.Y);

            Thread.Sleep(rand.Next(100, 300));

            // Drop at location:
            SendMouseEvent(MouseEventFlags.LEFTUP);

            // Adding an extra click at the end to ensure drop is registered
            Thread.Sleep(rand.Next(50, 150));
            SendMouseEvent(MouseEventFlags.LEFTDOWN);
            Thread.Sleep(rand.Next(50, 100));
            SendMouseEvent(MouseEventFlags.LEFTUP);

            // Add a click to reset view because LEFTUP while hovering over another card will register as click event:
            Cursor.Position = new Point(reset.X + rand.Next(-50, 50), reset.Y + rand.Next(-50, 50));
            SendMouseEvent(MouseEventFlags.LEFTDOWN);
            Thread.Sleep(rand.Next(50, 100));
            SendMouseEvent(MouseEventFlags.LEFTUP);
        }
    }

}
