public class Program
{
    static void Main(string[] Args)
    {
        Console.WriteLine("Start Login Server...");
        Console.Title = "LoginServer";

        LoginServer loginServer = new LoginServer();
        loginServer.Start();

        while (loginServer.isConnected == false)
        {
        }

        byte[] msg = new byte[8];
        loginServer.Send(msg);

        Console.ReadKey();
    }
}