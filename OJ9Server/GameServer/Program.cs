static class Program
{
    static void Main(string[] _args)
    {
        Console.WriteLine("Starting Game Server");
        Console.Title = "GameServer";
        
        Soccer soccer = new Soccer();
        soccer.Start();

        while (true)
        {
            Thread.Sleep(1000);
        }
    }
}