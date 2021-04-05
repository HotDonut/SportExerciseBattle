using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.IO;

namespace SportsExerciseBattle.REST_Server
{
    interface ITCP   {
        Stream GetStream(TcpClient client);
        TcpClient AcceptTcpClient();
        int DataAvailable(TcpClient client);
    }
}
