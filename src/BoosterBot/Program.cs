using BoosterBot.Helpers;
using BoosterBot.Models;
using BoosterBot.Resources;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.Reflection;

namespace BoosterBot;

internal class Program
{
    private static bool _updateAvailable = false;
    private static LocalizationManager _localizer;

    static async Task Main(string[] args)
    {
        bool masked = true; // REVERT BEFORE PR
        bool verbose = false;
        bool autoplay = true;
        bool saveScreens = false;
        bool downscaled = false;
        bool ltm = false;
        bool repair = false;
        double scaling = 1.0;
        string gameMode = "";
        string maxConquestTier = "";
        int maxTurns = 0;

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
                    case "--masked":
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
                    case "-d":
                    case "--downscaled":
                        downscaled = true;
                        break;
                    case "-e":
                    case "--event":
                        ltm = true;
                        break;
                    case "-r":
                    case "--repair":
                        repair = true;
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
                // Setup configuration
                IConfiguration configuration = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .Build();

                // Initialize localization
                _localizer = new LocalizationManager(configuration);

                // Initialize hotkey manager to allow pausing or exiting via keyboard shortcuts
                HotkeyManager.Initialize(_localizer);

                // Check for updates
                _updateAvailable = await UpdateChecker.CheckForUpdates();

                // Set encoding
                Console.OutputEncoding = System.Text.Encoding.UTF8;

                if (!Directory.Exists("screens"))
                    Directory.CreateDirectory("screens");

                var mode = 0;
                if (repair)
                    mode = 9;
                else if (!string.IsNullOrWhiteSpace(gameMode))
                    mode = gameMode.ToLower() switch { "c" => 1, "conquest" => 1, "l" => 2, "ladder" => 2, "r" => 2, "ranked" => 2, _ => GetModeSelection() };
                else
                    mode = GetModeSelection();

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

                var retreat = maxTurns > 0 || repair ? maxTurns : GetRetreatAfterTurn();

                PrintTitle();

                var type = (GameMode)mode;
                var logPath = $"logs\\{type.ToString().ToLower()}-log-{DateTime.Now.ToString("yyyyMMddHHmmss")}.txt";
                var config = new BotConfig(_localizer, scaling, verbose, autoplay, saveScreens, logPath, ltm, downscaled);
                IBoosterBot bot = mode switch
                {
                    1 => new ConquestBot(config, retreat, maxTier),
                    2 => new LadderBot(config, retreat),
                    3 => new EventBot(config, retreat),
                    9 => new RepairBot(config),
                    _ => throw new Exception(_localizer.GetString("Log_InvalidModeSelection"))
                };
 
                try
                {
                    bot.Run();
                }
                catch (Exception ex)
                {
                    Logger.Log(_localizer, "Log_FatalError", bot.GetLogPath());
                    Logger.Log(_localizer, ex.Message, bot.GetLogPath(), true);
                    Console.WriteLine();
                    Logger.Log(_localizer, ex.StackTrace, bot.GetLogPath(), true);
                }

                Console.WriteLine(Environment.NewLine + _localizer.GetString("Menu_PressKeyToExit"));
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
            Logger.Log(_localizer, "Log_FatalError", log);
            Logger.Log(_localizer, ex.Message, log, true);
            Console.WriteLine();
            Logger.Log(_localizer, ex.StackTrace, log, true);
        }
    }

    internal static void PrintTitle()
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

        // Show update notification if available
        if (_updateAvailable)
            Console.WriteLine(UpdateChecker.GetUpdateMessage());

        Console.WriteLine(Strings.Menu_ModeSelect_Description + Environment.NewLine);
        Console.WriteLine(_localizer.GetString("Menu_ModeSelect_Option1"));
        Console.WriteLine(_localizer.GetString("Menu_ModeSelect_Option2"));
        Console.WriteLine(_localizer.GetString("Menu_ModeSelect_Option3"));
        Console.WriteLine();
        Console.Write(_localizer.GetString("Menu_WaitingForSelection"));

        var key = Console.ReadKey();
        if (_updateAvailable && key.KeyChar == '0')
            UpdateChecker.OpenReleasePage();
        else if (key.KeyChar == '1' || key.KeyChar == '2' || key.KeyChar == '3')
            return int.Parse(key.KeyChar.ToString());

        return GetModeSelection();
    }

    private static GameState GetMaxTierSelection()
    {
        PrintTitle();
        Console.WriteLine(_localizer.GetString("Menu_ConquestLobby_Description") + Environment.NewLine);
        Console.WriteLine(_localizer.GetString("Menu_ConquestLobby_Option1"));
        Console.WriteLine(_localizer.GetString("Menu_ConquestLobby_Option2"));
        Console.WriteLine(_localizer.GetString("Menu_ConquestLobby_Option3"));
        Console.WriteLine(_localizer.GetString("Menu_ConquestLobby_Option4"));
        Console.WriteLine();
        Console.WriteLine(_localizer.GetString("Menu_ConquestLobby_NoticeTickets") + Environment.NewLine + _localizer.GetString("Menu_ConquestLobby_NoticeGold"));
        Console.WriteLine();
        Console.Write(_localizer.GetString("Menu_WaitingForSelection"));

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
        Console.WriteLine($"{_localizer.GetString("Menu_ConquestConfirm_HighestTier")} {tier}");
        Console.WriteLine();
        Console.WriteLine(_localizer.GetString("Menu_ConquestConfirm_TicketWarning"));
        Console.WriteLine(_localizer.GetString("Menu_ConquestConfirm_TicketDefault"));
        Console.WriteLine();
        Console.WriteLine(_localizer.GetString("Menu_ConfirmContinue") + Environment.NewLine);
        Console.WriteLine(_localizer.GetString("Menu_Option1_Yes"));
        Console.WriteLine(_localizer.GetString("Menu_Option2_No"));
        Console.WriteLine();
        Console.Write(_localizer.GetString("Menu_WaitingForSelection"));

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
		Console.WriteLine(_localizer.GetString("Menu_RetreatSelect_Description") + Environment.NewLine);
		Console.WriteLine(_localizer.GetString("Menu_RetreatSelect_NoRetreat"));
		Console.WriteLine("[1]");
		Console.WriteLine("[2]");
		Console.WriteLine("[3]");
		Console.WriteLine("[4]");
		Console.WriteLine("[5]");
		Console.WriteLine();
        Console.Write(_localizer.GetString("Menu_WaitingForSelection"));

        var key = Console.ReadKey();
        if ("0,1,2,3,4,5".Contains(key.KeyChar.ToString()))
        {
            var turn = int.Parse(key.KeyChar.ToString());

            if (turn < 1 || turn > 7)
                turn = 99;

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