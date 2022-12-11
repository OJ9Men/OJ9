public class LoginServer
{
    static void Main(string[] Args)
    {
        Console.WriteLine("Start Login Server...");
        Console.Title = "LoginServer";
        
        Auth auth = new Auth();
        auth.Init();
        
        while (true)
        {
        }

        Console.ReadKey();
    }
}