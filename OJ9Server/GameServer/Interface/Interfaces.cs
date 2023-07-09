using System.Net;
using System.Net.Sockets;
using System.Text;

public struct Client
{
    public Socket socket;
    public UserInfo userInfo;

    public Client(Socket _socket, UserInfo _userInfo)
    {
        socket = _socket;
        userInfo = _userInfo;
    }
}
public class StateObject
{
    public Socket socket = null;
    public byte[] buffer = new byte[OJ9Const.BUFFER_SIZE];
    public StringBuilder stringBuilder = new StringBuilder();
}

public abstract class Manager
{
    
}
