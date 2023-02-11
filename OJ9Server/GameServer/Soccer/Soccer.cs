using System.Net.Sockets;

public class Soccer : GameServer
{
    private Socket[] clients = new Socket[OJ9Const.MAX_GAME_ROOM_NUM];
    public Soccer()
    {
        gameType = GameType.Soccer;
    }

    public override void Start()
    {
        // TODO 
        // 1. Set socket
        // 2. receive data from client( Client -> Lobby -> Client -> Game )
    }
}