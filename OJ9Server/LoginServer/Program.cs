public class Program
{
    static void Main(string[] Args)
    {
        Console.WriteLine("Starting Login Server...");
        Console.Title = "LoginServer";

        LoginServer loginServer = new LoginServer();
        loginServer.Start();

        while (true)
        {
            Thread.Sleep(10);
        }
    }
}