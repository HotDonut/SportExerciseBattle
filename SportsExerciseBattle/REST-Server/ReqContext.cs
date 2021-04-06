using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Net;
using System.Threading;
using Newtonsoft.Json;
using SportsExerciseBattle.Database;
using SportsExerciseBattle.REST_Server;
using System.Net.Sockets;
using System.IO;

namespace SportsExerciseBattle.REST_Server
{
    public class ReqContext : IReqContext
    {
        public Dictionary<string, string> HeaderInfo;

        public string StatusCode { get; set; }
        public string Payload { get; set; }
        public string ContentType { get; set; }



        // runs in normal operation
        public ReqContext(TcpClient Client)
        {
            try
            {
                // if receivedData does not resemble a HttpRequest an exception is thrown.
                var reader = new StreamReader(Client.GetStream());
                string receivedData = "";
                string tmpMsg;
                do
                {
                    tmpMsg = reader.ReadLine();
                    receivedData += tmpMsg + "\r\n";

                } while (tmpMsg != string.Empty);

                HeaderInfo = new Dictionary<string, string>();


                //string[] bodyOnSecond = receivedData.Split("\r\n\r\n");
                string[] dataSnippets = receivedData.Split("\r\n");

                string[] headerDataFilter = dataSnippets[0].Split(" ");

                // Splitting header data and saving it
                HeaderInfo.Add("RequestMethod", headerDataFilter[0]);
                HeaderInfo.Add("RequestPath", headerDataFilter[1]);
                HeaderInfo.Add("HttpVersion", headerDataFilter[2]);

                for (int i = 1; i < dataSnippets.Length; i++)
                {
                    string[] tmp = dataSnippets[i].Split(": ");
                    if (tmp.Length == 2)
                    {
                        HeaderInfo.Add(tmp[0], tmp[1]);
                    }
                }


                if (HeaderInfo.ContainsKey("Content-Length"))
                {
                    int ContentLength = Int32.Parse(HeaderInfo["Content-Length"]);
                    if (ContentLength != 0)
                    {
                        string msg = "";
                        int temp;

                        for (int i = 0; i < ContentLength; i++)
                        {
                            temp = reader.Read();
                            if (temp == -1)
                                break;
                            msg += (char)temp;
                        }
                        HeaderInfo.Add("Body", msg);
                    }
                }

                Console.WriteLine("\n\n----------RECEIVED HTTP-REQUEST----------");
                              
                foreach (KeyValuePair<string, string> entry in HeaderInfo)
                {
                    Console.WriteLine(entry.Key + ": " + entry.Value);
                }

                Console.WriteLine("--------RECEIVED HTTP-REQUEST END--------\n");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                BadRequest();
            }
        }

        // Checks which function is appropriate for specific HttpRequest
        public bool RequestCoordinator(bool activeTournament)
        {
            dynamic data = HeaderInfo;

            if (HeaderInfo.ContainsKey("RequestPath") == false)
            {
                HeaderInfo.Add("RequestPath", "ERROR");
            }

            if (HeaderInfo.ContainsKey("RequestMethod") == false)
            {
                HeaderInfo.Add("RequestMethod", "ERROR");
            }

            if (HeaderInfo.ContainsKey("Body"))
            {
                data = JsonConvert.DeserializeObject(HeaderInfo["Body"]);
            }

            Console.WriteLine(":):):):):):):):) Reading Request");

            if ((HeaderInfo["RequestPath"] == "/users") &&
                (HeaderInfo["RequestMethod"] == "POST"))
            {
                try
                {
                    string usernameExists = data["Username"];
                    string passwordExists = data["Password"];

                    if ((usernameExists == null) || (passwordExists == null))
                    {
                        BadRequest();
                    }

                    if (DatabaseHandler.RegRequest(usernameExists, passwordExists) == -1)
                    {
                        BadRequest();
                    }
                    else
                    {
                        StatusCode = "201 Created";
                        ContentType = "text/plain";
                        var reply = "Account Created";
                        Payload = reply;

                        Console.WriteLine(">>Responding with 201 Created");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            else if ((HeaderInfo["RequestPath"] == "/sessions") &&
                (HeaderInfo["RequestMethod"] == "POST"))
            {
                try
                {
                    string usernameExists = data["Username"];
                    string passwordExists = data["Password"];

                    if ((usernameExists == null) || (passwordExists == null))
                    {
                        BadRequest();
                    }

                    string loginAnswer = DatabaseHandler.LoginRequest(usernameExists, passwordExists);

                    if (loginAnswer == "-1")
                    {
                        BadRequest();
                    }
                    else
                    {
                        StatusCode = "200 OK";
                        ContentType = "text/plain";
                        var reply = loginAnswer;
                        Payload = reply;

                        Console.WriteLine(">>Responding with 200 OK");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            else if ((HeaderInfo["RequestPath"].StartsWith("/users/")) &&
                    (HeaderInfo["RequestMethod"] == "GET"))
            {
                Boolean tokenExists = true;

                if (HeaderInfo.ContainsKey("Authorization") != true)
                {
                    tokenExists = false;
                }
                else if (DatabaseHandler.CountOccurrence("seb_users", "token", HeaderInfo["Authorization"]) != 1)
                {
                    tokenExists = false;
                }

                if (!tokenExists)
                {
                    BadRequest();
                }
                else
                {
                    var reply = DatabaseHandler.GetUserdata(HeaderInfo["Authorization"], HeaderInfo["RequestPath"]);

                    if (reply == "-1")
                    {
                        BadRequest();
                    }
                    else
                    {
                        StatusCode = "200 OK";
                        ContentType = "application/json";
                        Payload = reply;
                        Console.WriteLine(">>Responding with 200 OK");
                    }
                }
            }
            else if ((HeaderInfo["RequestPath"].StartsWith("/users/")) &&
                     (HeaderInfo["RequestMethod"] == "PUT"))
            {
                Boolean badRequest = false;

                if (HeaderInfo.ContainsKey("Authorization") != true)
                {
                    badRequest = true;
                }
                else if (DatabaseHandler.CountOccurrence("seb_users", "token", HeaderInfo["Authorization"]) != 1)
                {
                    badRequest = true;
                }

                var name = (string)data["Name"];
                var bio = (string)data["Bio"];
                var image = (string)data["Image"];
                if ((bio == null) ||
                    (name == null) ||
                    (image == null))
                {
                    badRequest = true;
                }

                if (badRequest)
                {
                    BadRequest();
                }
                else
                {


                    if (DatabaseHandler.PutUserdata(HeaderInfo["Authorization"], HeaderInfo["RequestPath"], data) == -1)
                    {
                        BadRequest();
                    }
                    else
                    {
                        StatusCode = "200 OK";
                        ContentType = "text/plain";
                        var reply = "OK";
                        Payload = reply;
                        Console.WriteLine(">>Responding with 200 OK");
                    }
                }
            }
            else if ((HeaderInfo["RequestPath"] == "/history") &&
                     (HeaderInfo["RequestMethod"] == "POST"))
            {
                Boolean badRequest = false;

                if (HeaderInfo.ContainsKey("Authorization") != true)
                {
                    badRequest = true;
                }
                else if (DatabaseHandler.CountOccurrence("seb_users", "token", HeaderInfo["Authorization"]) != 1)
                {
                    badRequest = true;
                }

                if (badRequest)
                {
                    BadRequest();
                }
                else
                {


                    if (DatabaseHandler.PushUpEntry(HeaderInfo["Authorization"], data, activeTournament) == -1)
                    {
                        BadRequest();
                    }
                    else
                    {
                        activeTournament = true;
                        float worldRecordPace = 140 / 60;
                        int pushups = Convert.ToInt32(data["Count"]);
                        int duration = Convert.ToInt32(data["DurationInSeconds"]);


                        float pace = pushups/duration;

                        var reply = "OK";

                        if(pace > worldRecordPace)
                        {
                            reply = "NEW WORLD RECORD";
                        }

                        StatusCode = "200 OK";
                        ContentType = "text/plain";                    
                        Payload = reply;
                        Console.WriteLine(">>Responding with 200 OK  ---> TOURNAMENT STARTED <---");
                    }
                }


            }

            else if ((HeaderInfo["RequestPath"] == "/history") &&
                     (HeaderInfo["RequestMethod"] == "GET"))
            {
                Boolean badRequest = false;

                if (HeaderInfo.ContainsKey("Authorization") != true)
                {
                    badRequest = true;
                }
                else if (DatabaseHandler.CountOccurrence("seb_users", "token", HeaderInfo["Authorization"]) != 1)
                {
                    badRequest = true;
                }

                if (badRequest)
                {
                    BadRequest();
                }
                else
                {

                    var reply = DatabaseHandler.DisplayAllEntries(HeaderInfo["Authorization"], data);

                    if (reply == "-1")
                    {
                        BadRequest();
                    }
                    else
                    {
                        StatusCode = "200 OK";
                        ContentType = "text/plain";
                        Payload = reply;
                        Console.WriteLine(">>Responding with 200 OK");
                    }
                }


            }

            else if ((HeaderInfo["RequestPath"] == "/stats") &&
                     (HeaderInfo["RequestMethod"] == "GET"))
            {
                Boolean badRequest = false;

                if (HeaderInfo.ContainsKey("Authorization") != true)
                {
                    badRequest = true;
                }
                else if (DatabaseHandler.CountOccurrence("seb_users", "token", HeaderInfo["Authorization"]) != 1)
                {
                    badRequest = true;
                }

                if (badRequest)
                {
                    BadRequest();
                }
                else
                {

                    var reply = DatabaseHandler.GetStats(HeaderInfo["Authorization"], data);

                    if (reply == "-1")
                    {
                        BadRequest();
                    }
                    else
                    {
                        StatusCode = "200 OK";
                        ContentType = "text/plain";
                        Payload = reply;
                        Console.WriteLine(">>Responding with 200 OK");
                    }
                }
            }

            else if ((HeaderInfo["RequestPath"] == "/score") &&
                 (HeaderInfo["RequestMethod"] == "GET"))
            {
                Boolean badRequest = false;

                if (HeaderInfo.ContainsKey("Authorization") != true)
                {
                    badRequest = true;
                }
                else if (DatabaseHandler.CountOccurrence("seb_users", "token", HeaderInfo["Authorization"]) != 1)
                {
                    badRequest = true;
                }

                if (badRequest)
                {
                    BadRequest();
                }
                else
                {

                    var reply = DatabaseHandler.GetScoreboard(HeaderInfo["Authorization"], data);

                    if (reply == "-1")
                    {
                        BadRequest();
                    }
                    else
                    {
                        StatusCode = "200 OK";
                        ContentType = "text/plain";
                        Payload = reply;
                        Console.WriteLine(">>Responding with 200 OK");
                    }
                }
            }

            else if ((HeaderInfo["RequestPath"] == "/tournament") &&
             (HeaderInfo["RequestMethod"] == "GET"))
            {
                Boolean badRequest = false;

                if (HeaderInfo.ContainsKey("Authorization") != true)
                {
                    badRequest = true;
                }
                else if (DatabaseHandler.CountOccurrence("seb_users", "token", HeaderInfo["Authorization"]) != 1)
                {
                    badRequest = true;
                }

                if (badRequest)
                {
                    BadRequest();
                }
                else
                {
                    
                    var reply = DatabaseHandler.GetTournamentInfo(HeaderInfo["Authorization"], data, activeTournament);
                        

                    if (reply == "-1")
                    {
                        BadRequest();
                    }
                    else
                    {
                        StatusCode = "200 OK";
                        ContentType = "text/plain";
                        Payload = reply;
                        Console.WriteLine(">>Responding with 200 OK");
                    }
                }

            }
            return activeTournament;

        }
        private void BadRequest()
        {
            StatusCode = "400 Bad Request";
            ContentType = "text/plain";
            Payload = "Bad Request";
            Console.WriteLine(">>Responding with 400 Bad Request");
        }

        private void ServerError()
        {
            StatusCode = "500 Internal Server Error";
            ContentType = "text/plain";
            Payload = "500 Internal Server Error";
            Console.WriteLine(">>Responding with 500 Internal Server Error");
        }
    }
}
