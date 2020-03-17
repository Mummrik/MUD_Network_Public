using System;
using System.Collections.Generic;

namespace MUD_Server
{
    class Game
    {
        public static float DeltaTime { get; protected set; }
        public static World[] worldInstances;
        public static bool testMovement;

        public static void Update(float deltaTime)
        {
            DeltaTime = deltaTime;
            foreach (Client client in Server.GetClients())
            {
                client.player.Update(deltaTime);
            }
        }

        public static void InitWorldInstances(int amount)
        {
            Console.WriteLine($"Loading '{amount}' world instances...");
            worldInstances = new World[amount];
            for (int i = 0; i < amount; i++)
            {
                worldInstances[i] = new World(i);
            }
        }
    }
}
