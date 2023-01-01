public enum PacketType
{
    Test
}

public class IPacketBase
{
    public PacketType packetType { get; set; }
}

public class C2LTest : IPacketBase
{
    public C2LTest()
    {
    }

    public C2LTest(int _port, string _str)
    {
        packetType = PacketType.Test;
        port = _port;
        str = _str;
    }

    public int port { get; set; }
    public string str { get; set; }
}

public class C2LLogin : IPacketBase
{
    public C2LLogin()
    {
    }

    public C2LLogin(string _id, string _pw)
    {
        id = _id;
        pw = _pw;
    }

    public string id { get; set; }
    public string pw { get; set; }
    public Int64 idKey { get; set; }
}

public class L2CLogin : IPacketBase
{
    public L2CLogin()
    {
    }

    public L2CLogin(int _idKey)
    {
        idKey = _idKey;
    }
    
    public int idKey { get; set; }
}
