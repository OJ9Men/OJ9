using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

static class Constants
{
    public const float FORCE_MAGNITUDE = 1000.0f;
    public const int RACKET_NUM = 5;
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

    [Header("No server 모드")] [SerializeField]
    private bool clientOnly;
    
    [Header("플레이어")]
    [SerializeField]
    private Transform[] playerInitPos;
    [SerializeField]
    private Transform player;

    [Header("골 라인")]
    [SerializeField]
    private Transform goalLineHolder;
    [SerializeField]
    private Transform puckInitPos;
    [SerializeField]
    private Transform puck;

    private GoalLineBoundary goalLineBoundary;
    
    // Network
    private Socket socket;
    private byte[] buffer;

    void Start()
    {
        goalLineBoundary = new GoalLineBoundary(
            goalLineHolder.GetChild(0).position.y,
            goalLineHolder.GetChild(1).position.y
        );

        buffer = new byte[OJ9Const.BUFFER_SIZE];
        if (!clientOnly)
        {
            ConnectGameServer();
        }
    }

    void Update()
    {
        float puckY = puck.transform.position.y;
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
        Rigidbody2D puckRb = puck.GetComponent<Rigidbody2D>();
        puckRb.velocity = Vector2.zero;
        puckRb.angularVelocity = 0.0f;
        puck.transform.position = puckInitPos.position;

        // Reset Player
        Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
        playerRb.velocity = Vector2.zero;
        playerRb.angularVelocity = 0.0f;
        player.transform.position = playerInitPos[0].position;
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
            OJ9Const.SERVER_IP + ":" + Convert.ToString(OJ9Const.GAME_SERVER_PORT_NUM)
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
        
        var gameInfo = GameManager.instance.GetGameInfo();
        var packet = new C2GReady(
            gameInfo.GetGameType(),
            gameInfo.GetRoomNumber(),
            GameManager.instance.userInfo
        );
        socket.Send(OJ9Function.ObjectToByteArray(packet));
        socket.BeginReceive(buffer, 0, OJ9Const.BUFFER_SIZE, SocketFlags.None, OnDataReceived, null);
        
        // TODO : Show wait ui
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
                if (packet.isMyTurn)
                {
                    SetJoystickEnabled(true);
                    // Show arrow
                }
                else
                {
                    SetJoystickEnabled(false);
                    // Show wait ui
                    // block input
                }
            }
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
    }

    private void SetJoystickEnabled(bool _enabled)
    {
        var movement = player.GetComponent<PlayerMovement>();
        movement.SetEnableJoystick(_enabled);
    }
}
