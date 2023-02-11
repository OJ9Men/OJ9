using System;
using System.Net;

public enum PacketType
{
    Login,
    CheckLobbyAccount,
    EnterLobby,
    
    // Lobby
    QueueGame,
    Matched,
    
    // Game
    Start,

    // Error
    L2BError,
    B2CError,
}

public enum ErrorType
{
    None,
    WrongPassword,
    Unknown,
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

    public bool IsLoginSuccess()
    {
        return guid != Guid.Empty;
    }
}

public class C2BQueueGame : IPacketBase
{
    public Guid guid { get; set; }
    public GameType gameType { get; set; }

    public C2BQueueGame()
    {
    }

    public C2BQueueGame(Guid _guid, GameType _gameType)
    {
        packetType = PacketType.QueueGame;
        guid = _guid;
        gameType = _gameType;
    }
}

public class B2CGameMatched : IPacketBase
{
    public GameType gameType { get; set; }
    public int roomNumber { get; set; }

    public B2CGameMatched()
    {
        
    }

    public B2CGameMatched(GameType _gameType, int _roomNumber)
    {
        packetType = PacketType.Matched;
        gameType = _gameType;
        roomNumber = _roomNumber;
    }
}

public class C2GGameStart : IPacketBase
{
    // TCP Packet
    
    public GameType gameType { get; set; }
    public int roomNumber { get; set; }

    public C2GGameStart()
    {
        
    }

    public C2GGameStart(GameType _gameType, int _roomNumber)
    {
        packetType = PacketType.Start;
        gameType = _gameType;
        roomNumber = _roomNumber;
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

public class L2BError : IPacketBase
{
    public ErrorType errorType { get; set; }
    public string clientEndPoint { get; set; }

    public L2BError()
    {
        
    }

    public L2BError(ErrorType _errorType, string _clientEndPoint)
    {
        packetType = PacketType.L2BError;
        errorType = _errorType;
        clientEndPoint = _clientEndPoint;
    }
}

public class B2CError : IPacketBase
{
    public ErrorType errorType { get; set; }

    public B2CError()
    {
        
    }

    public B2CError(ErrorType _errorType)
    {
        packetType = PacketType.B2CError;
        errorType = _errorType;
    }
}