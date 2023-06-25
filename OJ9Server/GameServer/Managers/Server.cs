using System.Collections.Concurrent;
using System.Configuration;
using System.Net;
using System.Net.Sockets;
using System.Text;
using MySql.Data.MySqlClient;

public class Server
{
    private MySqlConnection mysql;
    private ConcurrentBag<Client> clients;
    private Action<byte[], StateObject>[] packetHandlers;

    public void Start()
    {
        string dbServerString = string.Format(
            "Server=localhost;" +
            "Port={0};" +
            "Database={1};" +
            "Uid={2};" +
            "Pwd={3}",
            ConfigurationManager.AppSettings.Get("dbPort"),
            ConfigurationManager.AppSettings.Get("loginDatabase"),
            ConfigurationManager.AppSettings.Get("dbUserId"),
            ConfigurationManager.AppSettings.Get("dbUserPw")
        );
        mysql = new MySqlConnection(dbServerString);
        mysql.Open();
        clients = new ConcurrentBag<Client>();
        packetHandlers = new Action<byte[], StateObject>[(int)PacketType.Max];
        BindPacketHandlers();

        var listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        listener.Bind(new IPEndPoint(IPAddress.Any, OJ9Const.SERVER_PORT_NUM));
        listener.Listen(10);
        listener.BeginAccept(OnAccept, listener);
        
        Console.WriteLine("Server is ready!");
    }

    private void BindPacketHandlers()
    {
        packetHandlers[(int)PacketType.Login] = HandleLogin;
    }

    private void OnAccept(IAsyncResult _asyncResult)
    {
        var listener = (Socket)_asyncResult.AsyncState!;
        var clientSocket = listener.EndAccept(_asyncResult);

        StateObject stateObject = new StateObject();
        stateObject.socket = clientSocket;
        clientSocket.BeginReceive(
            stateObject.buffer,
            0,
            stateObject.buffer.Length,
            SocketFlags.None,
            OnDataReceived,
            stateObject
        );

        listener.BeginAccept(OnAccept, listener);
    }

    private void OnDataReceived(IAsyncResult _asyncResult)
    {
        StateObject stateObject = (StateObject)_asyncResult.AsyncState;
        Socket socket = stateObject.socket;
        var received = socket.EndReceive(_asyncResult);
        if (received <= 0)
        {
            return;
        }

        byte[] buffer = new byte[received];
        Array.Copy(stateObject.buffer, 0, buffer, 0 ,received);
        ProcessPacket(buffer, stateObject);
    }

    private void ProcessPacket(byte[] buffer, StateObject _stateObject)
    {
        var packetBase = OJ9Function.ByteArrayToObject<PacketBase>(buffer);
        var packetHandler = packetHandlers[(int)packetBase.packetType];
        if (packetHandler is null)
        {
            Console.WriteLine("Need to be binded. Packet type is " + packetBase.packetType);
        }
        else
        {
            packetHandlers[(int)packetBase.packetType](buffer, _stateObject);
        }
    }

    private void HandleLogin(byte[] _buffer, StateObject _stateObject)
    {
        var packet = OJ9Function.ByteArrayToObject<C2SLogin>(_buffer);
    }
}