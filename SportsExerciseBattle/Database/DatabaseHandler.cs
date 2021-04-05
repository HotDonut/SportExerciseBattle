using System;
using System.Collections.Generic;
using System.Text;
using Npgsql;
using Newtonsoft.Json;

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

            using var cmd = new NpgsqlCommand();
            cmd.Connection = connection;
            cmd.CommandText = $"INSERT INTO SEB_Users (username, password, elo, token) VALUES ('{username}', '{password}', '100', '{token}')";
            cmd.ExecuteNonQuery();

            return 0;
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

    }


}
