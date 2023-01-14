using System.Net;
using System.Net.Sockets;

class LoginServer
{
    private UdpClient listener;

    public void Start()
    {
        listener = new UdpClient(OJ9Const.PORT_NUM);

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
        IPEndPoint groupEndPoint = null;
        var buffer = listener.EndReceive(_asyncResult, ref groupEndPoint);
        var packBase = OJ9Function.ByteArrayToObject<IPacketBase>(buffer);
        switch (packBase.packetType)
        {
            case PacketType.Login:
            {
                C2LLogin packet = OJ9Function.ByteArrayToObject<C2LLogin>(buffer);
                Console.WriteLine("Id : " + packet.id + " pw : " + packet.pw);
            }
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        // TODO 
        // Instead of receiving, Send some hand shake packet
        listener.BeginReceive(DataReceived, null);
    }
}