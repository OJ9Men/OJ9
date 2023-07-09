using System.Collections.Concurrent;
using System.Configuration;
using System.Net;
using System.Net.Sockets;
using System.Text;
using MySql.Data.MySqlClient;

public class Server
{
    private MySqlConnection mysql;
    private ConcurrentDictionary<Guid, Client> clients;
    private Action<byte[], StateObject>[] packetHandlers;

    private ConcurrentQueue<Client>[] queueClients;

    public void Start()
    {
        string dbServerString = string.Format(
            "Server=localhost;" +
            "Port={0};" +
            "Database={1};" +
            "Uid={2};" +
            "Pwd={3}",
            ConfigurationManager.AppSettings.Get("dbPort"),
            ConfigurationManager.AppSettings.Get("fluffy_war_db"),
            ConfigurationManager.AppSettings.Get("dbUserId"),
            ConfigurationManager.AppSettings.Get("dbUserPw")
        );
        mysql = new MySqlConnection(dbServerString);
        mysql.Open();
        clients = new ConcurrentDictionary<Guid, Client>();
        packetHandlers = new Action<byte[], StateObject>[(int)PacketType.Max];
        queueClients = new ConcurrentQueue<Client>[(int)GameType.Max];
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
        packetHandlers[(int)PacketType.Start] = HandleStart;
    }

    private void OnAccept(IAsyncResult _asyncResult)
    {
        var listener = (Socket)_asyncResult.AsyncState!;
        var clientSocket = listener.EndAccept(_asyncResult);
        Console.WriteLine(string.Format("[%s] Connected", clientSocket));

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

    private void OnDisconnected(Socket _socket)
    {
        Console.WriteLine(string.Format("[%s] Disconnected", _socket));
        foreach (var iter in clients)
        {
            if (iter.Value.socket == _socket)
            {
                clients.TryRemove(iter.Value.userInfo.guid, out _);
            }
        }
    }

    private void OnDataReceived(IAsyncResult _asyncResult)
    {
        StateObject stateObject = (StateObject)_asyncResult.AsyncState;
        Socket socket = stateObject.socket;
        var received = socket.EndReceive(_asyncResult);
        if (received <= 0)
        {
            if (!socket.Connected)  // It does not work well
            {
                 OnDisconnected(socket);
            }
            return;
        }

        byte[] buffer = new byte[received];
        Array.Copy(stateObject.buffer, 0, buffer, 0, received);
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

    private Client? GetConnectedClient(Guid _guid)
    {
        if (clients.TryGetValue(_guid, out Client client))
        {
            return client;
        }

        return null;
    }

    private void HandleLogin(byte[] _buffer, StateObject _stateObject)
    {
        var packet = OJ9Function.ByteArrayToObject<C2SLogin>(_buffer);
        var userInfo = CheckAccount(packet.id, packet.pw);

        if (userInfo.IsValid())
        {
            var newClient = new Client(_stateObject.socket, userInfo);
            clients.TryAdd(userInfo.guid, newClient);
        }

        var sendPacket = new S2CLogin(userInfo);
        _stateObject.socket.Send(OJ9Function.ObjectToByteArray(sendPacket));
    }

    private void HandleStart(byte[] _buffer, StateObject _stateObject)
    {
        var packet = OJ9Function.ByteArrayToObject<C2SStartGame>(_buffer);

        if (packet.gameType >= GameType.Max)
        {
            Console.WriteLine(string.Format("[%s] : Invalid gametype (%s)", _stateObject.socket, packet.gameType));
            return;
        }

        var client = GetConnectedClient(packet.guid);
        if (client.HasValue)
        {
            queueClients[(int)packet.gameType].Enqueue(client.Value);
            TryMatch();
        }
    }

    private void TryMatch()
    {
        foreach (var clientQueue in queueClients)
        {
            var waitingUserNum = clientQueue.Count;
            if (waitingUserNum >= 2)
            {
                Client first, second;
                clientQueue.TryDequeue(out first);
                clientQueue.TryDequeue(out second);
                StartGame(first, second);
            }
        }
    }

    private void StartGame(Client _first, Client _second)
    {
        // TODO
    }

    private UserInfo CheckAccount(string _id, string _pw)
    {
        var userInfo = new UserInfo();

        MySqlCommand sqlCommand = new MySqlCommand(
            string.Format("SELECT guid, pw, nick_name, last_login_utc, soccer_rate FROM info WHERE id = '{0}';", _id), 
            mysql);
        sqlCommand.ExecuteNonQuery();
        var reader = sqlCommand.ExecuteReader();

        bool hasAccount = false;
        while (reader.Read())
        {
            var pw = reader["pw"];

            if (pw.ToString() == _pw)
            {
                var guid = reader["guid"].ToString();
                var nickName = reader["nick_name"].ToString();
                var loginUtc = reader["last_login_utc"].ToString();
                var soccerRate = reader["soccer_rate"].ToString();
                
                userInfo.SetInfo(
                    Guid.Parse(guid),
                    nickName,
                    Convert.ToInt32(loginUtc),
                    Convert.ToInt32(soccerRate));
                
            }
            
            hasAccount = true;
        }
        reader.Close();
        
        if (!hasAccount)
        {
            userInfo = AddAccount(_id, _pw);
        }
        
        return userInfo;
    }
    
    private UserInfo AddAccount(string _id, string _pw)
    {
        var userInfo = new UserInfo();
        
        var rand = new Random((int)System.DateTime.Now.Ticks);
        string dummyUserName = "플레이어" + rand.Next();
        
        Guid guid = Guid.NewGuid();
        var loginUtc = ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds();
        MySqlCommand sqlCommand = new MySqlCommand(
            string.Format("INSERT INTO info (guid, id, pw, nick_name, last_login_utc, soccer_rate) VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}')", guid, _id, _pw, dummyUserName, loginUtc, 0),
            mysql);

        if (sqlCommand.ExecuteNonQuery() != 1)
        {
            throw new FormatException("insert data failed");
        }
        
        userInfo.SetInfo(guid, dummyUserName, loginUtc, 0);
        return userInfo;
    }
}