using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace BoosterBot;

internal static class ContentHelper
{
    private static string _resourcePath;
    private static Process _currentViewer;

    static ContentHelper()
    {
        // Get the executing assembly's directory
        var assemblyLocation = Assembly.GetExecutingAssembly().Location;
        var assemblyDirectory = Path.GetDirectoryName(assemblyLocation);
        _resourcePath = Path.Combine(assemblyDirectory, "resources");

        // Ensure resources directory exists
        if (!Directory.Exists(_resourcePath))
        {
            Directory.CreateDirectory(_resourcePath);
        }

        // Register cleanup on app domain exit
        AppDomain.CurrentDomain.ProcessExit += (s, e) => CleanupViewer();
    }

    public static void ShowImage(string imageName, string prompt = "")
    {
        var imagePath = Path.Combine(_resourcePath, imageName);

        if (!File.Exists(imagePath))
        {
            Console.WriteLine($"\nError: Example image not found: {imageName}");
            return;
        }

        try
        {
            // Close any existing viewer
            CleanupViewer();

            // Show prompt if provided
            if (!string.IsNullOrWhiteSpace(prompt))
            {
                Console.WriteLine($"\n{prompt}");
            }

            Console.WriteLine("\nPress any key to view the example image...");
            Console.ReadKey(true);

            // Start the image viewer
            _currentViewer = Process.Start(new ProcessStartInfo
            {
                FileName = imagePath,
                UseShellExecute = true,
                Verb = "open"
            });

            // Optional: Wait a moment to ensure the viewer has started
            Thread.Sleep(500);

            // Bring console window back to front
            BringConsoleToFront();

            Console.WriteLine("\nExample image has been opened in your default image viewer.");
            Console.WriteLine("Press any key to close the image and continue...");
            Console.ReadKey(true);

            // Close the viewer
            CleanupViewer();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nError showing image: {ex.Message}");
            Console.WriteLine($"Image display error: {ex.Message}", "logs\\repair.txt");
        }
    }

    private static void CleanupViewer()
    {
        try
        {
            if (_currentViewer != null && !_currentViewer.HasExited)
            {
                _currentViewer.Kill();
                _currentViewer.Dispose();
                _currentViewer = null;
            }
        }
        catch
        {
            // Best effort cleanup
        }
    }

    private static void BringConsoleToFront()
    {
        // Windows API imports for window management
        [DllImport("kernel32.dll", ExactSpelling = true)]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_RESTORE = 9;

        try
        {
            var handle = GetConsoleWindow();
            ShowWindow(handle, SW_RESTORE);
            SetForegroundWindow(handle);
        }
        catch
        {
            // Best effort - if it fails, the console just won't come to front
        }
    }

    public static void EnsureResourceExists(string imageName, byte[] imageData)
    {
        var imagePath = Path.Combine(_resourcePath, imageName);
        if (!File.Exists(imagePath))
        {
            File.WriteAllBytes(imagePath, imageData);
        }
    }
}