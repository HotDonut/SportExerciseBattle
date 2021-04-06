using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;

namespace SportsExerciseBattle.REST_Server
{
    public class WebHandler
    {
        private readonly ITCP _tcpHandler;

        private IReqContext _requestContext;

        public TcpClient Client;

        // used while normal operation
        public WebHandler(ITCP tcpHandler)
        {
            Console.WriteLine();
            _tcpHandler = tcpHandler;
            Client = _tcpHandler.AcceptTcpClient();
        }

        // for testing purposes
        public WebHandler(ITCP tcpHandler, IReqContext requestContext)
        {
            _tcpHandler = tcpHandler;
            _requestContext = requestContext;
        }    

        public bool WorkHttpRequest(bool activeTournament)
        {
            _requestContext = new ReqContext(Client);
            return _requestContext.RequestCoordinator(activeTournament);
        }

        public void SendHttpContent()
        {
            var response = "HTTP/1.1" + " " + _requestContext.StatusCode + "\r\n"
                       + "Server: " + "SEB-Server" + "\r\n";

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
