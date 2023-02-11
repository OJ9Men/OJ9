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

    void Start()
    {
        goalLineBoundary = new GoalLineBoundary(
            goalLineHolder.GetChild(0).position.y,
            goalLineHolder.GetChild(1).position.y
        );
        
        GameInfo gameInfo = GameManager.instance.GetGameInfo();
        C2GGameStart packet = new C2GGameStart(gameInfo.GetGameType(), gameInfo.GetRoomNumber());
        byte[] buffer = OJ9Function.ObjectToByteArray(packet);
        // TODO : Connect TCP!
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
}
