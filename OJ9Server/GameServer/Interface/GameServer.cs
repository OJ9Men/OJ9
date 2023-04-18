using System.Net;
using System.Net.Sockets;
using System.Text;

public struct Client
{
    private readonly Socket? socket;
    private readonly byte[] buffer = new byte[OJ9Const.BUFFER_SIZE];
    
    public UserInfo userInfo;

    public Client(Socket _socket, UserInfo _userInfo)
    {
        socket = _socket;
        userInfo = _userInfo;
    }

    public delegate void OnReceivedCallback(byte[] _buffer, ref Client _client);
    
    public void BeginReceive(OnReceivedCallback _onReceivedCallback)
    {
        socket.BeginReceive(
            buffer,
            0,
            buffer.Length,
            SocketFlags.None,
            EndReceive,
            _onReceivedCallback
        );
    }

    public void Send(byte[] packet)
    {
        if (socket == null)
        {
            throw new FormatException("No socket! Send before socket is set");
        }
        
        socket.Send(packet);
    }

    private void EndReceive(IAsyncResult _asyncResult)
    {
        var callback = (OnReceivedCallback)_asyncResult.AsyncState!;
        var size = socket.EndReceive(_asyncResult);
        var stringFromBuffer = Encoding.UTF8.GetString(buffer, 0, size);
        callback(Encoding.UTF8.GetBytes(stringFromBuffer), ref this);
        
        socket.BeginReceive(
            buffer,
            0,
            buffer.Length,
            SocketFlags.None,
            EndReceive,
            callback
        );
    }
}

public abstract class GameServer
{
    protected UdpClient lobbyListener;
    // Listen
    protected Socket listener;

    protected readonly List<Client> clients = new List<Client>();
    protected GameType gameType;

    public abstract void Start();
}