using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace MUD_Server
{
    class Server
    {
        public static Socket ConnectSocket { get { return connectSocket; } }
        private static Socket connectSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        public static Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        public static byte[] buffer;

        public const int maxBufferSize = 4096;
        public const int port = 7171;
        public static Dictionary<Guid, Client> clients = new Dictionary<Guid, Client>();

        public static void InitNetwork()
        {
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, port);
            // UDP buffer and socket
            buffer = new byte[maxBufferSize];
            socket.Bind(localEndPoint);

            // TCP Connection socket, socket responsible for incomming connections.
            connectSocket.Bind(localEndPoint);
            connectSocket.Listen(10);
            connectSocket.BeginAccept(ClientConnectCB, null);
        }

        private static void ClientConnectCB(IAsyncResult result)
        {
            Socket tempSocket = connectSocket.EndAccept(result);
            Guid cid = Guid.NewGuid();

            if (!clients.ContainsKey(cid))
            {
                clients.Add(cid, new Client(tempSocket, cid));
                if (clients.ContainsKey(cid))
                    SendGuid(in cid);
            }

            connectSocket.BeginAccept(ClientConnectCB, null);
        }

        private static void SendTcpCB(IAsyncResult result)
        {
            Socket socket = (Socket)result.AsyncState;
            socket.EndSend(result);
        }
        private static void SendUdpCB(IAsyncResult result)
        {
            Socket socket = (Socket)result.AsyncState;
            socket.EndSendTo(result);
        }
        /// <summary>
        /// Sends a packet to a client
        /// </summary>
        /// <param name="toClient">Client that will receive the packet</param>
        /// <param name="buffer">DataBuffer that contains the packet data</param>
        /// <param name="protocol">Protocol that the data is sent over, Tcp or Udp</param>
        public static void Send(in Guid toClient, in DataBuffer buffer, in ProtocolType protocol)
        {
            if (!clients.ContainsKey(toClient)) { return; }
            if (buffer.Length() > 0)
            {
                buffer.WriteLength();
                byte[] data = buffer.ToArray();
                Client client = clients[toClient];
                if (protocol == ProtocolType.Tcp)
                {
                    // Send over Tcp
                    if (client.socket.Connected)
                    {
                        client.socket.BeginSend(data, 0, data.Length, SocketFlags.None, SendTcpCB, client.socket);
                    }
                }
                else if (protocol == ProtocolType.Udp)
                {
                    // Send over Udp
                    if (client.endPoint != null)
                    {
                        socket.BeginSendTo(data, 0, data.Length, SocketFlags.None, client.endPoint, SendUdpCB, socket);
                    }
                }
            }
        }

        /*
        /// <summary>
        /// DOSEN'T WORK DO NOT USE!!!
        /// Send this packet to all the connected clients
        /// </summary>
        /// <param name="action">The packet method that should be sent</param>
        public static void SendToAll(in Action action)
        {
            foreach (Client client in clients.Values)
            {
                action.Invoke();
            }
        }

        /// <summary>
        /// DOSEN'T WORK DO NOT USE!!!
        /// Send this packet to all the connected clients, execept the ignored client
        /// </summary>
        /// <param name="ignoreClient">Ignore sending to this client</param>
        /// <param name="action">The packet method that should be sent</param>
        public static void SendToAll(in Guid ignoreClient, in Action action)
        {
            foreach (Client client in clients.Values)
            {
                if (client.id != ignoreClient)
                {
                    action.Invoke();
                }
            }
        }*/
        /// <summary>
        /// Get all the connected clients on the server as a List
        /// </summary>
        /// <returns>List of connected clients</returns>
        public static List<Client> GetClients()
        {
            return new List<Client>(clients.Values);
        }

        /// <summary>
        /// Send a ping packet to a client.
        /// </summary>
        /// <param name="toClient">The client the packet will be sent to</param>
        public static void PingToClient(in Guid toClient)
        {
            using (DataBuffer buffer = new DataBuffer((int)ServerPacket.PingClient))
            {
                Send(in toClient, in buffer, ProtocolType.Udp);
            }
        }
        /// <summary>
        /// Send the clients Id to the client
        /// </summary>
        /// <param name="toClient">Client that will receive the packet</param>
        public static void SendGuid(in Guid toClient)
        {
            using (DataBuffer buffer = new DataBuffer((int)ServerPacket.SendGuid))
            {
                buffer.Write(toClient);
                Send(in toClient, in buffer, ProtocolType.Tcp);
            }
        }
        /// <summary>
        /// Send a new player to a client
        /// </summary>
        /// <param name="toClient">Client that will receive the packet</param>
        /// <param name="player">Player data that the client will receive</param>
        public static void SendPlayerToClient(in Guid toClient, in Player player)
        {
            using (DataBuffer buffer = new DataBuffer((int)ServerPacket.SendPlayer))
            {
                buffer.Write(player.id);
                buffer.Write(player.position);
                buffer.Write(player.rotation);

                Send(in toClient, in buffer, ProtocolType.Tcp);
            }
        }
        /// <summary>
        /// Remove a player from a client
        /// </summary>
        /// <param name="toClient">Client that will receive the packet</param>
        /// <param name="removeId">Id of the player that will get removed</param>
        public static void RemovePlayerFromClient(in Guid toClient, in Guid removeId)
        {
            using (DataBuffer buffer = new DataBuffer((int)ServerPacket.RemovePlayer))
            {
                buffer.Write(removeId);
                Send(in toClient, in buffer, ProtocolType.Tcp);
            }
        }
        /// <summary>
        /// Send the position of a player to a client
        /// </summary>
        /// <param name="toClient">Client that will receive the packet</param>
        /// <param name="player">Player data that contains the position</param>
        public static void SendPlayerPosition(in Guid toClient, in Player player)
        {
            using (DataBuffer buffer = new DataBuffer((int)ServerPacket.PlayerPosition))
            {
                buffer.Write(player.id);
                buffer.Write(player.position);
                Send(in toClient, in buffer, ProtocolType.Udp);
            }
        }
    }
}
