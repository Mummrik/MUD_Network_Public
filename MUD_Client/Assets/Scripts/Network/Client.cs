using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class Client
{
    private static EndPoint endPoint;
    public static Socket SocketTcp { get { return socketTcp; } }
    private readonly static Socket socketTcp = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    public static Socket SocketUdp { get { return socketUdp; } }
    private readonly static Socket socketUdp = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

    public static DataBuffer receivedData;
    private static byte[] bufferTcp;
    private static byte[] bufferUdp;
    private static int bufferSize = 4096 * 2;

    public static void Connect(string host, int port)
    {
        endPoint = new IPEndPoint(IPAddress.Parse(host), port);
        IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, 0);
        //Tcp
        bufferTcp = new byte[bufferSize];
        socketTcp.Bind(localEndPoint);
        socketTcp.Connect(endPoint);
        receivedData = new DataBuffer();
        socketTcp.BeginReceive(bufferTcp, 0, bufferTcp.Length, SocketFlags.None, ReceiveTcpCB, socketTcp);

        //Udp
        bufferUdp = new byte[bufferSize];
        socketUdp.Bind(socketTcp.LocalEndPoint);
        socketUdp.BeginReceiveFrom(bufferUdp, 0, bufferUdp.Length, SocketFlags.None, ref endPoint, RecevieUdpCB, socketUdp);
    }

    public static void CloseConnection()
    {
        if (socketUdp.Connected)
        {
            socketUdp.Shutdown(SocketShutdown.Both);
            socketUdp.Close();
        }
        if (socketTcp.Connected)
        {
            socketTcp.Shutdown(SocketShutdown.Both);
            socketTcp.Close();
        }
    }

    private static void ReceiveTcpCB(IAsyncResult result)
    {
        if (NetworkManager.LogBytes)
            Debug.Log($"Received: '{socketTcp.EndReceive(result)}' bytes | Protocol: '{socketTcp.ProtocolType}'");
        else
            socketTcp.EndReceive(result);

        receivedData.Reset(HandleData.Read(in bufferTcp, socketTcp.ProtocolType));
        socketTcp.BeginReceive(bufferTcp, 0, bufferTcp.Length, SocketFlags.None, ReceiveTcpCB, socketTcp);
    }

    private static void RecevieUdpCB(IAsyncResult result)
    {
        if (NetworkManager.LogBytes)
            Debug.Log($"Received: '{socketUdp.EndReceiveFrom(result, ref endPoint)}' bytes | Protocol: '{socketUdp.ProtocolType}'");
        else
            socketUdp.EndReceiveFrom(result, ref endPoint);

        HandleData.Read(in bufferUdp, socketUdp.ProtocolType);
        socketUdp.BeginReceiveFrom(bufferUdp, 0, bufferUdp.Length, SocketFlags.None, ref endPoint, RecevieUdpCB, socketUdp);
    }

    public static void SendData(in DataBuffer data, in ProtocolType protocol)
    {
        data.WriteLength();

        if (protocol == ProtocolType.Tcp)
        {
            try
            {
                if (socketTcp.Connected)
                {
                    if (NetworkManager.LogBytes)
                        Debug.Log($"Sent: '{socketTcp.Send(data.ToArray())}' bytes | Protocol: '{protocol}'");
                    else
                        socketTcp.Send(data.ToArray());
                }
            }
            catch (Exception ex)
            {
                Debug.Log($"TCP sending Error:\n{ex}");
            }

        }
        else if (protocol == ProtocolType.Udp)
        {
            try
            {
                if (socketTcp.Connected)
                {
                    // Server need to know who sent the packet over UDP
                    data.InsertId(NetworkManager.Id);
                    if (NetworkManager.LogBytes)
                        Debug.Log($"Sent: '{socketUdp.SendTo(data.ToArray(), endPoint)}' bytes | Protocol: '{protocol}'");
                    else
                        socketUdp.SendTo(data.ToArray(), endPoint);
                }
            }
            catch (Exception ex)
            {
                Debug.Log($"UDP sending Error:\n{ex}");
            }
        }

        if (!socketTcp.Connected && !socketUdp.Connected)
        {
            NetworkManager.ping.text = "Offline";
            NetworkManager.ping.color = Color.red;
            Debug.Log("No connection to the server!");
        }

    }

    public static void PingToServer()
    {
        using (DataBuffer buffer = new DataBuffer((int)ClientPacket.PingServer))
        {
            SendData(in buffer, ProtocolType.Udp);
        }
    }

    public static void Disconnect()
    {
        using (DataBuffer buffer = new DataBuffer((int)ClientPacket.Disconnect))
        {
            SendData(in buffer, ProtocolType.Tcp);
        }
    }

    public static void RequestMyPlayer()
    {
        using (DataBuffer buffer = new DataBuffer((int)ClientPacket.RequestMyPlayer))
        {
            buffer.Write(socketUdp.LocalEndPoint.ToString());
            SendData(in buffer, ProtocolType.Tcp);
        }
    }

    public static void RequestPlayer(Guid cid)
    {
        using (DataBuffer buffer = new DataBuffer((int)ClientPacket.RequestPlayer))
        {
            buffer.Write(cid);
            SendData(in buffer, ProtocolType.Tcp);
        }
    }
    public static void RequestNeighbourPlayer()
    {
        using (DataBuffer buffer = new DataBuffer((int)ClientPacket.RequestNeighbourPlayer))
        {
            SendData(in buffer, ProtocolType.Tcp);
        }
    }

    public static void KeyInput(int inputId)
    {
        using (DataBuffer buffer = new DataBuffer((int)ClientPacket.KeyInput))
        {
            buffer.Write(inputId);
            SendData(in buffer, ProtocolType.Tcp);
        }
    }
}
