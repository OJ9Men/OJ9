public class Program
{
    static void Main(string[] Args)
    {
        Console.WriteLine("Start Login Server...");
        Console.Title = "LoginServer";

        LoginServer loginServer = new LoginServer();
        loginServer.Start();

        while (true)
        {
            Thread.Sleep(10);
        }

        byte[] msg = new byte[8];
        loginServer.Send(msg);

        Console.ReadKey();
    }
}