using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace MUD_Server
{
    class HandleData
    {
        public static Dictionary<int, Action<Guid, DataBuffer>> packetList;
        public static void InitPacketList()
        {
            packetList = new Dictionary<int, Action<Guid, DataBuffer>>();
            packetList.Add((int)ClientPacket.PingServer, PingServer);
            packetList.Add((int)ClientPacket.Disconnect, ClientDisconnect);
            packetList.Add((int)ClientPacket.RequestMyPlayer, RequestMyPlayer);
            packetList.Add((int)ClientPacket.RequestPlayer, RequestPlayer);
            packetList.Add((int)ClientPacket.RequestNeighbourPlayer, RequestNeighbourPlayer);
            packetList.Add((int)ClientPacket.KeyInput, KeyInput);
        }

        public static void Read(Guid fromClient, byte[] data)
        {
            using (DataBuffer buffer = new DataBuffer(data))
            {
                // Udp send Guid, so the server need to know who sent the packet
                if (fromClient == Guid.Empty)
                {
                    fromClient = buffer.ReadGuid();
                }

                int packetLength = buffer.ReadInt();
                int packetId = buffer.ReadInt();

                if (packetList.TryGetValue(packetId, out Action<Guid, DataBuffer> packet))
                {
                    if (fromClient != Guid.Empty)
                    {
                        packet.Invoke(fromClient, buffer);
                    }
                }
            }
        }

        private static void PingServer(Guid fromClient, DataBuffer data)
        {
            Server.PingToClient(in fromClient);
        }

        private static void ClientDisconnect(Guid fromClient, DataBuffer data)
        {
            Server.clients[fromClient].Disconnet();
        }

        private static void RequestMyPlayer(Guid fromClient, DataBuffer data)
        {
            Player player = Server.clients[fromClient].player;

            string ipAddress = data.ReadString();
            if (Server.clients[fromClient].endPoint == null)
            {
                string[] ep = ipAddress.Split(':');
                Server.clients[fromClient].endPoint = new IPEndPoint(IPAddress.Parse(ep[0]), int.Parse(ep[1]));
                Server.clients[fromClient].ConnectUdp();
            }

            int worldId = player.worldId;
            if (worldId < 0) { worldId = 0; }


            foreach (Client client in Server.GetClients())
                Server.SendPlayerToClient(in client.id, in player);

            foreach (Client client in Server.GetClients())
                if (client.id != fromClient)
                    Server.SendPlayerToClient(in fromClient, client.player);
        }

        private static void RequestPlayer(Guid fromClient, DataBuffer data)
        {
            Player player = Server.clients[data.ReadGuid()].player;
            Server.SendPlayerToClient(in fromClient, in player);
        }

        private static void RequestNeighbourPlayer(Guid fromClient, DataBuffer data)
        {
            foreach (Client client in Server.GetClients())
            {
                if (client.id != fromClient)
                    Server.SendPlayerToClient(in fromClient, client.player);
            }
        }

        private static void KeyInput(Guid fromClient, DataBuffer data)
        {
            int keyInput = data.ReadInt();
            Server.clients[fromClient].player.SetKey(keyInput);
        }
    }
}
