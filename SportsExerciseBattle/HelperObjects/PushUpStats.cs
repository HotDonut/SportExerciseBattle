using System;
using System.Collections.Generic;
using System.Text;

namespace SportsExerciseBattle.HelperObjects
{
    public class PushUpStats
    {
        public int userID;
        public long Count;
        public int ELO;

        public PushUpStats(int _userID, long _count, int _elo)
        {
            userID = _userID;
            Count = _count;
            ELO = _elo;
        }
    }
}
