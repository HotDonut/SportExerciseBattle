using System;
using System.Collections.Generic;
using System.Text;

namespace SportsExerciseBattle.HelperObjects
{
    class TournamentEntry
    {
        public string username;
        public long Count;
        public int ELO;

        public TournamentEntry(string username, long count, int elo)
        {
            this.username = username;
            this.Count = count;
            this.ELO = elo;
        }
    }
}
