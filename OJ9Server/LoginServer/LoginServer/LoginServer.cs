using System.Net;
using System.Net.Sockets;

class LoginServer
{
    private UdpClient udpClient;

    public void Start()
    {
        udpClient = new UdpClient(OJ9Const.SERVER_PORT_NUM);

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

    private void DataReceived(IAsyncResult _asyncResult)
    {
        IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.None, 0);
        var buffer = udpClient.EndReceive(_asyncResult, ref ipEndPoint);
        var packBase = OJ9Function.ByteArrayToObject<IPacketBase>(buffer);
        switch (packBase.packetType)
        {
            case PacketType.Login:
            {
                C2LLogin packet = OJ9Function.ByteArrayToObject<C2LLogin>(buffer);
                StartLogin(packet, ipEndPoint);
            }
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        udpClient.BeginReceive(DataReceived, null);
    }

    private void StartLogin(C2LLogin packet, IPEndPoint ipEndPoint)
    {
        // TODO : Check id / pw
        Console.WriteLine("Id : " + packet.id + " pw : " + packet.pw);

        byte[] sendBuff = OJ9Function.ObjectToByteArray(new L2CLogin("1923759127378", "Hello World"));
        udpClient.Send(sendBuff, ipEndPoint);
    }
}