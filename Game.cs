using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo1
{
    enum GAME_PHASE
    {
        GAME_PHASE_READY,
        GAME_PHASE_START,
        GAME_PHASE_DRAW,
        GAME_PHASE_PRECOMBAT,
        GAME_PHASE_COMBAT,
        GAME_PHASE_END,
    }
    class Game
    {
        private List<Player> players = new List<Player>();
        
        private int currentRoundNo = 0;
        private GAME_PHASE currentPhase = 0;

        public void AddPlayer(Player player)
        {
            players.Add(player);
        }

        public GAME_PHASE GetCurrentPhase() { return currentPhase; }
        public void StartGame()
        {
            currentPhase = GAME_PHASE.GAME_PHASE_START;
            ShufflePlayerDeck();
            NextPhase();
        }

        public void NextTurn()
        {
            currentPhase = GAME_PHASE.GAME_PHASE_START;
            currentRoundNo++;
        }

        public void NextPhase()
        {
            currentPhase++;            
        }

        public void DrawPhase()
        {
            if (currentPhase != GAME_PHASE.GAME_PHASE_DRAW)
                return;

            foreach (var player in players)
            {
                for(int i = 0; i < 5; ++i)
                    player.DrawCard();
            }
        }

        //public void Mulligan(Player player, List<GameCard> list)
        public void MulliganAll(Player player)
        {
            if (currentPhase != GAME_PHASE.GAME_PHASE_DRAW)
                return;

            player.MulliganAll();
        }

        public void Mulligan(Player player, List<GameCard> list)
        {
            if (currentPhase != GAME_PHASE.GAME_PHASE_DRAW)
                return;

            player.Mulligan(list);
        }

        public void PreCombatPhase()
        {
            if (currentPhase != GAME_PHASE.GAME_PHASE_PRECOMBAT)
                return;


        }

        public bool CheckGameState()
        {
            bool ret = false;
            foreach (var player in players)
            {
                switch (GetCurrentPhase())
                {
                    case GAME_PHASE.GAME_PHASE_READY: break;
                    case GAME_PHASE.GAME_PHASE_START: break;
                    case GAME_PHASE.GAME_PHASE_DRAW: break;
                    case GAME_PHASE.GAME_PHASE_PRECOMBAT: break;
                    case GAME_PHASE.GAME_PHASE_COMBAT: break;
                }
            }
            return ret;
        }

        private void ShufflePlayerDeck()
        {
            foreach(var player in players)
            {
                player.ShuffleDeck();
            }
        }
    }
}
