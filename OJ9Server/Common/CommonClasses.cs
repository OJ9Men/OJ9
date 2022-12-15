using System.Runtime.Serialization.Formatters.Binary;

class MessagePacket
{
    public int playerId;
    public string message;
    public MessagePacket(int _playerId, string _message)
    {
        playerId = _playerId;
        message = _message;
    }
}

// TODO : Serialize and deserialize
//T FromBytes<T>(byte[] data)
//{
//    if (data == null)
//    {
//        return default(T);
//    }
//
//    BinaryFormatter bf = new BinaryFormatter();
//    using MemoryStream ms = new MemoryStream(data))
//    {
//        object obj = bf.Deserialize(ms);
//        return (T)obj;
//    }
//}