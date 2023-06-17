using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Management;
using System.Windows.Forms;

namespace BoosterBot;

public class Utilities
{
    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr FindWindow(string strClassName, string strWindowName);

    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hwnd, ref Rect rectangle);

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hwnd);

    [DllImport("user32.dll")]
    private static extern bool EnumDisplaySettings(string lpszDeviceName, int iModeNum, ref DevMode lpDevMode);

    [DllImport("user32.dll")]
    private static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);
    
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

    /// <summary>
    /// Used to simulate a mouse click at a specific point on the screen.
    /// </summary>
    public static void Click(int x, int y)
    {
        Cursor.Position = new System.Drawing.Point(x, y);
        mouse_event((int)(MouseEventFlags.LEFTDOWN), 0, 0, 0, 0);
        Thread.Sleep(250);
        mouse_event((int)(MouseEventFlags.LEFTUP), 0, 0, 0, 0);
        Thread.Sleep(250);
    }

    /// <summary>
    /// Used to simulate a swipe from one point to another on the screen.
    /// </summary>
    public static void Swipe(int startX, int startY, int endX, int endY)
    {
        // Determine the distance to swipe
        int distanceX = endX - startX;
        int distanceY = endY - startY;

        // Determine the number of steps and the size of each step
        int steps = 100;
        float stepX = distanceX / (float)steps;
        float stepY = distanceY / (float)steps;

        // Press down the mouse button
        Cursor.Position = new Point(startX, startY);
        mouse_event((int)(MouseEventFlags.LEFTDOWN), 0, 0, 0, 0);
        Thread.Sleep(50);  // Adjust sleep time as needed

        // Gradually move the mouse to the end position
        for (int i = 0; i < steps; i++)
        {
            Cursor.Position = new Point(startX + (int)(stepX * i), startY + (int)(stepY * i));
            Thread.Sleep(5);  // Adjust sleep time as needed
        }

        // Release the mouse button
        Cursor.Position = new Point(endX, endY);
        mouse_event((int)(MouseEventFlags.LEFTUP), 0, 0, 0, 0);
    }

    public static void Click(Point pnt) => Click(pnt.X, pnt.Y);

    // center + 20, bottom - 180
    public static void PlayCard(Point card, Point loc, Point reset)
    {
        var rand = new Random();

        // Click on card:
        Cursor.Position = new System.Drawing.Point(card.X, card.Y);
        mouse_event((int)(MouseEventFlags.LEFTDOWN), 0, 0, 0, 0);

        Thread.Sleep(rand.Next(400, 800));

        // Drag to location and drop:
        Cursor.Position = new System.Drawing.Point(loc.X, loc.Y);
        mouse_event((int)(MouseEventFlags.LEFTUP), 0, 0, 0, 0);

        Thread.Sleep(rand.Next(1000, 2000));

        // Add a click to reset view because LEFTUP while hovering over another card will register as click event:
        Cursor.Position = new System.Drawing.Point(reset.X, reset.Y);
        mouse_event((int)(MouseEventFlags.LEFTDOWN), 0, 0, 0, 0);
        mouse_event((int)(MouseEventFlags.LEFTUP), 0, 0, 0, 0);
    }

    /// <summary>
    /// Retrieves the game process and returns the dimensions of the process window.
    /// </summary>
    /// <returns>A <see cref="Rect">Rect</see> object containing the position of the game window.</returns>
    public static Rect GetGameWindowLocation()
    {
        var rect = new Rect();
        var ptr = FocusGameWindow();

        SetForegroundWindow(ptr);
        GetWindowRect(ptr, ref rect);

        return rect;
    }

    /// <summary>
    /// Gives focus to the game window to bring it up in case it is inadvertently minimized or covered by another window.
    /// </summary>
    /// <returns>A <see cref="IntPtr">IntPtr</see> pointer for the game process.</returns>
    public static IntPtr FocusGameWindow()
    {
        var processes = Process.GetProcessesByName("SNAP");

        if (processes?.Length == 0)
            throw new Exception("SNAP.exe is not running!");

        var snap = processes[0];
        var ptr = snap.MainWindowHandle;
        SetForegroundWindow(ptr);

        return ptr;
    }

    /// <summary>
    /// Captures the current game's window, saves it to a file, and returns the dimensions of the screencap.
    /// </summary>
    /// <param name="window">The position of the game window.</param>
    /// <param name="scaling"> scaling factor that needs to be applied to the position coordinates (e.g 1.25x, 1.5x, 2.0x, etc.).</param>
    /// <returns>A <see cref="Rect">Rect</see> object containing the dimensions of the screen cap.</returns>
    public static Dimension GetGameScreencap(Rect window, double scaling)
    {
        // Get game window width and height:
        var width = (int)((window.Right - window.Left) * scaling);
        var height = (int)((window.Bottom - window.Top) * scaling) - 6;

        // Create bitmap:
        using var bitmap = new Bitmap(width, height);
        using var g = Graphics.FromImage(bitmap);

        // Capture game window:
        var left = (int)(window.Left * scaling) + 6;
        var top = (int)(window.Top * scaling);
        g.CopyFromScreen(left, top, 0, 0, bitmap.Size, CopyPixelOperation.SourceCopy);

        bitmap.Save("screen.png", ImageFormat.Png);

        return new Dimension
        {
            Width = width,
            Height = height
        };
    }

    /// <summary>
    /// Currently unused. May be useful if I decide to expand support for multiple displays or dynamic scaling.
    /// </summary>
    public static void GetDisplayProperties()
    {
        var scope = new ManagementScope("\\\\.\\ROOT\\cimv2");
        var query = new ObjectQuery("SELECT * FROM Win32_VideoController WHERE DeviceID=\"VideoController1\"");
        var searcher = new ManagementObjectSearcher(scope, query);
        var queryCollection = searcher.Get();

        //enumerate the collection.
        foreach (var device in queryCollection)
        {
            //Console.WriteLine("CurrentHorizontalResolution : {0}", m["CurrentHorizontalResolution"]);
            //Console.WriteLine("CurrentVerticalResolution : {0}", m["CurrentVerticalResolution"]);
            foreach (var prop in device.Properties)
                Console.WriteLine(prop.Name.PadRight(30) + prop.Value);
        }

        Console.WriteLine("\n\n");

        var screenList = Screen.AllScreens;

        foreach (var screen in screenList)
        {
            DevMode dm = new DevMode();
            dm.dmSize = (short)Marshal.SizeOf(typeof(DevMode));
            EnumDisplaySettings(screen.DeviceName, -1, ref dm);

            var scalingFactor = Math.Round(Decimal.Divide(dm.dmPelsWidth, screen.Bounds.Width), 2);
            Console.WriteLine("Screen: " + screen.DeviceName, "Scaling: " + scalingFactor);
        }
    }
}