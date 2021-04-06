using System;
using System.Collections.Generic;
using System.Text;

namespace SportsExerciseBattle.Database
{
    public interface IDatabaseHandler
    {
        void IncreaseELOafterTournament();
        long CountOccurrence(string fromTable, string columnName, string whereCondition);
        string GetUserdata(string token, string path);
    }
}
