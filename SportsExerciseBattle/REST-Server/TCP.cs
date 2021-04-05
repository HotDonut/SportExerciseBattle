using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace SportsExerciseBattle.REST_Server
{
    class TCP : ITCP
    {
        private TcpListener _server;
        //public TcpClient _client;

        public TCP()
        {
            // Connect server
            _server = new TcpListener(IPAddress.Loopback, 10001);
            _server.Start(5);
        }

        public TcpClient AcceptTcpClient()
        {
            Console.WriteLine(">>>>>>>>>>Waiting for a client<<<<<<<<<<");
            Console.WriteLine("...");

            var client = _server.AcceptTcpClient();

            Console.WriteLine(">>Servicing client");
            Console.WriteLine("...");

            return client;
        }

        public Stream GetStream(TcpClient client)
        {
            return client.GetStream();
        }

        public void CloseClient(TcpClient client) => client.Close();

        public void Stop() => _server.Stop();

        public int DataAvailable(TcpClient client)
        {
            return client.Available;
        }
    }
}
