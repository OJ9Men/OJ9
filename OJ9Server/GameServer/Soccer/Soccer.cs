using System.Net;
using System.Net.Sockets;

public class Soccer : GameServer
{
    public Soccer(int _inMaxRoomNumber) : base(_inMaxRoomNumber)
    {
        gameType = GameType.Soccer;
    }

    public override void Start()
    {
        Console.WriteLine("Listening Starts");

        listenEndPoint = OJ9Function.CreateIPEndPoint("127.0.0.1:" + OJ9Const.SOCCER_LISTEN_PORT_NUM);
        listener = new Socket(
            listenEndPoint.AddressFamily,
            SocketType.Stream,
            ProtocolType.Tcp
        );

        listener.Bind(listenEndPoint);
        listener.Listen(maxRoomNumber);
        listener.BeginAccept(OnAccepted, null);
    }

    private void OnAccepted(IAsyncResult _asyncResult)
    {
        Client client = new Client(listener.EndAccept(_asyncResult));
        client.BeginReceive(OnReceived);
        
        clients.Add(client);
        listener.BeginAccept(OnAccepted, null);
    }

    private void OnReceived(byte[] _buffer)
    {
        // IPacketBase packetBase = OJ9Function.ByteArrayToObject<IPacketBase>(_buffer);
    }
}