using System.Collections.Concurrent;

static class Program
{
    static void Main(string[] _args)
    {
        Console.WriteLine("Starting server...");
        Console.Title = "Server";
        
        Server server = new Server();
        server.Start();

        while (true)
        {
            Thread.Sleep(1000);
        }
    }
}

public class Server
{
    // Managers
    private LoginManager loginManager;
    private LobbyManager lobbyManager;
    
    // Games
    private Soccer soccer;
    
    // User
    private ConcurrentBag<Client> clients;

    public Server()
    {
        loginManager = new LoginManager();
        lobbyManager = new LobbyManager();
        soccer = new Soccer();

        clients = new ConcurrentBag<Client>();
    }
    
    public void Start()
    {
        loginManager.Start();
        lobbyManager.Start();
        soccer.Start();
    }
}
