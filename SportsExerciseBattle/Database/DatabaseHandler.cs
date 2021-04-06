using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Npgsql;
using Newtonsoft.Json;
using SportsExerciseBattle.HelperObjects;

namespace SportsExerciseBattle.Database
{
    public class DatabaseHandler
    {
        static readonly string ConnectionString = "Host=localhost;Username=postgres;Password=lglglg;Database=postgres";

        public static int RegRequest(string username, string password)
        {
            long userAlreadyExists = CountOccurrence("seb_users", "username", username);
            if (userAlreadyExists >= 1) return -1;

            var token = "Basic " + username + "-sebToken";

            using var connection = new NpgsqlConnection(ConnectionString);
            connection.Open();

            using var insert = new NpgsqlCommand();
            insert.Connection = connection;
            insert.CommandText = $"INSERT INTO SEB_Users (username, password, elo, token) VALUES ('{username}', '{password}', '100', '{token}')";
            insert.ExecuteNonQuery();

            using var getID = new NpgsqlCommand();
            getID.Connection = connection;
            getID.CommandText = $"SELECT userID from SEB_Users WHERE username = '{username}' AND password = '{password}'";
            long userID = (long)getID.ExecuteScalar();

            using var createProfile = new NpgsqlCommand();
            createProfile.Connection = connection;
            createProfile.CommandText = $"INSERT INTO SEB_Profile (userID, name, bio, image) VALUES ('{userID}', '{username}', 'default', 'default')";
            createProfile.ExecuteNonQuery();

            return 0;
        }

        public static string LoginRequest(string username, string password)
        {
            using var connection = new NpgsqlConnection(ConnectionString);
            connection.Open();

            var fromTable = "seb_users";

            string sql1 = $"SELECT Count(*) FROM {fromTable} WHERE username = '{username}' AND password = '{password}'";
            using var cmd1 = new NpgsqlCommand(sql1, connection);

            long exi = (long)cmd1.ExecuteScalar();

            if (exi <= 0) return "-1";

            string sql2 = $"SELECT token FROM {fromTable} WHERE username = '{username}' AND password = '{password}'";
            using var cmd2 = new NpgsqlCommand(sql2, connection);

            string token = (string)cmd2.ExecuteScalar();

            return token;
        }

        public static long CountOccurrence(string fromTable, string columnName, string whereCondition)
        {
            using var con = new NpgsqlConnection(ConnectionString);
            con.Open();

            string sql = $"SELECT Count(*) FROM {fromTable} WHERE {columnName} = '{whereCondition}'";
            using var cmd = new NpgsqlCommand(sql, con);

            long exi = (long)cmd.ExecuteScalar();

            return exi;
        }

        public static string GetUserdata(string token, string path)
        {
            string[] pathFilter = path.Split("/");

            long userID = GetIDfromToken(token);

            if (userID == -1)
            {
                return "-1";
            }

            using var conComp = new NpgsqlConnection(ConnectionString);
            conComp.Open();

            string sqlComp = $"SELECT userID FROM seb_users WHERE username = '{pathFilter[2]}'";
            using var cmdComp = new NpgsqlCommand(sqlComp, conComp);

            long personIdComp = 0;

            if ((cmdComp.ExecuteScalar() == DBNull.Value) || (cmdComp.ExecuteScalar() == null)) return "-1";
            else personIdComp = (long)cmdComp.ExecuteScalar();
            conComp.Close();

            if (userID != personIdComp)
            {
                return "-1";
            }

            using var con2 = new NpgsqlConnection(ConnectionString);
            con2.Open();

            string sql2 = $"SELECT * FROM seb_profile WHERE userID = {userID}";
            using var cmd2 = new NpgsqlCommand(sql2, con2);

            using var reader = cmd2.ExecuteReader();
            reader.Read();

            string name = reader.GetString(reader.GetOrdinal("name"));
            string bio = reader.GetString(reader.GetOrdinal("bio"));
            string image = reader.GetString(reader.GetOrdinal("image"));

            con2.Close();

            var tmpObject = new ProfileToJSON(name, bio, image);

            return JsonConvert.SerializeObject(tmpObject);
        }


        public static int PutUserdata(string token, string path, dynamic data)
        {
            string[] pathFilter = path.Split("/");

            long userIDtoken = GetIDfromToken(token);

            if (userIDtoken == -1)
            {
                return -1;
            }



            using var conComp = new NpgsqlConnection(ConnectionString);
            conComp.Open();

            string sqlComp = $"SELECT userid FROM seb_users WHERE username = '{pathFilter[2]}'";
            using var cmdComp = new NpgsqlCommand(sqlComp, conComp);

            long userIDname = 0;

            if ((cmdComp.ExecuteScalar() == DBNull.Value) || (cmdComp.ExecuteScalar() == null)) return -1;
            else userIDname = (long)cmdComp.ExecuteScalar();
            conComp.Close();

            if (userIDtoken != userIDname)
            {
                return -1;
            }


            using var conUpdate = new NpgsqlConnection(ConnectionString);
            conUpdate.Open();

            string sqlUpdate = $"UPDATE seb_profile SET name = '{data["Name"]}', bio = '{data["Bio"]}', image = '{data["Image"]}' WHERE userID = '{userIDtoken}'";
            using var cmdUpdate = new NpgsqlCommand(sqlUpdate, conUpdate);

            cmdUpdate.ExecuteNonQuery();
            conUpdate.Close();

            return 0;
        }

        public static int PushUpEntry(string token, dynamic data, bool activeTournament)
        {
            long userID = GetIDfromToken(token);

            if (userID == -1)
            {
                return -1;
            }


            using var conSelect = new NpgsqlConnection(ConnectionString);
            conSelect.Open();

            string sqlSelect = $"SELECT MAX(tournamentID) FROM SEB_History";
            using var cmdSelect = new NpgsqlCommand(sqlSelect, conSelect);

            int tournamentID;

            if ((cmdSelect.ExecuteScalar() == DBNull.Value) || (cmdSelect.ExecuteScalar() == null)) tournamentID = 0;
            else tournamentID = (int)cmdSelect.ExecuteScalar();
            conSelect.Close();


            if (!activeTournament)
            {
                tournamentID++;
            }

            using var conInsert = new NpgsqlConnection(ConnectionString);
            conInsert.Open();


            string sqlInsert = $"INSERT INTO SEB_History (userID, tournamentID, count, duration) VALUES ('{userID}', '{tournamentID}', '{data["Count"]}', '{data["DurationInSeconds"]}')";
            using var cmdInsert = new NpgsqlCommand(sqlInsert, conInsert);

            cmdInsert.ExecuteNonQuery();
            conInsert.Close();

            return 0;
        }

        public static string DisplayAllEntries(string token, dynamic data)
        {
            long userID = GetIDfromToken(token);

            if (userID == -1)
            {
                return "-1";
            }


            using var conSelect = new NpgsqlConnection(ConnectionString);
            conSelect.Open();

            string sqlSelect = $"SELECT tournamentID, count, duration FROM SEB_History WHERE userID = '{userID}'";
            using var cmdSelect = new NpgsqlCommand(sqlSelect, conSelect);

            using var reader = cmdSelect.ExecuteReader();

            var pushUpEntryList = new List<PushUpEntry>();

            while (reader.Read())
            {
                int tournamentID = reader.GetInt32(reader.GetOrdinal("tournamentid"));
                int count = reader.GetInt32(reader.GetOrdinal("count"));
                int duration = reader.GetInt32(reader.GetOrdinal("duration"));

                var tmpObject = new PushUpEntry(tournamentID, count, duration);

                pushUpEntryList.Add(tmpObject);
            }

            return JsonConvert.SerializeObject(pushUpEntryList);
        }


        public static string GetStats(string token, dynamic data)
        {
            long userID = GetIDfromToken(token);

            if (userID == -1)
            {
                return "-1";
            }


            if (GetPushUpSumAndElo(userID) == null)
            {
                return "-1";
            }

            var tmpObject = GetPushUpSumAndElo(userID);


            return JsonConvert.SerializeObject(tmpObject);
        }


        public static string GetScoreboard(string token, dynamic data)
        {
            long userID = GetIDfromToken(token);

            if (userID == -1)
            {
                return "-1";
            }


            using var conSelect = new NpgsqlConnection(ConnectionString);
            conSelect.Open();

            string sqlSelect = $"SELECT userID FROM SEB_Users";
            using var cmdSelect = new NpgsqlCommand(sqlSelect, conSelect);

            using var reader = cmdSelect.ExecuteReader();

            var PushUpStatList = new List<PushUpStats>();

            while (reader.Read())
            {
                int tempUserID = reader.GetInt32(reader.GetOrdinal("userID"));


                if (GetPushUpSumAndElo(tempUserID) != null)
                {
                    var tmpObject = GetPushUpSumAndElo(tempUserID);
                    PushUpStatList.Add(tmpObject);
                }

            }
            conSelect.Close();



            return JsonConvert.SerializeObject(PushUpStatList);
        }

        public static string GetTournamentInfo(string token, dynamic data, bool isActive)
        {
            long userID = GetIDfromToken(token);

            if (userID == -1)
            {
                return "-1";
            }


            using var conSelect = new NpgsqlConnection(ConnectionString);
            conSelect.Open();

            string sqlSelect = $"SELECT DISTINCT tournamentID FROM SEB_History WHERE userid = {userID}";
            using var cmdSelect = new NpgsqlCommand(sqlSelect, conSelect);

            using var reader = cmdSelect.ExecuteReader();

            var TournamentInfoList = new List<TournamentInfo>();


            while (reader.Read())
            {
                int tempTournamentID = reader.GetInt32(reader.GetOrdinal("tournamentID"));

                using var conSelect2 = new NpgsqlConnection(ConnectionString);
                conSelect2.Open();

                string sqlSelect2 = $"SELECT username, SUM(count) as count, SUM(duration) as duration FROM SEB_History INNER JOIN SEB_Users ON SEB_History.userID = SEB_Users.userID WHERE tournamentID = {tempTournamentID} GROUP BY username";
                using var cmdSelect2 = new NpgsqlCommand(sqlSelect2, conSelect2);

                using var reader2 = cmdSelect2.ExecuteReader();

                var TournamentEntryList = new List<TournamentEntry>();

                while (reader2.Read())
                {

                    string tempUsername = reader2.GetString(reader2.GetOrdinal("username"));
                    int tempCount = reader2.GetInt32(reader2.GetOrdinal("count"));
                    int tempDuration = reader2.GetInt32(reader2.GetOrdinal("duration"));

                    var tempObject = new TournamentEntry(tempUsername, tempCount, tempDuration);
                    TournamentEntryList.Add(tempObject);
                }

                var tempObject2 = new TournamentInfo(TournamentEntryList, false, tempTournamentID);
                TournamentInfoList.Add(tempObject2);

                conSelect2.Close();
            }
            conSelect.Close();

            if (isActive)
            {
                TournamentInfoList.LastOrDefault().isActive = true;
            }



            return JsonConvert.SerializeObject(TournamentInfoList);
        }

        public static PushUpStats GetPushUpSumAndElo(long userID)
        {
            using var conSelect = new NpgsqlConnection(ConnectionString);
            conSelect.Open();

            string sqlSelect = $"SELECT SUM(count) FROM SEB_History WHERE userID = '{userID}'";
            using var cmdSelect = new NpgsqlCommand(sqlSelect, conSelect);

            long PushUpSum;

            if ((cmdSelect.ExecuteScalar() == DBNull.Value) || (cmdSelect.ExecuteScalar() == null)) PushUpSum = 0;
            else PushUpSum = (long)cmdSelect.ExecuteScalar();
            conSelect.Close();

            using var conSelect2 = new NpgsqlConnection(ConnectionString);
            conSelect2.Open();

            string sqlSelect2 = $"SELECT elo FROM SEB_users WHERE userID = '{userID}'";
            using var cmdSelect2 = new NpgsqlCommand(sqlSelect2, conSelect2);

            int ELO;

            if ((cmdSelect2.ExecuteScalar() == DBNull.Value) || (cmdSelect2.ExecuteScalar() == null)) return null;
            else ELO = (int)cmdSelect2.ExecuteScalar();
            conSelect2.Close();

            var tmpObject = new PushUpStats((int)userID, PushUpSum, ELO);
            return tmpObject;
        }

        public static long GetIDfromToken(string token)
        {
            using var connection = new NpgsqlConnection(ConnectionString);
            connection.Open();

            string sql = $"SELECT userID FROM seb_users WHERE token = '{token}'";
            using var cmd = new NpgsqlCommand(sql, connection);

            long userID = 0;
            if ((cmd.ExecuteScalar() == DBNull.Value) || (cmd.ExecuteScalar() == null)) return -1;
            else userID = (long)cmd.ExecuteScalar();
            connection.Close();

            return userID;
        }

        public static void IncreaseELOafterTournament()
        {
            using var conSelect = new NpgsqlConnection(ConnectionString);
            conSelect.Open();

            string sqlSelect = $"SELECT SEB_users.userid, SUM(count) as count, elo  FROM SEB_History INNER JOIN SEB_Users ON SEB_History.userID = SEB_Users.userID WHERE tournamentID = (SELECT MAX(tournamentID) FROM SEB_History) GROUP BY SEB_users.userid";
            using var cmdSelect = new NpgsqlCommand(sqlSelect, conSelect);

            using var reader = cmdSelect.ExecuteReader();

            var ParticipantList = new List<PushUpStats>();

            while (reader.Read())
            {

                int tempUserID = reader.GetInt32(reader.GetOrdinal("userid"));
                int tempCount = reader.GetInt32(reader.GetOrdinal("count"));
                int tempELO = reader.GetInt32(reader.GetOrdinal("elo"));

                var tempObject = new PushUpStats(tempUserID, tempCount, tempELO);
                ParticipantList.Add(tempObject);
            }

            var WinnerList = new List<PushUpStats>();

            WinnerList.Add(ParticipantList[0]);

            bool firstIteration = true;

            foreach (var Participant in ParticipantList)
            {
                if (!firstIteration)
                {
                    if (Participant.Count == WinnerList[0].Count)
                    {
                        WinnerList.Add(Participant);
                    }

                    if (Participant.Count > WinnerList[0].Count)
                    {
                        WinnerList.Clear();
                        WinnerList.Add(Participant);
                    }
                }
                else
                {
                    firstIteration = false;
                }

            }



            foreach (var Participant in WinnerList)
            {
                ParticipantList.Remove(Participant);
            }

            if (WinnerList.Count() > 1)
            {
                foreach (var Participant in WinnerList)
                {
                    Participant.ELO += 1;
                }
            }
            else if (WinnerList.Count() == 1)
            {
                WinnerList[0].ELO += 2;
            }

            foreach (var Participant in ParticipantList)
            {
                Participant.ELO -= 1;
            }

            updateELO(WinnerList, ParticipantList);

        }

        public static void updateELO(List<PushUpStats> WinnerListUpdated, List<PushUpStats> LooserListUpdated)
        {
            using var conUpdate = new NpgsqlConnection(ConnectionString);
            conUpdate.Open();

            using var update = new NpgsqlCommand();
            update.Connection = conUpdate;

            foreach (var Participant in WinnerListUpdated)
            {
                update.CommandText = $"UPDATE SEB_Users SET elo = ({Participant.ELO}) WHERE userID = {Participant.userID}";
                update.ExecuteNonQuery();
            }

            foreach (var Participant in LooserListUpdated)
            {
                update.CommandText = $"UPDATE SEB_Users SET elo = ({Participant.ELO}) WHERE userID = {Participant.userID}";
                update.ExecuteNonQuery();
            }


            conUpdate.Close();
        }

        public static int PingDataBase()
        {
            try
            {
                using var con = new NpgsqlConnection(ConnectionString);
                con.Open();

                string sql = $"SELECT COUNT(*) FROM seb_users";
                using var cmd = new NpgsqlCommand(sql, con);

                cmd.ExecuteScalar();
                con.Close();
                return 0;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return -1;
            }
        }

    }

}
