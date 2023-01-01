public static class OJ9Const
{
    public static int PORT_NUM = 5000;
}

public static class OJ9Function
{
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