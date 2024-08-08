﻿using BoosterBot.Models;
using System.Diagnostics;
using System.Reflection;

namespace BoosterBot;

internal class Program
{
    static void Main(string[] args)
    {
        bool masked = true;             // CHANGE FOR RELEASE
        bool verbose = false;
        bool autoplay = true;
        bool saveScreens = false;
        double scaling = 1.0;
        string gameMode = "";
        string maxConquestTier = "";
        int maxTurns = -1;

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
                    case "-mode":
                    case "--mode":
                    case "-m":
                        gameMode = args[i + 1];
                        break;
                    case "-turns":
                    case "--turns":
                    case "-t":
                        maxTurns = int.Parse(args[i + 1]);
                        break;
                    case "-tier":
                    case "--tier":
                    case "-ct":
                        maxConquestTier = args[i + 1];
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

                var mode = string.IsNullOrWhiteSpace(gameMode) ? GetModeSelection() : gameMode.ToLower() switch { "c" => 1, "conquest" => 1, "l" => 2, "ladder" => 2, "r" => 2, "ranked" => 2, _ => GetModeSelection() };
                GameState maxTier = GameState.UNKNOWN;

                if (mode == 1)
                {
                    if (string.IsNullOrWhiteSpace(maxConquestTier))
                        maxTier = GetMaxTierSelection();
                    else
                        maxTier = maxConquestTier.ToLower() switch
                        {
                            "pg" => GameState.CONQUEST_LOBBY_PG,
                            "proving grounds" => GameState.CONQUEST_LOBBY_PG,
                            "s" => GameState.CONQUEST_LOBBY_SILVER,
                            "silver" => GameState.CONQUEST_LOBBY_SILVER,
                            "g" => GameState.CONQUEST_LOBBY_GOLD,
                            "gold" => GameState.CONQUEST_LOBBY_GOLD,
                            "i" => GameState.CONQUEST_LOBBY_INFINITE,
                            "infinite" => GameState.CONQUEST_LOBBY_INFINITE,
                            _ => GetMaxTierSelection()
                        };
                }
                else if (mode == 3)
                    maxTurns = 10;

                var retreatAfterTurn = maxTurns > -1 ? maxTurns : GetRetreatAfterTurn();

                PrintTitle();

                IBoosterBot bot = mode switch
                {
                    1 => new ConquestBot(scaling, verbose, autoplay, saveScreens, maxTier, retreatAfterTurn),
                    2 => new LadderBot(scaling, verbose, autoplay, saveScreens, retreatAfterTurn),
                    3 => new LadderBot(scaling, verbose, autoplay, saveScreens, retreatAfterTurn, true),
                    _ => throw new Exception("Invalid mode selection.")
                };
 
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
        version = version.Split('+')[0];

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
        Console.WriteLine("[2] Ranked Ladder Farm");
        Console.WriteLine("[3] Ranked Autoclimb (Beta)");
        Console.WriteLine();
        Console.Write("Waiting for selection...");

        var key = Console.ReadKey();
        if (key.KeyChar == '1' || key.KeyChar == '2' || key.KeyChar == '3')
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

	private static int GetRetreatAfterTurn()
	{
		PrintTitle();
		Console.WriteLine("Retreat after turn:\n");
		Console.WriteLine("[0] Do not auto retreat");
		Console.WriteLine("[1]");
		Console.WriteLine("[2]");
		Console.WriteLine("[3]");
		Console.WriteLine("[4]");
		Console.WriteLine("[5]");
		Console.WriteLine("[6]");
		Console.WriteLine("[7]");
		Console.WriteLine();
		Console.Write("Waiting for selection...");

		var key = Console.ReadKey();
        if ("0,1,2,3,4,5,6,7".Contains(key.KeyChar.ToString()))
        {
            var turn = int.Parse(key.KeyChar.ToString());

            if (turn < 1 || turn > 7)
            {
                turn = 99;
            }

            return turn;
        }

		return GetRetreatAfterTurn();
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