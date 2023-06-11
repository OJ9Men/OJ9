using System;

public static class OJ9Const
{
    public static int INDEX_NONE = -1;
    public static int INVALID_CHAR_TYPE = -1;
    
    public static string SERVER_IP = "127.0.0.1";
    //public static string SERVER_IP = "124.111.89.128";
    public static int LOGIN_TRY_COUNT = 5;

    public static int LOGIN_SERVER_PORT_NUM = 5000;
    public static int LOBBY_SERVER_PORT_NUM = 5001;
    public static int SOCCER_SERVER_PORT_NUM = 5002;
    
    // GameServerConst
    public static int RECEIVE_SIZE = 1024;
    public static int BUFFER_SIZE = 1024;
    public static int MAX_GAME_ROOM_NUM = 1024;
}

public enum GameType
{
    Soccer,
    Max,
}

public enum ServerType
{
    Login,
    Lobby,
    Soccer,
}

public struct UserInfo
{
    public Guid guid { get; set; }
    public string nickname { get; set; }
    public int soccerRate { get; set; }

    public UserInfo()
    {
        guid = Guid.Empty;
        nickname = string.Empty;
        soccerRate = 0;
    }

    public void SetInfo(Guid _guid, string _nickname, int _soccerRate)
    {
        guid = _guid;
        nickname = _nickname;
        soccerRate = _soccerRate;
    }

    public bool IsValid()
    {
        return guid != Guid.Empty;
    }
}

public class ConnectionInfo
{
    public UserInfo userInfo { get; set; }
    public System.Net.IPEndPoint ipEndPoint { get; }
    
    public ConnectionInfo(UserInfo _userInfo, System.Net.IPEndPoint _ipEndPoint)
    {
        userInfo = _userInfo;
        ipEndPoint = _ipEndPoint;
    }
}
