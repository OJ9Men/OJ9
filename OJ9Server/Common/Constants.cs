public static class Constants
{
    public static int PORT_NUM = 5000;
    
    // It does not work
    public static byte[] ObjectToByteArray(object obj)
    {
        var objToString = System.Text.Json.JsonSerializer.Serialize(obj);
        return System.Text.Encoding.UTF8.GetBytes(objToString);
    }
    
    public static object ByteArrayToObject(byte[] bytes)
    {
        var stringObj = System.Text.Encoding.UTF8.GetString(bytes);
        return System.Text.Json.JsonSerializer.Deserialize<object>(stringObj);
    }
}

public enum PacketType
{
    Test
}
public class IPacketBase
{
    public PacketType packetType;
}

public class C2LoginTest : IPacketBase
{
    public C2LoginTest(int _port, string _str)
    {
        packetType = PacketType.Test;
        port = _port;
        str = _str;
    }

    public int port;
    public string str;
}

