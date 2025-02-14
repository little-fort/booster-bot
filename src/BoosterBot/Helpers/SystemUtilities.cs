using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows;

namespace BoosterBot
{
    internal class SystemUtilities
    {
        // 导入Windows API函数
        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(string strClassName, string strWindowName);

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hwnd, ref Rect rectangle);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hwnd);

        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        private static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetCursorPos(out POINT lpPoint);

        // 定义 POINT 结构体
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }

        // 鼠标事件标志
        [Flags]
        public enum MouseEventFlags
        {
            LEFTDOWN = 0x00000002,
            LEFTUP = 0x00000004,
            MIDDLEDOWN = 0x00000020,
            MIDDLEUP = 0x00000040,
            MOVE = 0x00000001,
            ABSOLUTE = 0x00008000,
            RIGHTDOWN = 0x00000008,
            RIGHTUP = 0x00000010
        }

        // 点击鼠标
        public static void Click(int x, int y)
        {
            SetCursorPos(x, y);
            mouse_event((int)(MouseEventFlags.LEFTDOWN), 0, 0, 0, 0);
            System.Threading.Thread.Sleep(250);
            mouse_event((int)(MouseEventFlags.LEFTUP), 0, 0, 0, 0);
            System.Threading.Thread.Sleep(250);
        }

        // 处理点击事件，支持 System.Drawing.Point 类型
        public static void Click(System.Drawing.Point pnt) => Click(pnt.X, pnt.Y);

        // 拖动鼠标
        public static void Drag(int startX, int startY, int endX, int endY)
        {
            int distanceX = endX - startX;
            int distanceY = endY - startY;
            int steps = 50;
            float stepX = distanceX / (float)steps;
            float stepY = distanceY / (float)steps;

            SetCursorPos(startX, startY);
            mouse_event((int)(MouseEventFlags.LEFTDOWN), 0, 0, 0, 0);
            System.Threading.Thread.Sleep(50);

            for (int i = 0; i < steps; i++)
            {
                SetCursorPos(startX + (int)(stepX * i), startY + (int)(stepY * i));
                System.Threading.Thread.Sleep(1);
            }

            SetCursorPos(endX, endY);
            mouse_event((int)(MouseEventFlags.LEFTUP), 0, 0, 0, 0);
        }

        // 获取游戏窗口的矩形
        public static Rect GetGameWindowLocation()
        {
            var rect = new Rect();
            var ptr = FocusGameWindow();
            SetForegroundWindow(ptr);
            GetWindowRect(ptr, ref rect);
            return rect;
        }

        // 聚焦到游戏窗口
        public static IntPtr FocusGameWindow()
        {
            var processes = Process.GetProcessesByName("SNAP").ToList();
            processes.AddRange(Process.GetProcessesByName("SnapCN"));
            processes.AddRange(Process.GetProcessesByName("streaming_client"));

            if (processes.Count == 0)
                throw new Exception("Game process not found");

            var snap = processes[0];
            var ptr = snap.MainWindowHandle;
            SetForegroundWindow(ptr);
            return ptr;
        }

        // 获取游戏截图，并进行缩放
        public static Dimension GetGameScreencap(Rect window, double scaling)
        {
            var width = (int)((window.Right - window.Left) * scaling);
            var height = (int)((window.Bottom - window.Top) * scaling) - 6;

            using var bitmap = new Bitmap(width, height);
            using var g = Graphics.FromImage(bitmap);

            var left = (int)(window.Left * scaling) + 6;
            var top = (int)(window.Top * scaling);
            g.CopyFromScreen(left, top, 0, 0, bitmap.Size, CopyPixelOperation.SourceCopy);

            bitmap.Save(BotConfig.DefaultImageLocation, ImageFormat.Png);

            return new Dimension
            {
                Width = width,
                Height = height
            };
        }
    }
}
