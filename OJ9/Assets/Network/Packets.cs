using System;

public enum PacketType
{
    Login,
    CheckLobbyAccount,
    EnterLobby,
    
    // Lobby
    QueueGame,
    Matched,
    CancelQueue,
    
    // Game
    Ready,
    
    // Soccer
    Start,
    Shoot,

    // Error
    L2BError,
    B2CError,
    
    Max,
}

public enum ErrorType
{
    None,
    WrongPassword,
    Unknown,
}

public class PacketBase
{
    public PacketType packetType { get; set; }
}

public class C2SLogin : PacketBase
{
    public string id { get; set; }
    public string pw { get; set; }

    public C2SLogin()
    {
    }

    public C2SLogin(string _id, string _pw)
    {
        packetType = PacketType.Login;
        id = _id;
        pw = _pw;
    }
}

public class S2CLogin : PacketBase
{
    public UserInfo userInfo { get; set; }
    public bool isSuccess { get; set; }
    public S2CLogin()
    {
    }

    public S2CLogin(UserInfo _userInfo)
    {
        packetType = PacketType.Login;
        userInfo = _userInfo;
        isSuccess = userInfo.IsValid();
    }
}

public class C2SStartGame : PacketBase
{
    public Guid guid { get; set; }
    public C2SStartGame()
    {
    }

    public C2SStartGame(Guid _guid)
    {
        packetType = PacketType.Start;
        guid = _guid;
    }
}

public class S2CStartGame : PacketBase
{
    public UserInfo enemy { get; set; }
    public bool isMyTurn { get; set; }

    public S2CStartGame()
    {
    }

    public S2CStartGame(UserInfo _enemy, bool _isMyTurn)
    {
        packetType = PacketType.Start;
        enemy = _enemy;
        isMyTurn = _isMyTurn;
    }
}

public class L2BCheckAccount : PacketBase
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

public class C2BQueueGame : PacketBase
{
    public UserInfo userInfo { get; set; }

    public C2BQueueGame()
    {
    }

    public C2BQueueGame(UserInfo _userInfo)
    {
        packetType = PacketType.QueueGame;
        userInfo = _userInfo;
    }
}

public class C2BCancelQueue : PacketBase
{
    public UserInfo userInfo { get; set; }

    public C2BCancelQueue()
    {
    }
    
    public C2BCancelQueue(UserInfo _userInfo)
    {
        packetType = PacketType.CancelQueue;
        userInfo = _userInfo;
    }
}

public class B2CGameMatched : PacketBase
{
    public int roomNumber { get; set; }
    public B2CGameMatched()
    {
    }

    public B2CGameMatched(int _roomNumber)
    {
        packetType = PacketType.Matched;
        roomNumber = _roomNumber;
    }
}
public class C2GReady : PacketBase
{
    // TCP Packet
    
    public int roomNumber { get; set; }
    public UserInfo userInfo { get; set; }

    public C2GReady()
    {
        
    }

    public C2GReady(int _roomNumber, UserInfo _userInfo)
    {
        packetType = PacketType.Ready;
        roomNumber = _roomNumber;
        userInfo = _userInfo;
    }
}

public class G2CStart : PacketBase
{
    public bool isMyTurn { get; set; }
    
    public G2CStart()
    {
        
    }
    
    public G2CStart(bool _isMyTurn)
    {
        packetType = PacketType.Start;
        isMyTurn = _isMyTurn;
    }
}

public class L2CLogin : PacketBase
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

public class B2CEnterLobby : PacketBase
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

public class L2BError : PacketBase
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

public class B2CError : PacketBase
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

public class C2SShoot : PacketBase
{
    public int roomNumber;
    public UserInfo userInfo;
    public System.Numerics.Vector2 dir;
    public int paddleId;

    public C2SShoot(int _roomNumber, UserInfo _userInfo, System.Numerics.Vector2 _dir, int _paddleId)
    {
        roomNumber = _roomNumber;
        userInfo = _userInfo;
        dir = _dir;
        paddleId = _paddleId;
        packetType = PacketType.Shoot;
    }
}

public class G2CShoot : PacketBase
{
    public System.Numerics.Vector2 dir;
    public int paddleId;
    
    // this packet would be absolutely other player
    public G2CShoot(System.Numerics.Vector2 _dir, int _paddleId)
    {
        dir = _dir;
        paddleId = _paddleId;
    }
}
