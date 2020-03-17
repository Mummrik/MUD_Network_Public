using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(InputManager))]
public class NetworkManager : MonoBehaviour
{
    public static bool LogBytes = false;
    public string host = "127.0.0.1";
    public int port = 7171;
    public static Text status;
    public static Text ping;
    [System.NonSerialized] public static Stopwatch pingTimer;

    public static Guid Id { get => id; set => SetGuid(value); }
    private static Guid id;

    public static GameObject playerPrefab;
    public static GameObject tilePrefab;
    public static Dictionary<Guid, Player> players = new Dictionary<Guid, Player>();

    private static GameObject world;

    void Awake()
    {
        DontDestroyOnLoad(this);

        playerPrefab = Resources.Load("Prefabs/Player") as GameObject;
        tilePrefab = Resources.Load("Prefabs/Tile") as GameObject;
        world = new GameObject("World");

        ping = GameObject.Find("Ping_Text").GetComponent<Text>();
        status = GameObject.Find("Status_Text").GetComponent<Text>();
        pingTimer = new Stopwatch();
        UnityThread.initUnityThread();
        HandleData.InitPacketList();
        try
        {
            SetStatusText(host);
            Client.Connect(host, port);
            ping.color = Color.green;
        }
        catch
        {
            // No Connection to the server
            ping.text = "Offline";
            ping.color = Color.red;
        }
    }

    private void OnApplicationQuit()
    {
        if (Client.SocketTcp.Connected || Client.SocketUdp.Connected)
        {
            if (Client.SocketTcp.Connected)
                Client.Disconnect();

            Client.CloseConnection();
        }
    }

    internal static void DisplayPing()
    {
        pingTimer.Stop();
        //long ms = pingTimer.ElapsedMilliseconds / 10;
        long ms = pingTimer.ElapsedMilliseconds;
        ping.text = $"{ms.ToString()}";
        pingTimer.Restart();
    }

    private static void SetGuid(Guid _id)
    {
        if (id == Guid.Empty)
            id = _id;
    }

    public static Player SpawnPlayer(Guid cid)
    {
        if (!players.ContainsKey(cid))
        {
            GameObject player = Instantiate(playerPrefab);
            player.name = $"Player | {cid}";

            players.Add(cid, player.GetComponent<Player>());

            if (cid == Id)
            {
                Camera.main.GetComponent<CameraController>().SetupCamera(player.transform);
            }

            return player.GetComponent<Player>();
        }

        return null;
    }

    public static bool DespawnPlayer(Guid cid)
    {
        if (players.ContainsKey(cid))
        {
            Player player = players[cid];
            players.Remove(cid);
            Destroy(player.gameObject);
            return true;
        }
        return false;
    }

    public static void SetStatusText(string msg)
    {
        status.text = msg;
    }

    public static void SetupMapRegion(Vector3 origin)
    {
        for (float x = origin.x - 36f; x < origin.x + 36f; x++)
        {
            for (float z = origin.z - 36; z < origin.z + 36; z++)
            {
                // Just for testing this should be handle by the server later
                GameObject tile = Instantiate(tilePrefab, new Vector3(x, 0, z), tilePrefab.transform.rotation, world.transform);
                tile.GetComponent<SpriteRenderer>().color = new Color32(0, 100, 0, 255);
            }
        }
    }
}
