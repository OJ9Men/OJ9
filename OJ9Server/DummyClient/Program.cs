// See https://aka.ms/new-console-template for more information

static class Program
{
    private static void Main(string[] _args)
    {
        Console.Title = "DummyClient";
        Console.WriteLine("Start DummyClient...");

        DummyClient.DummyClient client = new DummyClient.DummyClient();
        client.Connect();

        while (!client.isConnected)
        {
            
        }
        
        Console.WriteLine("Server connected!");
        byte[] msg = new byte[8];
        client.Send(msg);
        
        Console.ReadKey();
    }
}