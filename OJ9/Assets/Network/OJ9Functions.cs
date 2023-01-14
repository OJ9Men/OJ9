using System;
using System.Globalization;
using System.Net;

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

    public static IPEndPoint CreateIPEndPoint(string _endPoint)
    {
        string[] ep = _endPoint.Split(':');
        if (ep.Length < 2)
        {
            throw new FormatException("invalid endPoint");
        }

        IPAddress ip;
        if (ep.Length > 2)
        {
            if (!IPAddress.TryParse(string.Join(":", ep, 0, ep.Length - 1), out ip))
            {
                throw new FormatException("invalid ip address");
            }
        }
        else
        {
            if (!IPAddress.TryParse(ep[0], out ip))
            {
                throw new FormatException("invalid ip address");
            }
        }

        int port;
        if (!int.TryParse(ep[ep.Length - 1], NumberStyles.None, NumberFormatInfo.CurrentInfo, out port))
        {
            throw new FormatException("invalid port");
        }

        return new IPEndPoint(ip, port);
    }
}