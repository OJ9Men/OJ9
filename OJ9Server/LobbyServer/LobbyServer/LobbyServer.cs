using System.Configuration;
using System.Net;
using System.Net.Sockets;
using MySql.Data.MySqlClient;

public class LobbyServer
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
                    ConfigurationManager.AppSettings.Get("lobbyServerPort")
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
            ConfigurationManager.AppSettings.Get("lobbyDatabase"),
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
            case PacketType.L2BError:
            {
                L2BError packet = OJ9Function.ByteArrayToObject<L2BError>(buffer);

                byte[] clientBuff =
                    OJ9Function.ObjectToByteArray(new B2CError(packet.errorType));
                udpClient.Send(clientBuff, clientBuff.Length, OJ9Function.CreateIPEndPoint(packet.clientEndPoint));
            }
                break;
            case PacketType.CheckLobbyAccount:
            {
                L2BCheckAccount packet = OJ9Function.ByteArrayToObject<L2BCheckAccount>(buffer);
                if (!packet.IsLoginSuccess())
                {
                    byte[] sendBuff =
                        OJ9Function.ObjectToByteArray(new B2CError(ErrorType.Unknown));
                    udpClient.Send(sendBuff, sendBuff.Length, OJ9Function.CreateIPEndPoint(packet.clientEndPoint));
                }
                try
                {
                    UserInfo userInfo = GetAccount(packet.guid);
                    if (!userInfo.IsValid())
                    {
                        userInfo = AddAccountDb(packet.guid);
                    }

                    if (!userInfo.IsValid())
                    {
                        throw new FormatException("Userinfo does not exist and cannot create");
                    }
                    
                    EnterLobby(userInfo, OJ9Function.CreateIPEndPoint(packet.clientEndPoint));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
                break;
            case PacketType.EnterGame:
            {
                C2BEnterGame packet = OJ9Function.ByteArrayToObject<C2BEnterGame>(buffer);
                // TODO : process entering game
            }
                break;
            default:
                throw new FormatException("Invalid packet type in LoginServer");
        }

        udpClient.BeginReceive(DataReceived, null);
    }

    private UserInfo GetAccount(Guid _guid)
    {
        MySqlCommand sqlCommand = new MySqlCommand(
            string.Format("SELECT * FROM user WHERE guid = '{0}';", _guid),
            mysql
        );
        sqlCommand.ExecuteNonQuery();
        var reader = sqlCommand.ExecuteReader();

        UserInfo userInfo = new UserInfo();
        while (reader.Read())
        {
            userInfo.guid = _guid;
            userInfo.nickname = reader["nickname"].ToString()!;

            var rating = reader["rating"].ToString();
            userInfo.rating = string.IsNullOrEmpty(rating) ? 0 : Convert.ToInt32(rating);
            break;  // it's unique
        }
        reader.Close();

        return userInfo;
    }

    private UserInfo AddAccountDb(Guid _guid)
    {
        var rand = new Random();
        string dummyUserName = "플레이어" + rand.Next();
        MySqlCommand sqlCommand = new MySqlCommand(
            string.Format("INSERT INTO user (guid, nickname) VALUES ('{0}', '{1}')", _guid, dummyUserName),
            mysql);
        
        if (sqlCommand.ExecuteNonQuery() != 1)
        {
            throw new FormatException("insert data failed");
        }

        return new UserInfo(_guid, dummyUserName, 0);
    }

    private void EnterLobby(UserInfo _userInfo, IPEndPoint _ipEndPoint)
    {
        byte[] sendBuff =
            OJ9Function.ObjectToByteArray(new B2CEnterLobby(_userInfo));
        udpClient.Send(sendBuff, sendBuff.Length, _ipEndPoint);
    }
}