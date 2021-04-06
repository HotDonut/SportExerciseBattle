using System;
using System.Collections.Generic;
using System.Text;

namespace SportsExerciseBattle.HelperObjects
{
    class TournamentInfo
    {
        public List<TournamentEntry> participants;
        public bool isActive;
        public int tournamentID;


        public TournamentInfo(List<TournamentEntry> participants, bool isActive, int tournamentID)
        {
            this.participants = participants;
            this.isActive = isActive;
            this.tournamentID = tournamentID;
        }
    }
}
