using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace MUD_Server
{
    class Client
    {
        public EndPoint endPoint;           // Used by the UDP socket, IpAddress, sort of a ID
        public Socket socket;               // TCP Socket
        private byte[] buffer;              // TCP receive buffer
        private bool closeUDP;

        public Guid id;                     // Client connection id
        public Player player;               // Player Data

        public Client(Socket newTCPSocket, Guid newCid)
        {
            id = newCid;
            player = new Player(id);

            // TCP
            socket = newTCPSocket;
            socket.NoDelay = true;
            buffer = new byte[Server.maxBufferSize];
            socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, ReceiveTcpCB, socket);
            Console.WriteLine($"[{DateTime.Now.ToString("H:mm:ss")}] Connection | cid: {id} | {socket.RemoteEndPoint.ToString()}");
        }

        private void ReceiveTcpCB(IAsyncResult result)
        {
            try
            {
                if (socket.Connected)
                {
                    if (socket.EndReceive(result) > 0)
                    {
                        HandleData.Read(id, buffer);
                        if (socket.Connected)
                            socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, ReceiveTcpCB, socket);
                    }
                    else
                    {
                        //TODO: Close connection of client that send 0 data
                        Disconnet();
                    }
                }
            }
            catch (Exception ex)
            {
                //CloseConnection();
                Disconnet();
                Console.WriteLine($"\n[TCP] Receive Error:\n{ex.ToString()}\n");
            }
        }

        public void ConnectUdp()
        {
            Server.socket.BeginReceiveFrom(Server.buffer, 0, Server.buffer.Length, SocketFlags.None, ref endPoint, ReceiveUdpCB, Server.socket);
        }

        private void ReceiveUdpCB(IAsyncResult result)
        {
            try
            {
                if (endPoint == null) { return; }
                Server.socket.EndReceiveFrom(result, ref endPoint);
                HandleData.Read(Guid.Empty, Server.buffer);
                Server.socket.BeginReceiveFrom(Server.buffer, 0, Server.buffer.Length, SocketFlags.None, ref endPoint, ReceiveUdpCB, Server.socket);

            }
            catch (Exception ex)
            {
                //CloseConnection();  // should we close tcp connection when udp fail?
                Console.WriteLine($"\n[UDP] Receive Error:\n{ex.ToString()}\n");
            }
        }

        private void CloseConnection()
        {
            Console.WriteLine($"[{DateTime.Now.ToString("H:mm:ss")}] Disconnect | cid: {id} | {socket.RemoteEndPoint.ToString()}");
            endPoint = null;
            if (Server.clients.ContainsKey(id))
                Server.clients.Remove(id);

            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
        }

        public void Disconnet()
        {
            foreach (Client client in Server.GetClients())
                if (client.id != id)
                    Server.RemovePlayerFromClient(in client.id, in id);

            CloseConnection();
        }

    }
}
