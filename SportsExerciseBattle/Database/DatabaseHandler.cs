using System;
using System.Collections.Generic;
using System.Text;
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

            if(userIDtoken == -1)
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
    }



}
