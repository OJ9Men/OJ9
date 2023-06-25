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
    private Action<PacketBase>[] packetHandlers;

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
        packetHandlers = new Action<PacketBase>[(int)PacketType.Max];
        BindPacketHandlers();

        var listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        listener.Bind(new IPEndPoint(IPAddress.Any, OJ9Const.SERVER_PORT_NUM));
        listener.Listen();
        listener.BeginAccept(OJ9Const.RECEIVE_SIZE, OnAccept, listener);
    }

    private void BindPacketHandlers()
    {
        packetHandlers[(int)PacketType.Login] = HandleLogin;
    }

    private void OnAccept(IAsyncResult _asyncResult)
    {
        var listener = (Socket)_asyncResult.AsyncState!;
        var clientSocket = listener.EndAccept(out var _, _asyncResult);

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

        listener.BeginAccept(OJ9Const.RECEIVE_SIZE, OnAccept, listener);
    }

    private void OnDataReceived(IAsyncResult _asyncResult)
    {
        String content = String.Empty;
        StateObject stateObject = (StateObject)_asyncResult.AsyncState;
        Socket socket = stateObject.socket;
        var byteRead = socket.EndReceive(_asyncResult);
        if (byteRead <= 0)
        {
            return;
        }

        stateObject.stringBuilder.Append(Encoding.UTF8.GetString(
            stateObject.buffer, 0, byteRead));
        content = stateObject.stringBuilder.ToString();

        if (content.Length != 0)
        {
            ProcessPacket(Encoding.UTF8.GetBytes(content));
        }
        else
        {
            // Read more data
            socket.BeginReceive(
                stateObject.buffer,
                0,
                OJ9Const.BUFFER_SIZE,
                0,
                new AsyncCallback(OnDataReceived),
                stateObject);
        }
    }

    private void ProcessPacket(byte[] buffer)
    {
        var packetBase = OJ9Function.ByteArrayToObject<PacketBase>(buffer);
        var packetHandler = packetHandlers[(int)packetBase.packetType];
        if (packetHandler is null)
        {
            Console.WriteLine("Need to be binded. Packet type is " + packetBase.packetType);
        }
        else
        {
            packetHandlers[(int)packetBase.packetType](packetBase);
        }
    }

    private void HandleLogin(PacketBase _packet)
    {
        var packet = (C2SLogin)_packet;
        
    }
}