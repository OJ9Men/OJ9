static class Program
{
    static void Main(string[] _args)
    {
        Console.WriteLine("Starting Game Server");
        Console.Title = "GameServer";
        
        SoccerServer soccerServer = new SoccerServer();
        soccerServer.Start();

        while (true)
        {
            Thread.Sleep(1000);
        }
    }
}