using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoosterBot.Models
{
    internal enum GameState
    {
        UNKNOWN,
        MAIN_MENU,
        MID_MATCH,
        LADDER_MATCHMAKING,
        LADDER_MATCH,
        LADDER_MATCH_END,
        LADDER_MATCH_END_REWARDS,
        CONQUEST_LOBBY_PG,
        CONQUEST_PREMATCH,
        CONQUEST_MATCHMAKING,
        CONQUEST_MATCH,
        CONQUEST_ROUND_END,
        CONQUEST_MATCH_END,
        CONQUEST_MATCH_END_REWARDS,
        CONQUEST_POSTMATCH_LOSS_SCREEN,
        CONQUEST_POSTMATCH_WIN_CONTINUE,
        CONQUEST_POSTMATCH_WIN_TICKET
    }
}
