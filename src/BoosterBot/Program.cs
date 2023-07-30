using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Reflection;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace BoosterBot;

internal class Program
{
    static void Main(string[] args)
    {
        bool masked = true; //false;
        bool verbose = false;
        bool autoplay = true;
        bool saveScreens = false;
        bool conquest = false;
        double scaling = 1.0;

        // Parse flags:
        if (args.Length > 0)
            for (int i = 0; i < args.Length; i++)
                switch (args[i])
                {
                    case "-scaling":
                    case "--scaling":
                    case "-s":
                        scaling = double.Parse(args[i++]);
                        break;
                    case "-verbose":
                    case "--verbose":
                    case "-v":
                        verbose = true;
                        break;
                    case "-quiet":
                    case "--quiet":
                    case "-q":
                        verbose = false;
                        break;
                    case "-noautoplay":
                    case "--noautoplay":
                    case "-na":
                        autoplay = false;
                        break;
                    case "-savescreens":
                    case "--savescreens":
                    case "-ss":
                        saveScreens = true;
                        break;
                    case "-conquest":
                    case "--conquest":
                    case "-c":
                        conquest = true;
                        break;
                    case "-masked":
                        masked = true;
                        break;
                }

        if (masked)
        {
            // Create directory if it doesn't exist
            var dir = "screens";
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var mode = GetModeSelection();
            PrintTitle();

            try
            {
                IBoosterBot bot = (mode == 1) ? new ConquestBot(scaling, verbose, autoplay, saveScreens) : new BoosterBot(scaling, verbose, autoplay, saveScreens);
                bot.Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine();
                Console.WriteLine(ex.StackTrace);
            }

            Console.WriteLine("\nPress [Enter] to continue...");
            Console.ReadLine();
        }
        else
        {
            var exe = MaskProcess();
            var startInfo = new ProcessStartInfo();
            startInfo.FileName = exe;
            startInfo.UseShellExecute = true;
            startInfo.CreateNoWindow = false;

            var process = new Process();
            process.StartInfo = startInfo;
            process.StartInfo.Arguments = "-masked " + string.Join(' ', args);

            process.Start();
        }
    }

    private static void PrintTitle()
    {
        var version = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;

        Console.Clear();
        Console.WriteLine("****************************************************");
        var title = $"BoosterBot v{version}";
        title = title.PadLeft(24 + (title.Length / 2), ' ');
        title = $"{title}{"".PadRight(49 - title.Length, ' ')}";
        Console.WriteLine(title);
        Console.WriteLine("****************************************************");
        Console.WriteLine();
    }

    private static int GetModeSelection()
    {
        PrintTitle();
        Console.WriteLine("Available farming modes:");
        Console.WriteLine("[1] Conquest");
        Console.WriteLine("[2] Ranked Ladder");
        Console.WriteLine();
        Console.Write("Enter selection: ");

        var key = Console.ReadKey();
        if (key.KeyChar == '1' || key.KeyChar == '2')
            return int.Parse(key.KeyChar.ToString());

        return GetModeSelection();
    }

    private static void PurgeExecutables()
    {
        var process = Process.GetCurrentProcess().ProcessName + ".exe";
        var files = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.exe");
        foreach (var file in files)
            if (!file.EndsWith("BoosterBot.exe") && !file.EndsWith("createdump.exe") && !file.EndsWith(process))
                File.Delete(file);
    }

    // Method to make a copy of a file with a different name:
    private static string MaskProcess()
    {
        PurgeExecutables();

        string name = $"{RandomString()}.exe";
        File.Copy("BoosterBot.exe", name);

        return name;
    }

    // Static method to generate a random string of characters
    private static string RandomString()
    {
        const string chars = "abcdefghijklmnopqrstuvwxyz";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, random.Next(6, 10)).Select(s => s[random.Next(s.Length)]).ToArray());
    }
}