using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.Serialization;

static class Constants
{
    public const float FORCE_MAGNITUDE = 1000.0f;
    public const int PLAYER_NUM = 5;
}

public class SoccerManager : MonoBehaviour
{
    struct GoalLineBoundary
    {
        public float Up, Down;

        public GoalLineBoundary(float up, float down)
        {
            Up = up;
            Down = down;
        }
    }

    enum GameMode
    {
        Dev,
        Release,
        ClientOnly,
    }

    [Header("No server 모드")]
    [SerializeField] private GameMode gameMode;

    [Header("플레이어 포지션")]
    [SerializeField] private Transform playerPosHolder;
    [SerializeField] private Transform enemyPosHolder;
    
    [Header("플레이어")]
    [SerializeField] private Transform playerHolder;
    [SerializeField] private Transform enemyHolder;

    private Transform[] playerInitPos;
    private Transform[] enemyInitPos;
    private PlayerMovement[] playerMovements;
    private PlayerMovement[] enemyMovements;

    [Header("골 라인")] [SerializeField] private Transform goalLineHolder;
    [SerializeField] private Transform ballInitPos;
    [SerializeField] private Transform ball;

    private GoalLineBoundary goalLineBoundary;

    // Network
    private Socket socket;
    private byte[] buffer;
    private bool isMyTurn;

    void Start()
    {
        playerInitPos = new Transform[Constants.PLAYER_NUM];
        enemyInitPos = new Transform[Constants.PLAYER_NUM];
        if (playerPosHolder.childCount != enemyPosHolder.childCount || playerPosHolder.childCount != Constants.PLAYER_NUM)
        {
            throw new FormatException("pos holder is not enough");
        }
        
        for (var i = 0; i < Constants.PLAYER_NUM; ++i)
        {
            playerInitPos[i] = playerPosHolder.GetChild(i);
            enemyInitPos[i] = enemyPosHolder.GetChild(i);
        }
        
        playerMovements = new PlayerMovement[Constants.PLAYER_NUM];
        enemyMovements = new PlayerMovement[Constants.PLAYER_NUM];
        for (var i = 0; i < playerHolder.childCount; ++i)
        {
            var iter = playerHolder.GetChild(i);
            playerMovements[i] = iter.GetComponent<PlayerMovement>();
            playerMovements[i].aimDoneDelegate = OnAimDone;
        }

        for (var i = 0; i < enemyHolder.childCount; ++i)
        {
            var iter = enemyHolder.GetChild(i);
            enemyMovements[i] = iter.GetComponent<PlayerMovement>();
        }

        goalLineBoundary = new GoalLineBoundary(
            goalLineHolder.GetChild(0).position.y,
            goalLineHolder.GetChild(1).position.y
        );

        buffer = new byte[OJ9Const.BUFFER_SIZE];

        switch (gameMode)
        {
            case GameMode.Dev:
            case GameMode.Release:
            {
                ConnectGameServer();
            }
                break;
            case GameMode.ClientOnly:
            {
                isMyTurn = true;
            }
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void OnAimDone(Vector2 _vector2, int _playerId)
    {
        if (!isMyTurn)
        {
            throw new FormatException("Not in turn, Aim must not be available");
        }

        Debug.Log(("Aim done : " + _vector2));

        if (socket == null) // When client only mode
        {
            return;
        }
        
        var packet = new C2GShoot(
            new System.Numerics.Vector2(_vector2.x, _vector2.y), _playerId
        );
        socket.Send(OJ9Function.ObjectToByteArray(packet));
    }

    void Update()
    {
        float puckY = ball.transform.position.y;
        if (goalLineBoundary.Down < puckY && puckY < goalLineBoundary.Up)
        {
            return;
        }

        if (goalLineBoundary.Down >= puckY)
        {
            Debug.Log("Blue win");
        }
        else if (goalLineBoundary.Up <= puckY)
        {
            Debug.Log("Blue win");
        }

        ResetPositions();
    }

    private void ResetPositions()
    {
        // Reset Puck
        Rigidbody2D puckRb = ball.GetComponent<Rigidbody2D>();
        puckRb.velocity = Vector2.zero;
        puckRb.angularVelocity = 0.0f;
        ball.transform.position = ballInitPos.position;

        // Reset Player/Ememy
        for (var i = 0; i < playerHolder.childCount; ++i)
        {
            var iter = playerHolder.GetChild(i);
            var playerRb = iter.GetComponent<Rigidbody2D>();
            playerRb.velocity = Vector2.zero;
            playerRb.angularVelocity = 0.0f;
            iter.position = playerInitPos[i].position;
        }
        for (var i = 0; i < enemyHolder.childCount; ++i)
        {
            var iter = enemyHolder.GetChild(i);
            var enemyRb = iter.GetComponent<Rigidbody2D>();
            enemyRb.velocity = Vector2.zero;
            enemyRb.angularVelocity = 0.0f;
            iter.position = enemyInitPos[i].position;
        }
    }

    private void ConnectGameServer()
    {
        if (socket != null)
        {
            socket.Close();
            socket = null;
        }

        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        var endPoint = OJ9Function.CreateIPEndPoint(
            OJ9Const.SERVER_IP + ":" + Convert.ToString(OJ9Const.SOCCER_SERVER_PORT_NUM)
        );
        socket.BeginConnect(endPoint, OnConnectResponse, null);
    }

    private void OnConnectResponse(IAsyncResult _asyncResult)
    {
        try
        {
            socket.EndConnect(_asyncResult);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        Debug.Log("Server connected");

        switch (gameMode)
        {
            case GameMode.Dev:
            {
                // Nothing to do
            }
                break;
            case GameMode.Release:
            {
                var gameInfo = GameManager.instance.GetGameInfo();
                var packet = new C2GReady(
                    gameInfo.GetGameType(),
                    gameInfo.GetRoomNumber(),
                    GameManager.instance.userInfo
                );
                socket.Send(OJ9Function.ObjectToByteArray(packet));
            }
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        socket.BeginReceive(buffer, 0, OJ9Const.BUFFER_SIZE, SocketFlags.None, OnDataReceived, null);
    }

    private void OnDataReceived(IAsyncResult _asyncResult)
    {
        var packetSize = socket.EndReceive(_asyncResult);
        var packetBase = OJ9Function.ByteArrayToObject<IPacketBase>(buffer, packetSize);
        switch (packetBase.packetType)
        {
            case PacketType.Start:
            {
                var packet = OJ9Function.ByteArrayToObject<G2CStart>(buffer, packetSize);
                isMyTurn = packet.isMyTurn;
                SetPlayerJoystickEnabled(packet.isMyTurn);
                // TODO : show arrow, wait ui
            }
                break;
            case PacketType.Shoot:
            {
            }
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void SetPlayerJoystickEnabled(bool _enabled)
    {
        foreach (var iter in playerMovements)
        {
            iter.SetEnableJoystick(_enabled);
        }
    }
}