using System.Net;
using System.Net.Sockets;

public class DummyClient
{
    private Socket socket;
    private UdpClient listener;
    private Socket sender;

    private static int MY_PORT_NUM = 5002;

    public void StartListening()
    {
        listener = new UdpClient(OJ9Const.CLIENT_PORT_NUM);

        try
        {
            listener.BeginReceive(DataReceived, null);
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

        Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        byte[] sendbuf = OJ9Function.ObjectToByteArray(new C2LLogin("dummy Id", "dummy pw", OJ9Function.GetLocalIpAddr()));
        IPEndPoint ep = IPEndPoint.Parse("127.0.0.1:" + OJ9Const.SERVER_PORT_NUM);

        s.SendTo(sendbuf, ep);

        Console.WriteLine("Message sent to the broadcast address");
    }
}