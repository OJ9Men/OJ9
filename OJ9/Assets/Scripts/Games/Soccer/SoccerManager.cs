using System;
using System.Net.Sockets;
using UnityEngine;

static class Constants
{
    public const float FORCE_MAGNITUDE = 1000.0f;
    public const int PLAYER_NUM = 5;
}

public class SoccerManager : MonoBehaviour
{
    enum GameState
    {
        None,
        WaitForReady,
        ShowReady,
        Start,
    }
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

    [Header("개발/온/오프라인 게임모드")]
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

    [Header("위젯")] 
    [SerializeField] private GameObject readyWidget;

    private GoalLineBoundary goalLineBoundary;
    private GameState gameState;

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

        gameState = GameState.None;
    }

    private void OnAimDone(Vector2 _vector2, int _paddleId)
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
            GameManager.instance.GetGameInfo().GetRoomNumber(),
            GameManager.instance.userInfo,
            new System.Numerics.Vector2(_vector2.x, _vector2.y),
            _paddleId
        );
        socket.Send(OJ9Function.ObjectToByteArray(packet));
    }

    private void ProcessState()
    {
        switch (gameState)
        {
            case GameState.WaitForReady:
            {
                readyWidget.SetActive(true);
                gameState = GameState.ShowReady;
            }
                break;
            case GameState.ShowReady:
            {
                // Do nothing
            }
                break;
            case GameState.Start:
            {
                readyWidget.SetActive(false);
            }
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void CheckGoalLine()
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
    void Update()
    {
        ProcessState();
        CheckGoalLine();
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
        socket.BeginReceive(buffer, 0, OJ9Const.BUFFER_SIZE, SocketFlags.None, OnDataReceived, null);

        gameState = GameState.WaitForReady;
    }

    public void OnReadyButtonClicked()
    {
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
    }

    private void OnDataReceived(IAsyncResult _asyncResult)
    {
        socket.EndReceive(_asyncResult);
        var packetBase = OJ9Function.ByteArrayToObject<PacketBase>(buffer);
        switch (packetBase.packetType)
        {
            case PacketType.Start:
            {
                gameState = GameState.Start;
                
                var packet = OJ9Function.ByteArrayToObject<G2CStart>(buffer);
                isMyTurn = packet.isMyTurn;
                SetPlayerJoystickEnabled(packet.isMyTurn);
                // TODO : show arrow, wait ui
            }
                break;
            case PacketType.Shoot:
            {
                var packet = OJ9Function.ByteArrayToObject<G2CShoot>(buffer);
                isMyTurn = true;
                var paddleId = packet.paddleId;
                if (enemyMovements.Length <= paddleId)
                {
                    throw new FormatException("Invalid paddle id");
                }
                enemyMovements[paddleId].Shoot(new Vector2(packet.dir.X, packet.dir.Y));
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