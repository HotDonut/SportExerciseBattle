using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;

namespace SportsExerciseBattle.REST_Server
{
    public class WebHandler : IWebHandler
    {
        private readonly ITcpHandler _tcpHandler;

        private IRequestContext _requestContext;

        public Battle BattleCenter = new Battle();

        public TcpClient Client;

        // used while normal operation
        public WebHandler(ITcpHandler tcpHandler, Battle _battleCenter)
        {
            Console.WriteLine();
            BattleCenter = _battleCenter;
            _tcpHandler = tcpHandler;
            Client = _tcpHandler.AcceptTcpClient();
        }

        // for testing purposes
        public WebHandler(ITcpHandler tcpHandler, IRequestContext requestContext)
        {
            _tcpHandler = tcpHandler;
            _requestContext = requestContext;
        }
        public WebHandler(ITcpHandler tcpHandler)
        {
            _tcpHandler = tcpHandler;
        }

        // reads message sent by client
        // Streamreader did not work, which made testing a little harder
        // Instead of getting .DataAvailable() from the StreamReader-Object,
        // I had to check, if the TcpClient has available data.
        public string GetHttpContent()
        {
            var stream = _tcpHandler.GetStream(Client);
            var receivedData = "";

            while (_tcpHandler.DataAvailable(Client) != 0)
            {
                Byte[] bytes = new Byte[4096];
                int i = stream.Read(bytes, 0, bytes.Length);
                receivedData += System.Text.Encoding.ASCII.GetString(bytes, 0, i);
            }

            return receivedData;
        }

        public void WorkHttpRequest(string content, List<String> messageData)
        {
            _requestContext = new RequestContext(content, messageData, BattleCenter);
            _requestContext.RequestCoordinator();
        }

        public void SendHttpContent()
        {
            var response = "HTTP/1.1" + " " + _requestContext.StatusCode + "\r\n"
                       + "Server: " + "MTCG-Server" + "\r\n";

            if (_requestContext.Payload != "")
            {
                response += "Content-Type: " + _requestContext.ContentType + "\r\n";


                var mlength = _requestContext.Payload.Length;
                response += "Content-Length: " + mlength + "\r\n\r\n" + _requestContext.Payload;

            }
            using StreamWriter writer = new StreamWriter(_tcpHandler.GetStream(Client)) { AutoFlush = true };
            writer.WriteLine(response);
            Console.WriteLine(response);
        }
    }
}
