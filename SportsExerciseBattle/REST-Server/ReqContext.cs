﻿using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Net;
using System.Threading;
using Newtonsoft.Json;

namespace SportsExerciseBattle.REST_Server
{
    public class ReqContext : IReqContext
    {
        public Dictionary<string, string> HeaderInfo;

        public string StatusCode { get; set; }
        public string Payload { get; set; }
        public string ContentType { get; set; }

        private List<string> messagesData = new List<string>();

        // runs in normal operation
        public ReqContext(string receivedData, List<string> messagesData)
        {
            // if receivedData does not resemble a HttpRequest an exception is thrown.
            try
            {
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
        {/*
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
            }*/
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
