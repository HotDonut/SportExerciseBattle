using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Net;
using System.Threading;
using Newtonsoft.Json;
using SWE1_MTCG.DBHandler;

namespace SWE1_MTCG.REST
{
    public class RequestContext : IRequestContext
    {
        public Dictionary<string, string> HeaderInfo;

        public string StatusCode { get; set; }
        public string Payload { get; set; }
        public string ContentType { get; set; }

        private List<string> messagesData = new List<string>();

        public Battle BattleCenter = new Battle();

        // for testing purposes
        public RequestContext()
        {
        }

        // runs in normal operation
        public RequestContext(string receivedData, List<string> messagesData, Battle battleCenter)
        {
            // if receivedData does not resemble a HttpRequest an exception is thrown.
            try
            {
                BattleCenter = battleCenter;
                HeaderInfo = new Dictionary<string, string>();

                this.messagesData = messagesData;
                string[] bodyOnSecond = receivedData.Split("\r\n\r\n");
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

                HeaderInfo.Add("Body", bodyOnSecond[1]);

                foreach (KeyValuePair<string, string> entry in HeaderInfo)
                {
                    Console.WriteLine(entry.Key + ": " + entry.Value);
                }
            }

            catch (Exception e)
            {
                Console.WriteLine(e);
                BadRequest();
            }
        }

        // Checks which function is appropriate for specific HttpRequest
        public void RequestCoordinator()
        {
            if (DatabaseHandler.PingDataBase() == -1)
            {
                ServerError();
                return;
            }

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

            else if ((HeaderInfo["RequestPath"] == "/packages") &&
                     (HeaderInfo["RequestMethod"] == "POST"))
            {
                int check = 0;

                if (HeaderInfo.ContainsKey("Authorization") != true)
                {
                    check = 1;
                }

                int count = 0;

                foreach (dynamic item in data)
                {
                    count++;
                    var id = (string)item["Id"];
                    var name = (string)item["Name"];
                    var damage = (string)item["Damage"];
                    Console.WriteLine($"id: {id}  name: {name}  damage: {damage}");
                    if ((id == null) ||
                        (name == null) ||
                        (damage == null))
                    {
                        check = 1;
                    }
                }

                if (count != 5)
                {
                    check = 1;
                }

                if (check == 1)
                {
                    BadRequest();
                }
                else
                {
                    if (DatabaseHandler.AddPackage(data, HeaderInfo["Authorization"]) == -1)
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

            else if ((HeaderInfo["RequestPath"] == "/transactions/packages") &&
                     (HeaderInfo["RequestMethod"] == "POST"))
            {
                int check = 0;

                if (HeaderInfo.ContainsKey("Authorization") != true)
                {
                    check = 1;
                }
                else if (DatabaseHandler.CountOccurrence("Credentials", "token", HeaderInfo["Authorization"]) != 1)
                {
                    check = 1;
                }

                if (check == 1)
                {
                    BadRequest();
                }
                else
                {
                    if (DatabaseHandler.Transaction(HeaderInfo["Authorization"]) == -1)
                    {
                        BadRequest();
                    }
                    else
                    {
                        Console.WriteLine("why");
                        StatusCode = "200 OK";
                        ContentType = "text/plain";
                        var reply = "OK";
                        Payload = reply;
                        Console.WriteLine(">>Responding with 200 OK");
                    }
                }
            }

            else if ((HeaderInfo["RequestPath"] == "/cards") &&
                     (HeaderInfo["RequestMethod"] == "GET"))
            {
                int check = 0;

                if (HeaderInfo.ContainsKey("Authorization") != true)
                {
                    check = 1;
                }
                else if (DatabaseHandler.CountOccurrence("Credentials", "token", HeaderInfo["Authorization"]) != 1)
                {
                    check = 1;
                }

                if (check == 1)
                {
                    BadRequest();
                }
                else
                {
                    StatusCode = "200 OK";
                    ContentType = "application/json";
                    var reply = DatabaseHandler.GetCards(HeaderInfo["Authorization"]);
                    Payload = reply;
                    Console.WriteLine(">>Responding with 200 OK");

                }
            }

            else if ((HeaderInfo["RequestPath"] == "/deck") &&
                     (HeaderInfo["RequestMethod"] == "GET"))
            {
                int check = 0;

                if (HeaderInfo.ContainsKey("Authorization") != true)
                {
                    check = 1;
                }
                else if (DatabaseHandler.CountOccurrence("Credentials", "token", HeaderInfo["Authorization"]) != 1)
                {
                    check = 1;
                }

                if (check == 1)
                {
                    BadRequest();
                }
                else
                {
                    StatusCode = "200 OK";
                    ContentType = "application/json";
                    var reply = DatabaseHandler.GetDeck(HeaderInfo["Authorization"]);
                    Payload = reply;
                    Console.WriteLine(">>Responding with 200 OK");

                }
            }

            else if ((HeaderInfo["RequestPath"] == "/deck?format=plain") &&
                     (HeaderInfo["RequestMethod"] == "GET"))
            {
                int check = 0;

                if (HeaderInfo.ContainsKey("Authorization") != true)
                {
                    check = 1;
                }
                else if (DatabaseHandler.CountOccurrence("Credentials", "token", HeaderInfo["Authorization"]) != 1)
                {
                    check = 1;
                }

                if (check == 1)
                {
                    BadRequest();
                }
                else
                {
                    StatusCode = "200 OK";
                    ContentType = "application/json";
                    var reply = DatabaseHandler.GetDeckPlain(HeaderInfo["Authorization"]);
                    Payload = reply;
                    Console.WriteLine(">>Responding with 200 OK");

                }
            }

            else if ((HeaderInfo["RequestPath"] == "/deck") &&
                     (HeaderInfo["RequestMethod"] == "PUT"))
            {
                int check = 0;

                if (HeaderInfo.ContainsKey("Authorization") != true)
                {
                    check = 1;
                }

                int count = 0;

                foreach (dynamic item in data)
                {
                    count++;
                    if (item == null)
                    {
                        check = 1;
                    }
                }

                if (count != 4)
                {
                    check = 1;
                }

                if (check == 1)
                {
                    BadRequest();
                }
                else
                {
                    if (DatabaseHandler.ConfigureDeck(data, HeaderInfo["Authorization"]) == "0")
                    {
                        StatusCode = "200 OK";
                        ContentType = "application/json";
                        var reply = "OK";
                        Payload = reply;
                        Console.WriteLine(">>Responding with 200 OK");
                    }
                    else
                    {
                        StatusCode = "400 Bad Request";
                        ContentType = "application/json";
                        var reply = DatabaseHandler.GetDeck(HeaderInfo["Authorization"]);
                        Payload = reply;
                        Console.WriteLine(">>Responding with 400 Bad Request");
                    }

                }
            }

            else if ((HeaderInfo["RequestPath"].StartsWith("/users/")) &&
                     (HeaderInfo["RequestMethod"] == "GET"))
            {
                int check = 0;

                if (HeaderInfo.ContainsKey("Authorization") != true)
                {
                    check = 1;
                }
                else if (DatabaseHandler.CountOccurrence("Credentials", "token", HeaderInfo["Authorization"]) != 1)
                {
                    check = 1;
                }

                if (check == 1)
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
                int check = 0;

                if (HeaderInfo.ContainsKey("Authorization") != true)
                {
                    check = 1;
                }
                else if (DatabaseHandler.CountOccurrence("Credentials", "token", HeaderInfo["Authorization"]) != 1)
                {
                    check = 1;
                }

                var name = (string)data["Name"];
                var bio = (string)data["Bio"];
                var image = (string)data["Image"];
                if ((bio == null) ||
                    (name == null) ||
                    (image == null))
                {
                    check = 1;
                }

                if (check == 1)
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

            else if ((HeaderInfo["RequestPath"].StartsWith("/stats")) &&
                     (HeaderInfo["RequestMethod"] == "GET"))
            {
                int check = 0;

                if (HeaderInfo.ContainsKey("Authorization") != true)
                {
                    check = 1;
                }
                else if (DatabaseHandler.CountOccurrence("Credentials", "token", HeaderInfo["Authorization"]) != 1)
                {
                    check = 1;
                }

                if (check == 1)
                {
                    BadRequest();
                }
                else
                {
                    var reply = DatabaseHandler.GetStats(HeaderInfo["Authorization"]);

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

            else if ((HeaderInfo["RequestPath"].StartsWith("/score")) &&
                     (HeaderInfo["RequestMethod"] == "GET"))
            {
                int check = 0;

                if (HeaderInfo.ContainsKey("Authorization") != true)
                {
                    check = 1;
                }
                else if (DatabaseHandler.CountOccurrence("Credentials", "token", HeaderInfo["Authorization"]) != 1)
                {
                    check = 1;
                }

                if (check == 1)
                {
                    BadRequest();
                }
                else
                {
                    var reply = DatabaseHandler.GetScoreboard();

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

            else if ((HeaderInfo["RequestPath"].StartsWith("/tradings")) &&
                     (HeaderInfo["RequestMethod"] == "GET"))
            {
                int check = 0;

                if (HeaderInfo.ContainsKey("Authorization") != true)
                {
                    check = 1;
                }
                else if (DatabaseHandler.CountOccurrence("Credentials", "token", HeaderInfo["Authorization"]) != 1)
                {
                    check = 1;
                }

                if (check == 1)
                {
                    BadRequest();
                }
                else
                {
                    var reply = DatabaseHandler.GetDeals();

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

            else if ((HeaderInfo["RequestPath"].StartsWith("/score")) &&
                     (HeaderInfo["RequestMethod"] == "GET"))
            {
                int check = 0;

                if (HeaderInfo.ContainsKey("Authorization") != true)
                {
                    check = 1;
                }
                else if (DatabaseHandler.CountOccurrence("Credentials", "token", HeaderInfo["Authorization"]) != 1)
                {
                    check = 1;
                }

                if (check == 1)
                {
                    BadRequest();
                }
                else
                {
                    var reply = DatabaseHandler.GetScoreboard();

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

            else if ((HeaderInfo["RequestPath"] == "/tradings") &&
                     (HeaderInfo["RequestMethod"] == "POST"))
            {
                int check = 0;

                if (HeaderInfo.ContainsKey("Authorization") != true)
                {
                    check = 1;
                }
                else if (DatabaseHandler.CountOccurrence("Credentials", "token", HeaderInfo["Authorization"]) != 1)
                {
                    check = 1;
                }

                var id = (string)data["Id"];
                var cardToTrade = (string)data["CardToTrade"];
                var type = (string)data["Type"];
                var minDam = (int)data["MinimumDamage"];
                if ((id == null) ||
                    (cardToTrade == null) ||
                    (type == null) ||
                    (minDam == null))
                {
                    check = 1;
                }
                Console.WriteLine("------------------------------------");
                if (check == 1)
                {
                    Console.WriteLine("1");
                    BadRequest();
                }
                else
                {
                    var reply = DatabaseHandler.AddTrade(data, HeaderInfo["Authorization"]);

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

            else if ((HeaderInfo["RequestPath"].StartsWith("/tradings/")) &&
                     (HeaderInfo["RequestMethod"] == "DELETE"))
            {
                int check = 0;

                if (HeaderInfo.ContainsKey("Authorization") != true)
                {
                    check = 1;
                }
                else if (DatabaseHandler.CountOccurrence("Credentials", "token", HeaderInfo["Authorization"]) != 1)
                {
                    check = 1;
                }

                if (check == 1)
                {
                    BadRequest();
                }
                else
                {
                    var reply = DatabaseHandler.DeleteTrade(HeaderInfo["Authorization"], HeaderInfo["RequestPath"]);

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

            else if ((HeaderInfo["RequestPath"].StartsWith("/tradings/")) &&
                (HeaderInfo["RequestMethod"] == "POST"))
            {
                int check = 0;

                if (HeaderInfo.ContainsKey("Authorization") != true)
                {
                    check = 1;
                }
                else if (DatabaseHandler.CountOccurrence("Credentials", "token", HeaderInfo["Authorization"]) != 1)
                {
                    check = 1;
                }

                var onlyEntry = (string)data;
                if (onlyEntry == null)
                {
                    check = 1;
                }

                if (check == 1)
                {
                    BadRequest();
                }
                else
                {
                    var reply = DatabaseHandler.Trade(HeaderInfo["Authorization"], HeaderInfo["RequestPath"], data);

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

            else if ((HeaderInfo["RequestPath"] == "/battles") &&
                     (HeaderInfo["RequestMethod"] == "POST"))
            {
                int check = 0;

                if (HeaderInfo.ContainsKey("Authorization") != true)
                {
                    check = 1;
                }
                else if (DatabaseHandler.CountOccurrence("Credentials", "token", HeaderInfo["Authorization"]) != 1)
                {
                    check = 1;
                }

                if (check == 1)
                {
                    BadRequest();
                }
                else
                {
                    var battleRegistration = BattleCenter.RegForBattle(HeaderInfo["Authorization"]);

                    if (battleRegistration == -100)
                    {
                        StatusCode = "100 Continue";
                        ContentType = "text/plain";
                        Payload = "BATTLE QUEUE IS FULL";
                    }
                    else if (battleRegistration == -101)
                    {
                        StatusCode = "100 Continue";
                        ContentType = "text/plain";
                        Payload = "CARD COUNT IN DECK IS NOT 4. RECONFIGURE!";
                    }
                    else
                    {
                        var battleResult = BattleCenter.CheckOnBattle();
                        StatusCode = "100 Continue";
                        ContentType = "text/plain";
                        Payload = battleResult;
                    }
                }
            }

            else
            {
                BadRequest();
            }

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
