// Copy and past to client/server if changes are made to this file!
public enum ServerPacket
{
    PingClient = 1,
    SendGuid,
    SendPlayer,
    RemovePlayer,
    PlayerPosition,
}

public enum ClientPacket
{
    PingServer = 1,
    Disconnect,
    RequestMyPlayer,
    RequestPlayer,
    RequestNeighbourPlayer,
    KeyInput,
}