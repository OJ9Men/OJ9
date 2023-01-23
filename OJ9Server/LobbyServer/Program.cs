static class Program
{
    static void Main(string[] _args)
    {
        Console.WriteLine("Starting Lobby Server");
        Console.Title = "LobbyServer";

        LobbyServer lobbyServer = new LobbyServer();
        lobbyServer.Start();

        while (true)
        {
            Thread.Sleep(1000);
        }
    }
}