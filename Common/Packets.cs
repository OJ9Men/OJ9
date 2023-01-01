public enum PacketType
{
    Test
}

public class IPacketBase
{
    public PacketType packetType { get; set; }
}

public class C2LoginTest : IPacketBase
{
    public C2LoginTest()
    {
        
    }
    public C2LoginTest(int _port, string _str)
    {
        packetType =  PacketType.Test;
        port = _port;
        str = _str;
    }

    public int port { get; set; }
    public string str { get; set; }
}