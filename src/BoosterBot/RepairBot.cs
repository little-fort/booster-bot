using BoosterBot.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BoosterBot
{
    internal class RepairBot : BaseBot
    {
        private static Process? _currentViewer;

        public RepairBot(double scaling, bool verbose, bool autoplay, bool saveScreens, int retreatAfterTurn, bool downscaled, bool useEvent = false) :
            base(GameMode.REPAIR, scaling, verbose, autoplay, saveScreens, retreatAfterTurn, downscaled, useEvent) { }

        public override void Run()
        {
            if (!ConfirmRepair())
                return;

            StartRepair();
        }

        public void StartRepair(bool firstRun = true)
        {
            var mode = SelectMode(firstRun);
            var prompts = new List<RepairPrompt>();
            if (mode == 1) // Ladder
                prompts =
                [
                    new("Navigate to the game's main menu and identify the 'Play' button.", [ComponentMappings.REF_LADD_BTN_PLAY], _game.CanIdentifyMainMenu),
                    new("Press 'Play' to start matchmaking and identify the 'Cancel' button.", [ComponentMappings.REF_LADD_BTN_MATCHMAKING_1], _game.CanIdentifyLadderMatchmaking),
                    new("While in a match, identify the 'Retreat' button.", [ComponentMappings.REF_LADD_BTN_RETREAT], _game.CanIdentifyLadderRetreatBtn),
                    new("Play cards to spend energy, then identify the indicator for zero remaining energy.", [ComponentMappings.REF_ICON_ZERO_ENERGY], _game.CanIdentifyZeroEnergy),
                    new("Identify the 'End Turn' button.", [ComponentMappings.REF_CONQ_BTN_END_TURN], _game.CanIdentifyEndTurnBtn),
                    new("After ending a turn, identify the 'Undo End Turn' button.", [ComponentMappings.REF_CONQ_BTN_WAITING_1], _game.CanIdentifyMidTurn),
                    new("(Optional) After ending a turn, identify the 'Waiting...' button. This text is\n" +
                        "only displayed when the player has ended their turn after Agatha Harkness played\n" +
                        "a card, or when waiting for Daredevil's ability.", [ComponentMappings.REF_CONQ_BTN_WAITING_2], _game.CanIdentifyMidTurn),
                    new("When cards are being played, identify when the button's text shows 'Playing...'.", [ComponentMappings.REF_CONQ_BTN_PLAYING], _game.CanIdentifyMidTurn),
                    new("While in a match, close the game client. Restart the game client and wait for the\n" +
                        "main menu to load. Identify the 'Reconnect to Game' button.", [ComponentMappings.REF_BTN_RECONNECT_TO_GAME], _game.CanIdentifyReconnectToGameBtn),
                    new("Play a match to the end, then identify the 'Collect Rewards' button shown after the final turn.", [ComponentMappings.REF_LADD_BTN_COLLECT_REWARDS], _game.CanIdentifyLadderCollectRewardsBtn),
                    new("Press 'Collect Rewards', then identify the 'Next' button.", [ComponentMappings.REF_LADD_BTN_MATCH_END_NEXT], _game.CanIdentifyLadderMatchEndNextBtn)
                ];
            else if (mode == 2) // Conquest
                prompts =
                [
                    new("Navigate to the game's main menu and identify the 'Play' button.", [ComponentMappings.REF_CONQ_BTN_PLAY], _game.CanIdentifyMainMenu),
                    new("Navigate to the Conquest menu and identify the 'Infinity Conquest' lobby.", [ComponentMappings.REF_CONQ_LBL_LOBBY_INFINITE_1, ComponentMappings.REF_CONQ_LBL_LOBBY_INFINITE_3], _game.CanIdentifyConquestLobbyInfinite),
                    new("Identify the 'Gold Conquest' lobby.", [ComponentMappings.REF_CONQ_LBL_LOBBY_GOLD_1, ComponentMappings.REF_CONQ_LBL_LOBBY_GOLD_3], _game.CanIdentifyConquestLobbyGold),
                    new("Identify the 'Silver Conquest' lobby.", [ComponentMappings.REF_CONQ_LBL_LOBBY_SILVER_1, ComponentMappings.REF_CONQ_LBL_LOBBY_SILVER_3], _game.CanIdentifyConquestLobbySilver),
                    new("Identify the 'Proving Grounds' lobby.", [ComponentMappings.REF_CONQ_LBL_LOBBY_PG_1, ComponentMappings.REF_CONQ_LBL_LOBBY_PG_2, ComponentMappings.REF_CONQ_LBL_ENTRANCE_FEE], _game.CanIdentifyConquestLobbyPG),
                    new("Identify any lobby with no available tickets.", [ComponentMappings.REF_CONQ_LBL_NO_TICKETS], _game.CanIdentifyConquestNoTickets),
                    new("Enter the 'Proving Grounds' lobby and identify the 'Play' button.", [ComponentMappings.REF_CONQ_BTN_PLAY], _game.CanIdentifyConquestPlayBtn),
                    new("Press 'Play' to start matchmaking and identify the 'Cancel' button.", [ComponentMappings.REF_CONQ_BTN_MATCHMAKING_1], _game.CanIdentifyConquestMatchmaking),
                    new("While in a match, identify the 'Retreat' button.", [ComponentMappings.REF_CONQ_BTN_RETREAT_1], _game.CanIdentifyConquestRetreatBtn),
                    new("Play cards to spend energy, then identify the indicator for zero remaining energy.", [ComponentMappings.REF_ICON_ZERO_ENERGY], _game.CanIdentifyZeroEnergy),
                    new("Identify the 'End Turn' button.", [ComponentMappings.REF_CONQ_BTN_END_TURN], _game.CanIdentifyEndTurnBtn),
                    new("After ending a turn, identify the 'Undo End Turn' button.", [ComponentMappings.REF_CONQ_BTN_WAITING_1], _game.CanIdentifyMidTurn),
                    new("When cards are being played, identify when the button's text shows 'Playing...'.", [ComponentMappings.REF_CONQ_BTN_PLAYING], _game.CanIdentifyMidTurn),
                    new("While in a match, close the game client. Restart the game client and wait for the\n" +
                        "main menu to load. Identify the 'Reconnect to Game' button.", [ComponentMappings.REF_BTN_RECONNECT_TO_GAME], _game.CanIdentifyReconnectToGameBtn),
                    new("After completing a round, identify the 'Concede' button.", [ComponentMappings.REF_CONQ_BTN_CONCEDE_1], _game.CanIdentifyConquestConcede),
                    new("After completing the final round, identify the 'Next' button.", [ComponentMappings.REF_CONQ_BTN_MATCH_END_1], _game.CanIdentifyConquestMatchEndNext1),
                    new("While collecting rewards, identify the 'Next' button.,", [ComponentMappings.REF_CONQ_BTN_MATCH_END_2], _game.CanIdentifyConquestMatchEndNext2),
                    new("After winning a match and returning to the Conquest menu, identify the 'Next' button.", [ComponentMappings.REF_CONQ_BTN_WIN_NEXT], _game.CanIdentifyConquestWinNext),
                    new("After winning a match and returning to the Conquest menu, identify the 'Claim Ticket' button.", [ComponentMappings.REF_CONQ_BTN_WIN_TICKET], _game.CanIdentifyConquestTicketClaim),
                    // new("After losing a match and returning to the Conquest menu, identify the 'Continue' button.", [ComponentMappings.REF_CONQ_BTN_CONTINUE], _game.CanIdentifyConquestLossContinue)
                ];

            for (int i = 0; i < prompts.Count; i++)
            {
                var prompt = prompts[i];
                if(!prompt.Description.StartsWith('['))
                    prompt.Description = $"[{i + 1}/{prompts.Count}] {prompt.Description}";

                if (!ShowRepairPrompt(prompt))
                {
                    if (i > 0)
                        i -= 2;
                    else
                        i--;
                }
            }

            StartRepair(false);
        }

        private void PrintTitle()
        {
            Program.PrintTitle();
            var title = $"REPAIR MODE";
            title = title.PadLeft(24 + (title.Length / 2), ' ');
            title = $"{title}{"".PadRight(49 - title.Length, ' ')}";
            Console.WriteLine(title);
            Console.WriteLine();
            Console.WriteLine("****************************************************");
            Console.WriteLine();
        }

        public static void ShowImage(string image)
        {
            var assemblyLocation = Assembly.GetExecutingAssembly().Location;
            var assemblyDirectory = Path.GetDirectoryName(assemblyLocation);
            var imagePath = Path.Combine(Path.Combine(assemblyDirectory ?? "", "Resources", "repair"), image);

            if (!File.Exists(imagePath))
            {
                Console.WriteLine($"\nERROR: Example image not found: {image}");
                return;
            }

            try
            {
                // Close any existing viewer
                CleanupViewer();

                // Start the image viewer
                _currentViewer = Process.Start(new ProcessStartInfo
                {
                    FileName = imagePath,
                    UseShellExecute = true,
                    Verb = "open"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nError showing image: {ex.Message}");
                Logger.Log($"Image display error: {ex.Message}", "logs\\repair.txt");
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

        private bool ConfirmRepair()
        {
            PrintTitle();
            Console.WriteLine("WARNING:");
            Console.WriteLine("This mode is intended to repair BoosterBot's detection of critical parts of the");
            Console.WriteLine("game's interface. You should only proceed if BoosterBot cannot function properly.");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("Are you sure you want to continue?\n");
            Console.WriteLine("[1] Yes");
            Console.WriteLine("[2] No");
            Console.WriteLine();
            Console.Write("Waiting for selection...");

            var key = Console.ReadKey();
            if (key.KeyChar == '1')
                return true;
            else if (key.KeyChar == '2')
                return false;

            return ConfirmRepair();
        }

        private int SelectMode(bool firstRun)
        {
            PrintTitle();
            if (firstRun)
            {
                Console.WriteLine("To complete the process of repairing detection, you will manually interact with");
                Console.WriteLine("the game until specific UI components are visible (e.g. buttons, icons, or text labels).");
                Console.WriteLine();
                Console.WriteLine("When you confirm that the specified UI component is on the screen, this program will");
                Console.WriteLine("capture the current image and use it for future reference.");
                Console.WriteLine();
                Console.WriteLine("Both game types must be repaired separately. Please select the mode you would like to repair:\n");
            }
            else
                Console.WriteLine("Repair process complete. You may select an additional mode to repair, or close this window and restart BoosterBot normally.\n");

            Console.WriteLine("[1] Ladder");
            Console.WriteLine("[2] Conquest");
            Console.WriteLine();
            Console.Write("Waiting for selection...");

            var key = Console.ReadKey();
            if (key.KeyChar == '1' || key.KeyChar == '2')
                return int.Parse(key.KeyChar.ToString());

            return SelectMode(firstRun);
        }

        private bool ShowRepairPrompt(RepairPrompt prompt)
        {
            PrintTitle();
            Console.WriteLine(prompt.Description);
            Console.WriteLine();
            Console.WriteLine("When the specified UI component is visible on the screen, press [2] to confirm.");
            Console.WriteLine();
            Console.WriteLine("[0] Previous");
            Console.WriteLine("[1] Show example");
            Console.WriteLine("[2] Confirm");
            Console.WriteLine("[3] Skip");
            Console.WriteLine();
            Console.Write("Waiting for selection...\n\n");

            var key = Console.ReadKey();
            if (key.KeyChar == '0')
                return false;

            // Open the example image for reference
            if (key.KeyChar == '1')
                ShowImage(prompt.Files[0].Replace("-preproc", ""));
            else if (key.KeyChar == '2')
            {
                // Capture the current screen
                _config.GetWindowPositions();

                // Run the relevant CanIdentify method to crop an updated reference image
                prompt.Identify();

                // Copy the captured image to the reference directory
                foreach (var file in prompt.Files)
                {
                    var capPath = Path.Combine("screens", file);
                    var refPath = Path.Combine("reference", file);
                    File.Copy(capPath, refPath, true);
                }

                return true;
            }
            else if (key.KeyChar == '3')
                return true;

            return ShowRepairPrompt(prompt);
        }
    }
}
