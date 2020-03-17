using System;
using System.Threading;

namespace MUD_Server
{
    class Program
    {
        private static Thread consoleThread;
        private static Thread gameThread;
        private static bool isRunning;

        public const int TICKS_PER_SEC = 60;    // 48 = 0.02 deltatime
        public const int MS_PER_TICK = 1000 / TICKS_PER_SEC;
        private static float deltaTime;

        static void Main(string[] args)
        {
            Console.Write("Initializing Server...");
            InitConsoleThread();
            InitGameThread(); 
            HandleData.InitPacketList();
            Server.InitNetwork();
            Console.WriteLine($"[{DateTime.Now.ToString("H:mm:ss")}] Server is live...");
        }

        private static void InitGameThread()
        {
            gameThread = new Thread(new ThreadStart(GameThread));
            gameThread.Name = "GameThread";
            gameThread.Start();
        }

        private static void GameThread()
        {
            Console.WriteLine($"Server running at '{TICKS_PER_SEC}' ticks per second.");
            int worldInstances = 1;
            Game.InitWorldInstances(worldInstances);

            DateTime lastTick = DateTime.Now;
            DateTime nextTick = DateTime.Now;

            while (isRunning)
            {
                while (nextTick < DateTime.Now)
                {
                    deltaTime = (nextTick.Ticks - lastTick.Ticks) * 0.0000001f;
                    Game.Update(deltaTime);
                    lastTick = nextTick;
                    nextTick = nextTick.AddMilliseconds(MS_PER_TICK);

                    if (nextTick > DateTime.Now)
                    {
                        Thread.Sleep(nextTick - DateTime.Now);
                    }
                }
            }
        }

        private static void InitConsoleThread()
        {
            try
            {
                consoleThread = new Thread(ConsoleLoop);
                isRunning = true;
                consoleThread.Name = "ConsoleThread";
                consoleThread.Start();
                Console.WriteLine("\tDone!");
            }
            catch (Exception e)
            {
                Console.WriteLine("\tFailed");
                throw e;
            }
        }

        private static void ConsoleLoop()
        {
            string input;
            while (isRunning)
            {
                input = Console.ReadLine().ToLower();

                if (input.Equals("/shutdown"))
                {
                    Shutdown();
                }
                if (input.Equals("/testmovement"))
                {
                    Game.testMovement = !Game.testMovement;
                }
            }
        }

        private static void Shutdown()
        {
            Console.WriteLine("Server is Shutting down!");
            Server.ConnectSocket.Close();
            isRunning = false;
        }
    }
}
