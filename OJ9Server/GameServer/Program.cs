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
