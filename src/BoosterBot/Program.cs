using BoosterBot.Models;
using System.Diagnostics;
using System.Reflection;

namespace BoosterBot;

internal class Program
{
    static void Main(string[] args)
    {
        bool masked = false;
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
                var maxTier = GetMaxTierSelection();
                PrintTitle();

                IBoosterBot bot = (mode == 1) ? new ConquestBot(scaling, verbose, autoplay, saveScreens, maxTier) : new LadderBot(scaling, verbose, autoplay, saveScreens);
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
        Console.WriteLine("Available farming modes:\n");
        Console.WriteLine("[1] Conquest");
        Console.WriteLine("[2] Ranked Ladder");
        Console.WriteLine();
        Console.Write("Waiting for selection...");

        var key = Console.ReadKey();
        if (key.KeyChar == '1' || key.KeyChar == '2')
            return int.Parse(key.KeyChar.ToString());

        return GetModeSelection();
    }

    private static GameState GetMaxTierSelection()
    {
        PrintTitle();
        Console.WriteLine("Select the highest tier of Conquest the bot should farm:\n");
        Console.WriteLine("[1] Proving Grounds (Default)");
        Console.WriteLine("[2] Silver");
        Console.WriteLine("[3] Gold");
        Console.WriteLine("[4] Infinite");
        Console.WriteLine();
        Console.WriteLine("The bot will only farm tiers higher than Proving Grounds if tickets are available.\n*** No Gold will be consumed. ***");
        Console.WriteLine();
        Console.Write("Waiting for selection...");

        var key = Console.ReadKey();
        if (key.KeyChar == '1' || key.KeyChar == '2' || key.KeyChar == '3' || key.KeyChar == '4')
        {
            PrintTitle();

            GameState selection = GameState.UNKNOWN;
            switch (key.KeyChar)
            {
                case '1':
                    selection = GameState.CONQUEST_LOBBY_PG;
                    break;
                case '2':
                    selection = GameState.CONQUEST_LOBBY_SILVER;
                    break;
                case '3':
                    selection = GameState.CONQUEST_LOBBY_GOLD;
                    break;
                case '4':
                    selection = GameState.CONQUEST_LOBBY_INFINITE;
                    break;
            }

            if (selection > GameState.CONQUEST_LOBBY_PG)
                return ConfirmMaxTierSelection(selection);

            return selection;
        }

        return GetMaxTierSelection();
    }

    private static GameState ConfirmMaxTierSelection(GameState selection)
    {
        PrintTitle();

        var tier = selection.ToString().Replace("CONQUEST_LOBBY_", "");
        Console.WriteLine("Maximum farming tier: " + tier);
        Console.WriteLine();

        Console.WriteLine($"ALL available tickets for {tier}{(selection > GameState.CONQUEST_LOBBY_SILVER ? " and below" : "")} will be consumed. " +
            $"\nIf no tickets exist, bot will only play Proving Grounds.");
        Console.WriteLine();
        Console.WriteLine("Are you sure you want to continue?\n");
        Console.WriteLine("[1] Yes");
        Console.WriteLine("[2] No");
        Console.WriteLine();
        Console.Write("Waiting for selection...");

        var key = Console.ReadKey();
        if (key.KeyChar == '1')
            return selection;
        if (key.KeyChar == '2')
            return GetMaxTierSelection();

        return ConfirmMaxTierSelection(selection);
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