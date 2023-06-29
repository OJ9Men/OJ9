using System.Configuration;
using System.Net;
using System.Net.Sockets;
using MySql.Data.MySqlClient;

public class LoginManager : Manager
{
    private readonly UdpClient listener;
    private readonly MySqlConnection mySqlConnection;

    public LoginManager()
    {
        listener = new UdpClient(
            Convert.ToInt32(
                ConfigurationManager.AppSettings.Get("loginPort")
            )
        );
        
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

        mySqlConnection = new MySqlConnection(dbServerString);
    }
    
    public void Start()
    {
        listener.BeginReceive(DataReceived, null);
        mySqlConnection.Open();
    }

    private void DataReceived(IAsyncResult _asyncResult)
    {
        IPEndPoint ipEndPoint = null;
        var buffer = listener.EndReceive(_asyncResult, ref ipEndPoint);

        var packetBase = OJ9Function.ByteArrayToObject<PacketBase>(buffer);
        switch (packetBase.packetType)
        {
            case PacketType.Login:
            {
                C2SLogin packet = OJ9Function.ByteArrayToObject<C2SLogin>(buffer);
                var userInfo = CheckUserInfo(packet.id, packet.pw);
                // TODO : notify to server
                break;
            }
            default:
                throw new FormatException("Invalid packet type in LoginServer");
        }
    }

    public UserInfo TryLogin(string _id, string _pw)
    {
        return new UserInfo();
    }

    private UserInfo CheckUserInfo(string _id, string _pw)
    {
        UserInfo userInfo = new UserInfo();
        {
            MySqlCommand sqlCommand = new MySqlCommand(
                string.Format(
                    "SELECT guid, nick_name, last_login_utc, soccer_rate FROM info WHERE id = '{0}' AND pw = '{1}';",
                    _id, _pw),
                mySqlConnection);
            sqlCommand.ExecuteNonQuery();

            var reader = sqlCommand.ExecuteReader();
            while (reader.Read())
            {
                var guid = reader["guid"].ToString();
                var nickName = reader["nick_name"].ToString();
                var loginUtc = reader["last_login_utc"].ToString();   // TODO : save login timestamp
                var soccerRate = reader["soccer_rate"].ToString();

                userInfo.SetInfo(
                    Guid.Parse(guid),
                    nickName,
                    Convert.ToInt32(loginUtc),
                    Convert.ToInt32(soccerRate));
            }

            reader.Close();
        }

        if (!userInfo.IsValid())
        {
            var rand = new Random((int)System.DateTime.Now.Ticks);
            string dummyUserName = "플레이어" + rand.Next();
        
            Guid guid = Guid.NewGuid();
            var loginUtc = ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds();
            MySqlCommand sqlCommand = new MySqlCommand(
                string.Format("INSERT INTO info (guid, id, pw, nick_name, last_login_utc) VALUES ('{0}', '{1}', '{2}', '{3}', '{4}')", guid, _id, _pw, dummyUserName, loginUtc),
                mySqlConnection);

            if (sqlCommand.ExecuteNonQuery() != 1)
            {
                throw new FormatException("insert data failed");
            }
        
            userInfo.SetInfo(guid, dummyUserName, loginUtc, 0);
        }

        return userInfo;
    }
}
