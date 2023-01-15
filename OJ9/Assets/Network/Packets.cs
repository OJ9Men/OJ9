public enum PacketType
{
    Login
}

public class IPacketBase
{
    public PacketType packetType { get; set; }
}

public class C2LLogin : IPacketBase
{
    public string id { get; set; }
    public string pw { get; set; }
    public string ip { get; set; }

    public C2LLogin()
    {
    }

    public C2LLogin(string _id, string _pw, string _ip)
    {
        packetType = PacketType.Login;
        id = _id;
        pw = _pw;
        ip = _ip;
    }
}

public class L2CLogin : IPacketBase
{
    public string dbId { get; set; }
    public string welcomeMsg { get; set; }

    public L2CLogin()
    {
    }

    public L2CLogin(string _dbId, string _welcomeMsg)
    {
        packetType = PacketType.Login;
        dbId = _dbId;
        welcomeMsg = _welcomeMsg;
    }
}