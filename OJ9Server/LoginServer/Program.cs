public class Program
{
    static void Main(string[] Args)
    {
        Console.WriteLine("Start Login Server...");
        Console.Title = "LoginServer";
        
        LoginServer loginServer = new LoginServer();
        loginServer.Init();
        
        while (true)
        {
        }

        Console.ReadKey();
    }
}