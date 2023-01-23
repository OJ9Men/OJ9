using System.Configuration;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using MySql.Data.MySqlClient;

class LoginServer
{
    private UdpClient udpClient;
    private MySqlConnection mysql;

    public void Start()
    {
        try
        {
            StartDB();
            
            udpClient = new UdpClient(
                Convert.ToInt32(
                    ConfigurationManager.AppSettings.Get("loginServerPort")
                )
            );
            udpClient.BeginReceive(DataReceived, null);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private void StartDB()
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
    }

    private void DataReceived(IAsyncResult _asyncResult)
    {
        IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.None, 0);
        var buffer = udpClient.EndReceive(_asyncResult, ref ipEndPoint);
        var packBase = OJ9Function.ByteArrayToObject<IPacketBase>(buffer);
        switch (packBase.packetType)
        {
            case PacketType.Login:
            {
                C2LLogin packet = OJ9Function.ByteArrayToObject<C2LLogin>(buffer);
                StartLogin(packet, ipEndPoint);
            }
                break;
            default:
                throw new FormatException("Invalid packet type in LoginServer");
        }

        udpClient.BeginReceive(DataReceived, null);
    }

    private void StartLogin(C2LLogin _packet, IPEndPoint _ipEndPoint)
    {
        var id = TryLogin(_packet.id, _packet.pw); 
        byte[] sendBuff = OJ9Function.ObjectToByteArray(new L2CLogin(id));
        udpClient.Send(sendBuff, _ipEndPoint);
    }

    private Guid TryLogin(string _id, string _pw)
    {
        var id = Guid.Empty;
        
        MySqlCommand sqlCommand = new MySqlCommand(
            string.Format("SELECT id, pw FROM user_temp WHERE idString = '{0}';", _id), 
            mysql);
        sqlCommand.ExecuteNonQuery();
        var reader = sqlCommand.ExecuteReader();

        bool hasAccount = false;
        while (reader.Read())
        {
            var pw = reader["pw"];

            if (pw.ToString() == _pw)
            {
                id = Guid.Parse(reader["id"].ToString()!);
            }
            hasAccount = true;
        }
        reader.Close();
        if (!hasAccount)
        {
            id = AddAccount(_id, _pw);
        }

        return id;
    }

    private Guid AddAccount(string _id, string _pw)
    {
        Guid guid = Guid.NewGuid();
        MySqlCommand sqlCommand = new MySqlCommand(
            string.Format("INSERT INTO user_temp (id, idString, pw) VALUES ('{0}', '{1}', '{2}')", guid, _id, _pw),
            mysql);

        if (sqlCommand.ExecuteNonQuery() != 1)
        {
            throw new FormatException("insert data failed");
        }
        
        byte[] sendBuff =
            OJ9Function.ObjectToByteArray(new L2BAddAccount(guid));
        IPEndPoint endPoint = OJ9Function.CreateIPEndPoint(
            "127.0.0.1" + ":" +
            ConfigurationManager.AppSettings.Get("lobbyServerPort")
        );
        udpClient.Send(sendBuff, sendBuff.Length, endPoint);

        return guid;
    }
}