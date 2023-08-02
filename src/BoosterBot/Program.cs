using System.Diagnostics;
using System.Reflection;

namespace BoosterBot;

internal class Program
{
    static void Main(string[] args)
    {
        bool masked = true; // false; // RETURN BEFORE COMMIT
        bool verbose = false;
        bool autoplay = true;
        bool saveScreens = false;
        double scaling = 1.0;

        // Parse flags:
        if (args.Length > 0)
            for (int i = 0; i < args.Length; i++)
                switch (args[i])
                {
                    case "-scaling":
                    case "--scaling":
                    case "-s":
                        scaling = double.Parse(args[i + 1]);
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
                    case "-masked":
                        masked = true;
                        break;
                }

        try
        {
            if (!Directory.Exists("logs"))
                Directory.CreateDirectory("logs");
            else
                PurgeLogs();

            if (masked)
            {
                if (!Directory.Exists("screens"))
                    Directory.CreateDirectory("screens");

                var mode = GetModeSelection();
                PrintTitle();

                IBoosterBot bot = (mode == 1) ? new ConquestBot(scaling, verbose, autoplay, saveScreens) : new LadderBot(scaling, verbose, autoplay, saveScreens);
                try
                {
                    bot.Run();
                }
                catch (Exception ex)
                {
                    Logger.Log("***** FATAL ERROR *****", bot.GetLogPath());
                    Logger.Log(ex.Message, bot.GetLogPath());
                    Console.WriteLine();
                    Logger.Log(ex.StackTrace, bot.GetLogPath());
                }

                Console.WriteLine("\nPress [Enter] to exit...");
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
        catch (Exception ex)
        {
            var log = $"logs\\startup-log-{DateTime.Now.ToString("yyyyMMddHHmmss")}.txt";
            Logger.Log("***** FATAL ERROR *****", log);
            Logger.Log(ex.Message, log);
            Console.WriteLine();
            Logger.Log(ex.StackTrace, log);
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

    private static void PurgeLogs()
    {
        // Get all log files
        var logFiles = Directory.GetFiles("logs\\").Select(f => new FileInfo(f));

        // Order them by creation time (descending)
        var orderedLogFiles = logFiles.OrderByDescending(f => f.CreationTime).ToList();

        // Skip 10 most recent ones and delete the rest
        foreach (var oldFile in orderedLogFiles.Skip(5))
            oldFile.Delete();
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