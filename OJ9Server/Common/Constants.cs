public static class Constants
{
    public static int PORT_NUM = 5000;

    // It does not work
    public static byte[] ObjectToByteArray(object obj)
    {
        string objToString = System.Text.Json.JsonSerializer.Serialize(obj);
        return System.Text.Encoding.UTF8.GetBytes(objToString);
    }

    public static T ByteArrayToObject<T>(byte[] bytes)
    {
        string stringObj = System.Text.Encoding.UTF8.GetString(bytes);
        return System.Text.Json.JsonSerializer.Deserialize<T>(stringObj);
    }
}

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