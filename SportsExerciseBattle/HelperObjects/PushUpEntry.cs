using System;
using System.Collections.Generic;
using System.Text;

namespace SportsExerciseBattle.HelperObjects
{
    public class PushUpEntry
    {
        public int TournamentID;
        public int Count;
        public int Duration;

        public PushUpEntry(int _tournamentid, int _count, int _duration)
        {
            TournamentID = _tournamentid;
            Count = _count;
            Duration = _duration;
        }
    }
}
