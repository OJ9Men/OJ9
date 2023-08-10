using System;
using System.Net.Sockets;
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
    [SerializeField] private GameObject turnWidget;

    private GoalLineBoundary goalLineBoundary;
    
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
        
        turnWidget.SetActive(!GameManager.Get().GetGameInfo().isMyTurn);
    }

    private void OnAimDone(Vector2 _dir, int _paddleId)
    {
        if (!GameManager.Get().GetGameInfo().isMyTurn)
        {
            throw new FormatException("Not my turn");
        }
        Debug.Log(("Aim done : " + _dir));
        GameManager.Get().ReqShoot(_dir.x, _dir.y, _paddleId, OnShoot);
    }

    private void OnShoot(byte[] _buffer)
    {
        var packet = OJ9Function.ByteArrayToObject<S2CShoot>(_buffer);
        
        if (packet.guid == GameManager.Get().GetGameInfo().enemyInfo.guid)
        {
            enemyMovements[packet.paddleId].Shoot(new Vector2(packet.dir.X, packet.dir.Y));
            GameManager.Get().GetGameInfo().SetIsMyTurn(true);
            turnWidget.SetActive(false);
        }
        else if (packet.guid == GameManager.Get().userInfo.guid)
        {
            playerMovements[packet.paddleId].Shoot(new Vector2(packet.dir.X, packet.dir.Y));
            GameManager.Get().GetGameInfo().SetIsMyTurn(false);
            turnWidget.SetActive(true);
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
                //var gameInfo = GameManager.Get().GetGameInfo();
                //var packet = new C2GReady(
                //    gameInfo.GetGameType(),
                //    gameInfo.GetRoomNumber(),
                //    GameManager.Get().userInfo
                //);
                //socket.Send(OJ9Function.ObjectToByteArray(packet));
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