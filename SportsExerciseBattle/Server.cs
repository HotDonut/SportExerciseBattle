using System;
using System.Collections.Generic;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SportsExerciseBattle.REST_Server;
using SportsExerciseBattle.Database;

namespace SportsExerciseBattle
{
    public class Server
    {
        private static readonly SemaphoreSlim ConcurrentConnections = new SemaphoreSlim(2);

        public static bool activeTournament = false;
        static Task Main(string[] args)
        {

            Console.WriteLine("__________STARTING REST-SERVER__________");

            TCP tcpHandler = null;
            
            var tasks = new List<Task>();

            tasks.Add(Task.Run(() => TournamentMaster(10000)));

            try
            {
                tcpHandler = new TCP();

                while (true)
                {
                    ConcurrentConnections.Wait();
                    tasks.Add(Task.Run(() => ClientReception(tcpHandler)));
                    
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            finally
            {
                tcpHandler?.Stop();
                Task.WaitAll(tasks.ToArray());
            }
        }

        private static void ClientReception(TCP tcpHandler)
        {
            WebHandler webHandler = new WebHandler(tcpHandler);

            activeTournament = webHandler.WorkHttpRequest(activeTournament);

            Console.WriteLine("\n\n--------------SENT RESPONSE--------------");
            webHandler.SendHttpContent();
            Console.WriteLine("------------SENT RESPONSE END------------\n");

            tcpHandler.CloseClient(webHandler.Client);

            ConcurrentConnections.Release();
            Console.WriteLine(">>Client finished\n\n\n\n\n");
        }

        public static void TournamentMaster(int time)
        {
            while (true)
            {
                Thread.Sleep(100);
                if (Server.activeTournament == true)
                {
                    Console.WriteLine("--------------Tournament Begins--------------");
                    Thread.Sleep(time);
                    Console.WriteLine("--------------Tournament Ends--------------");
                    DatabaseHandler.IncreaseELOafterTournament();
                    Server.activeTournament = false;
                }
            }
        }
    }
}