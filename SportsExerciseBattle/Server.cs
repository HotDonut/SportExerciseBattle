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

            tasks.Add(Task.Run(() => TournamentMaster()));

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

        // ClientReception is like a lobby in a hotel. Here every client checks in
        // and every client checks out
        private static void ClientReception(TCP tcpHandler)
        {
            WebHandler webHandler = new WebHandler(tcpHandler);

            //Console.WriteLine("\n\n----------RECEIVED HTTP-REQUEST----------");
            //Console.WriteLine(content);
            //Console.WriteLine("--------RECEIVED HTTP-REQUEST END--------\n");

            activeTournament = webHandler.WorkHttpRequest(activeTournament);

            Console.WriteLine("\n\n--------------SENT RESPONSE--------------");
            webHandler.SendHttpContent();
            Console.WriteLine("------------SENT RESPONSE END------------\n");

            tcpHandler.CloseClient(webHandler.Client);

            ConcurrentConnections.Release();
            Console.WriteLine(">>Client finished\n\n\n\n\n");
        }

        private static void TournamentMaster()
        {
            while (true)
            {
                Thread.Sleep(100);
                if (activeTournament == true)
                {
                    Console.WriteLine("--------------Tournament Begins--------------");
                    Thread.Sleep(10000);
                    Console.WriteLine("--------------Tournament Ends--------------");
                    DatabaseHandler.IncreaseELOafterTournament();
                    activeTournament = false;
                }
            }
        }
    }
}