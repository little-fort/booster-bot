using BoosterBot.Helpers;
using BoosterBot.Models;
using BoosterBot.Resources;
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
        private readonly ComponentMappings _maps;
        private static Process? _currentViewer;

        public RepairBot(BotConfig config) : base(config, 0)
        {
            _maps = new ComponentMappings(config);
        }

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
            if (mode == 2) // Ladder
                prompts =
                [
                    new(Strings.Repair_Ladder_BTN_PLAY, [_maps.REF_LADD_BTN_PLAY], _game.CanIdentifyMainMenu),
                    new(Strings.Repair_Ladder_BTN_MATCHMAKING, [_maps.REF_LADD_BTN_MATCHMAKING_1], _game.CanIdentifyLadderMatchmaking),
                    new(Strings.Repair_Ladder_BTN_RETREAT, [_maps.REF_LADD_BTN_RETREAT], _game.CanIdentifyLadderRetreatBtn),
                    new(Strings.Repair_Match_ZERO_ENERGY, [_maps.REF_ICON_ZERO_ENERGY], _game.CanIdentifyZeroEnergy),
                    new(Strings.Repair_Match_END_TURN, [_maps.REF_CONQ_BTN_END_TURN], _game.CanIdentifyEndTurnBtn),
                    new(Strings.Repair_Match_UNDO, [_maps.REF_CONQ_BTN_WAITING_1], _game.CanIdentifyMidTurn),
                    new(Strings.Repair_Match_BTN_WAITING_1 + Environment.NewLine +
                        Strings.Repair_Match_BTN_WAITING_2 + Environment.NewLine + 
                        "    " + Strings.Repair_Match_BTN_WAITING_3 + Environment.NewLine +
                        "    " + Strings.Repair_Match_BTN_WAITING_4, [_maps.REF_CONQ_BTN_WAITING_2], _game.CanIdentifyMidTurn),
                    new(Strings.Repair_Match_BTN_PLAYING, [_maps.REF_CONQ_BTN_PLAYING], _game.CanIdentifyMidTurn),
                    new(Strings.Repair_Match_RECONNECT_1 + Environment.NewLine +
                        Strings.Repair_Match_RECONNECT_2, [_maps.REF_BTN_RECONNECT_TO_GAME], _game.CanIdentifyReconnectToGameBtn),
                    new(Strings.Repair_Ladder_BTN_COLLECT_REWARDS, [_maps.REF_LADD_BTN_COLLECT_REWARDS], _game.CanIdentifyLadderCollectRewardsBtn),
                    new(Strings.Repair_Match_END_NEXT, [_maps.REF_LADD_BTN_MATCH_END_NEXT], _game.CanIdentifyLadderMatchEndNextBtn)
                ];
            else if (mode == 1) // Conquest
                prompts =
                [
                    new(Strings.Repair_Ladder_BTN_PLAY, [_maps.REF_CONQ_BTN_PLAY], _game.CanIdentifyMainMenu),
                    new(Strings.Repair_Conquest_LOBBY_INFINITE, [_maps.REF_CONQ_LBL_LOBBY_INFINITE_1, _maps.REF_CONQ_LBL_LOBBY_INFINITE_3], _game.CanIdentifyConquestLobbyInfinite),
                    new(Strings.Repair_Conquest_LOBBY_GOLD, [_maps.REF_CONQ_LBL_LOBBY_GOLD_1, _maps.REF_CONQ_LBL_LOBBY_GOLD_3], _game.CanIdentifyConquestLobbyGold),
                    new(Strings.Repair_Conquest_LOBBY_SILVER, [_maps.REF_CONQ_LBL_LOBBY_SILVER_1, _maps.REF_CONQ_LBL_LOBBY_SILVER_3], _game.CanIdentifyConquestLobbySilver),
                    new(Strings.Repair_Conquest_LOBBY_PG, [_maps.REF_CONQ_LBL_LOBBY_PG_1, _maps.REF_CONQ_LBL_LOBBY_PG_2, _maps.REF_CONQ_LBL_ENTRANCE_FEE], _game.CanIdentifyConquestLobbyPG),
                    new(Strings.Repair_Conquest_NO_TICKETS, [_maps.REF_CONQ_LBL_NO_TICKETS], _game.CanIdentifyConquestNoTickets),
                    new(Strings.Repair_Conquest_BTN_PLAY, [_maps.REF_CONQ_BTN_PLAY], _game.CanIdentifyConquestPlayBtn),
                    new(Strings.Repair_Ladder_BTN_MATCHMAKING, [_maps.REF_CONQ_BTN_MATCHMAKING_1], _game.CanIdentifyConquestMatchmaking),
                    new(Strings.Repair_Ladder_BTN_RETREAT, [_maps.REF_CONQ_BTN_RETREAT_1], _game.CanIdentifyConquestRetreatBtn),
                    new(Strings.Repair_Match_ZERO_ENERGY, [_maps.REF_ICON_ZERO_ENERGY], _game.CanIdentifyZeroEnergy),
                    new(Strings.Repair_Match_END_TURN, [_maps.REF_CONQ_BTN_END_TURN], _game.CanIdentifyEndTurnBtn),
                    new(Strings.Repair_Match_UNDO, [_maps.REF_CONQ_BTN_WAITING_1], _game.CanIdentifyMidTurn),
                    new(Strings.Repair_Match_BTN_PLAYING, [_maps.REF_CONQ_BTN_PLAYING], _game.CanIdentifyMidTurn),
                    new(Strings.Repair_Match_RECONNECT_1 + Environment.NewLine +
                        Strings.Repair_Match_RECONNECT_2, [_maps.REF_BTN_RECONNECT_TO_GAME], _game.CanIdentifyReconnectToGameBtn),
                    new(Strings.Repair_Conquest_BTN_CONCEDE, [_maps.REF_CONQ_BTN_CONCEDE_1], _game.CanIdentifyConquestConcede),
                    new(Strings.Repair_Conquest_BTN_MATCH_END, [_maps.REF_CONQ_BTN_MATCH_END_1], _game.CanIdentifyConquestMatchEndNext1),
                    new(Strings.Repair_Conquest_BTN_MATCH_END_NEXT, [_maps.REF_CONQ_BTN_MATCH_END_2], _game.CanIdentifyConquestMatchEndNext2),
                    new(Strings.Repair_Conquest_BTN_WIN_NEXT, [_maps.REF_CONQ_BTN_WIN_NEXT], _game.CanIdentifyConquestWinNext),
                    new(Strings.Repair_Conquest_BTN_WIN_TICKET, [_maps.REF_CONQ_BTN_WIN_TICKET], _game.CanIdentifyConquestTicketClaim),
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
            var title = Strings.Repair_Title;
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
                Console.WriteLine($"{Environment.NewLine} {Strings.Repair_Error_ExampleImageNotFound} {image}");
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
                Console.WriteLine($"{Environment.NewLine} {Strings.Repair_Error_CantShowImage} {ex.Message}");
                // Log($"Image display error: {ex.Message}", "logs\\repair.txt");
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
            Console.WriteLine(Strings.Repair_Confirm_Warning);
            Console.WriteLine(Strings.Repair_Confirm_Description1);
            Console.WriteLine(Strings.Repair_Confirm_Description2);
            Console.WriteLine();
            Console.WriteLine(Strings.Menu_ConfirmContinue + Environment.NewLine);
            Console.WriteLine(Strings.Menu_Option1_Yes);
            Console.WriteLine(Strings.Menu_Option2_No);
            Console.WriteLine();
            Console.Write(Strings.Menu_WaitingForSelection);

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
                Console.WriteLine(Strings.Repair_ModeSelect_Description1);
                Console.WriteLine();
                Console.WriteLine(Strings.Repair_ModeSelect_Description2);
                Console.WriteLine();
                Console.WriteLine(Strings.Repair_ModeSelect_Description1 + Environment.NewLine);
                Console.WriteLine();

                // TODO: Uncomment for future releases
                // Console.WriteLine("NOTE: If you have run the repair process on a previous version of BoosterBot, copying the 'reference' directory from that version to the current version may solve any detection problems.");
            }
            else
                Console.WriteLine(Strings.Repair_ModeSelect_ProcessComplete + Environment.NewLine);

            Console.WriteLine(Strings.Menu_ModeSelect_Option1);
            Console.WriteLine(Strings.Menu_ModeSelect_Option2);
            Console.WriteLine();
            Console.Write(Strings.Menu_WaitingForSelection);

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
            Console.WriteLine(Strings.Repair_Prompt_PressToConfirm);
            Console.WriteLine();
            Console.WriteLine(Strings.Repair_Prompt_Option0_Previous);
            Console.WriteLine(Strings.Repair_Prompt_Option1_Example);
            Console.WriteLine(Strings.Repair_Prompt_Option2_Confirm);
            Console.WriteLine(Strings.Repair_Prompt_Option3_Skip);
            Console.WriteLine();
            Console.Write(Strings.Menu_WaitingForSelection);
            Console.WriteLine(Environment.NewLine);

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
