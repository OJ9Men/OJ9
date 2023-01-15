using System.Net;
using System.Net.Sockets;

class LoginServer
{
    private UdpClient listener;

    public void Start()
    {
        listener = new UdpClient(OJ9Const.SERVER_PORT_NUM);

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

    private void DataReceived(IAsyncResult _asyncResult)
    {
        IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.None, 0);
        var buffer = listener.EndReceive(_asyncResult, ref ipEndPoint);
        var packBase = OJ9Function.ByteArrayToObject<IPacketBase>(buffer);
        switch (packBase.packetType)
        {
            case PacketType.Login:
            {
                C2LLogin packet = OJ9Function.ByteArrayToObject<C2LLogin>(buffer);
                StartLogin(packet);
            }
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        listener.BeginReceive(DataReceived, null);
    }

    private void StartLogin(C2LLogin packet)
    {
        // TODO : Check id / pw
        Console.WriteLine("Id : " + packet.id + " pw : " + packet.pw);

        Socket socket = new Socket(
            AddressFamily.InterNetwork,
            SocketType.Dgram,
            ProtocolType.Udp
        );

        byte[] sendBuff = OJ9Function.ObjectToByteArray(new L2CLogin("1923759127378", "Hello World"));
        IPEndPoint endPoint = OJ9Function.CreateIPEndPoint(packet.ip + ":" + OJ9Const.CLIENT_PORT_NUM);
        socket.SendTo(sendBuff, endPoint);
    }
}