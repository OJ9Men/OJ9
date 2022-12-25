// See https://aka.ms/new-console-template for more information

static class Program
{
    private static void Main(string[] _args)
    {
        Console.Title = "DummyClient";
        Console.WriteLine("Start DummyClient...");

        DummyClient client = new DummyClient();
        client.StartListening();

        while (true)
        {
            var input = Console.ReadLine();
            client.SendString(input);
        }
    }
}