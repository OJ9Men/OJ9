
// TODO : It should be in config file.

using System;

public static class OJ9Const
{
    public static int LOGIN_SERVER_PORT_NUM = 5000;
    public static int LOBBY_SERVER_PORT_NUM = 5001;
    public static int GAME_SERVER_PORT_NUM = 5002;
    
    // GameServerConst
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
