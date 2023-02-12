static class Program
{
    static void Main(string[] _args)
    {
        Console.WriteLine("Starting Game Server");
        Console.Title = "GameServer";
        
        Soccer soccer = new Soccer(OJ9Const.MAX_GAME_ROOM_NUM);
        soccer.Start();

        while (true)
        {
            Thread.Sleep(1000);
        }
    }
}