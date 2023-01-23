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
            case PacketType.AddLobbyAccount:
            {
                L2BAddAccount packet = OJ9Function.ByteArrayToObject<L2BAddAccount>(buffer);
                try
                {
                    AddAccountDB(packet.guid);
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

    private void AddAccountDB(Guid _guid)
    {
        var rand = new Random();
        string dummnyUserName = "플레이어" + rand.Next();
        MySqlCommand sqlCommand = new MySqlCommand(
            string.Format("INSERT INTO user (guid, nickname) VALUES ('{0}', '{1}')", _guid, dummnyUserName),
            mysql);
        
        if (sqlCommand.ExecuteNonQuery() != 1)
        {
            throw new FormatException("insert data failed");
        }
        
    }
}