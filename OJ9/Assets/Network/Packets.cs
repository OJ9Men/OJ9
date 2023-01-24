using System;
using System.Net;

public enum PacketType
{
    Login,
    CheckLobbyAccount,
    EnterLobby,
    EnterGame,
}

public class IPacketBase
{
    public PacketType packetType { get; set; }
}

public class C2LLogin : IPacketBase
{
    public string id { get; set; }
    public string pw { get; set; }

    public C2LLogin()
    {
    }

    public C2LLogin(string _id, string _pw)
    {
        packetType = PacketType.Login;
        id = _id;
        pw = _pw;
    }
}

public class L2BCheckAccount : IPacketBase
{
    public Guid guid { get; set; }
    public string clientEndPoint { get; set; }

    public L2BCheckAccount()
    {
        
    }

    public L2BCheckAccount(Guid _guid, string _ipEndPoint)
    {
        packetType = PacketType.CheckLobbyAccount;
        guid = _guid;
        clientEndPoint = _ipEndPoint;
    }
}

public class C2BEnterGame : IPacketBase
{
    public Guid guid { get; set; }
    public GameType gameType { get; set; }

    public C2BEnterGame()
    {
    }

    public C2BEnterGame(Guid _guid, GameType _gameType)
    {
        packetType = PacketType.EnterGame;
        guid = _guid;
        gameType = _gameType;
    }
}

public class B2CEnterGame : IPacketBase
{
    public UserInfo enemyInfo { get; set; }
    public GameType gameType { get; set; }

    public B2CEnterGame()
    {
        
    }

    public B2CEnterGame(UserInfo _enemyInfo, GameType _gameType)
    {
        packetType = PacketType.EnterGame;
        enemyInfo = _enemyInfo;
        gameType = _gameType;
    }
}

public class L2CLogin : IPacketBase
{
    public Guid guid { get; set; }

    public L2CLogin()
    {
    }

    public L2CLogin(Guid _guid)
    {
        packetType = PacketType.Login;
        guid = _guid;
    }
}

public class B2CEnterLobby : IPacketBase
{
    public UserInfo userInfo { get; set; }
    public B2CEnterLobby()
    {
        
    }

    public B2CEnterLobby(UserInfo _userInfo)
    {
        packetType = PacketType.EnterLobby;
        userInfo = _userInfo;
    }
}