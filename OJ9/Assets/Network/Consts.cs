
// TODO : It should be in config file.

using System;
using System.Net;

public static class OJ9Const
{
    public static string SERVER_IP = "127.0.0.1";
    //public static string SERVER_IP = "124.111.89.128";
    public static int LOGIN_TRY_COUNT = 5;

    public static int LOGIN_SERVER_PORT_NUM = 5000;
    public static int LOBBY_SERVER_PORT_NUM = 5001;
    public static int SOCCER_SERVER_PORT_NUM = 5002;
    
    // GameServerConst
    public static int BUFFER_SIZE = 1024;
    public static int MAX_GAME_ROOM_NUM = 1024;
}

public enum GameType
{
    Soccer,
    Dummy1,
    Dummy2,
    Dummy3,
    Max,
}

public struct UserInfo
{
    public Guid guid { get; set; }
    public string nickname { get; set; }
    public int rating { get; set; }

    public UserInfo(Guid _guid, string _nickname, int _rating)
    {
        guid = _guid;
        nickname = _nickname;
        rating = _rating;
    }

    public bool IsValid()
    {
        return guid != Guid.Empty;
    }
}

public class WaitingClient
{
    public UserInfo userInfo;
    public IPEndPoint ipEndPoint;
    
    public WaitingClient(UserInfo _userInfo, IPEndPoint _ipEndPoint)
    {
        userInfo = _userInfo;
        ipEndPoint = _ipEndPoint;
    }
}
