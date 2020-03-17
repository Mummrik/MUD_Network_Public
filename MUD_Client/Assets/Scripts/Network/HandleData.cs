using System;
using System.Collections.Generic;
using UnityEngine;

public class HandleData
{
    public static Dictionary<int, Action<DataBuffer>> packetList;

    public static void InitPacketList()
    {
        packetList = new Dictionary<int, Action<DataBuffer>>();
        //Add server packets here
        packetList.Add((int)ServerPacket.PingClient, PingClient);
        packetList.Add((int)ServerPacket.SendGuid, NewGuid);
        packetList.Add((int)ServerPacket.SendPlayer, NewPlayer);
        packetList.Add((int)ServerPacket.RemovePlayer, RemovePlayer);
        packetList.Add((int)ServerPacket.PlayerPosition, PlayerPosition);
    }

    public static bool Read(in byte[] data, in System.Net.Sockets.ProtocolType protocol)
    {
        if (protocol == System.Net.Sockets.ProtocolType.Tcp)
        {
            int length = 0;

            Client.receivedData.SetBytes(data);

            if (Client.receivedData.UnreadLength() >= 4)
            {
                length = Client.receivedData.ReadInt();
                if (length <= 0) { return true; }
            }

            while (length > 0 && length <= Client.receivedData.UnreadLength())
            {
                byte[] packetData = Client.receivedData.ReadBytes(length);
                UnityThread.executeInUpdate(() =>
                {
                    using (DataBuffer buffer = new DataBuffer(packetData))
                    {
                        int packetId = buffer.ReadInt();
                        if (packetList.TryGetValue(packetId, out Action<DataBuffer> packet))
                        {
                            packet.Invoke(buffer);
                        }
                    }
                });

                length = 0;
                if (Client.receivedData.UnreadLength() >= 4)
                {
                    length = Client.receivedData.ReadInt();
                    if (length <= 0) { return true; }
                }
            }

            if (length <= 1) { return true; }
        }
        else if (protocol == System.Net.Sockets.ProtocolType.Udp)
        {
            byte[] packetData;
            using (DataBuffer buffer = new DataBuffer(data))
            {
                int length = buffer.ReadInt();
                packetData = buffer.ReadBytes(length);
            }
            UnityThread.executeInUpdate(() =>
            {
                using (DataBuffer buffer = new DataBuffer(packetData))
                {
                    int packetId = buffer.ReadInt();
                    if (packetList.TryGetValue(packetId, out Action<DataBuffer> packet))
                    {
                        packet.Invoke(buffer);
                    }
                }
            });
        }
        return false;
    }

    private static void PingClient(DataBuffer data)
    {
        NetworkManager.DisplayPing();
        //Client.PACKAGE_PingToServer();
    }

    private static void NewGuid(DataBuffer data)
    {
        Guid cid = data.ReadGuid();
        if (NetworkManager.Id == Guid.Empty)
            NetworkManager.Id = cid;

        Client.RequestMyPlayer();
        //Client.RequestNeighbourPlayer();
    }

    private static void NewPlayer(DataBuffer data)
    {
        Player player = NetworkManager.SpawnPlayer(data.ReadGuid());
        if (player != null)
        {
            Vector3 position = data.ReadVector3();
            Quaternion rotation = data.ReadQuaternion();

            player.position = position;
            // only values  from 0 - 360 allowed
            //player.rotation = new Quaternion(0, Mathf.Clamp(rotation.y, 0, 360), 0, 0);
            player.rotation = rotation;

            player.transform.position = position;
            player.transform.rotation = rotation;

            //Just for testing
            NetworkManager.SetupMapRegion(position);
        }
    }

    private static void RemovePlayer(DataBuffer data)
    {
        bool removed;
        Guid playerId = data.ReadGuid();
        if (NetworkManager.players.ContainsKey(playerId))
        {
            do
            {
                removed = NetworkManager.DespawnPlayer(playerId);
            } while (removed == false);
        }
    }

    private static void PlayerPosition(DataBuffer data)
    {
        Guid cid = data.ReadGuid();
        Vector3 position = data.ReadVector3();
        if (!NetworkManager.players.ContainsKey(cid)) { Client.RequestPlayer(cid); return; }
        Player player = NetworkManager.players[cid];
        player.transform.position = position;
        player.position = player.transform.position;

        if (cid == NetworkManager.Id)
            NetworkManager.SetStatusText(player.position.ToString());
    }

}
