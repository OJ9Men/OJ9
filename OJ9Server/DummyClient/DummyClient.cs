using System.Net;
using System.Net.Sockets;

public class DummyClient
{
    private Socket socket;
    private UdpClient udpClient;

    private static int MY_PORT_NUM = 5002;

    public void StartListening()
    {
        udpClient = new UdpClient(OJ9Const.CLIENT_PORT_NUM);

        try
        {
            udpClient.BeginReceive(DataReceived, null);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    void DataReceived(IAsyncResult _result)
    {
        Console.WriteLine("Received");
    }

    public void SendString(string? _input)
    {
        if (_input == null)
        {
            return;
        }

        byte[] sendbuf = OJ9Function.ObjectToByteArray(new C2LLogin("dummy Id", "dummy pw"));
        IPEndPoint ep = IPEndPoint.Parse("127.0.0.1:" + OJ9Const.SERVER_PORT_NUM);

        udpClient.Send(sendbuf, ep);

        Console.WriteLine("Message sent to the broadcast address");
    }
}