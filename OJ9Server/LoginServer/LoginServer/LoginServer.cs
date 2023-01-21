using System.Configuration;
using System.Net;
using System.Net.Sockets;
using MySql.Data.MySqlClient;

class LoginServer
{
    private UdpClient udpClient;
    private MySqlConnection mysql;

    public void Start()
    {
        try
        {
            // StartDB();
            
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
            ConfigurationManager.AppSettings.Get("Database"),
            ConfigurationManager.AppSettings.Get("dbUserId"),
            ConfigurationManager.AppSettings.Get("dbUserPw")
        );

        mysql = new MySqlConnection(dbServerString);
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
                throw new ArgumentOutOfRangeException();
        }

        udpClient.BeginReceive(DataReceived, null);
    }

    private void StartLogin(C2LLogin packet, IPEndPoint ipEndPoint)
    {
        // TODO : Check id / pw
        Console.WriteLine("Id : " + packet.id + " pw : " + packet.pw);

        byte[] sendBuff = OJ9Function.ObjectToByteArray(new L2CLogin("1923759127378", "Hello World"));
        udpClient.Send(sendBuff, ipEndPoint);
    }
}