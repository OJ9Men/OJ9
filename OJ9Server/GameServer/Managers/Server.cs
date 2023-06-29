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
            ConfigurationManager.AppSettings.Get("fluffy_war_db"),
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
        var userInfo = CheckAccount(packet.id, packet.pw);

        var newClient = new Client(_stateObject.socket, userInfo);
        clients.Add(newClient);

        var sendPacket = new S2CLogin(userInfo);
        _stateObject.socket.Send(OJ9Function.ObjectToByteArray(sendPacket));
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
                
                hasAccount = true;
            }
            
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