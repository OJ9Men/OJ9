using System.Net.Sockets;

public abstract class GameServer
{
    protected Socket socket;
    protected GameType gameType;
    
    public abstract void Start();
}