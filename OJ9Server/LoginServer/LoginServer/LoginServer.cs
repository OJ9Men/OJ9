using System.Net;
using System.Net.Sockets;
using System.Text;

class LoginServer
{
    private UdpClient listener;

    public void Start()
    {
        listener = new UdpClient(Constants.PORT_NUM);

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
        var packBase = Constants.ByteArrayToObject<IPacketBase>(buffer);
        switch (packBase.packetType)
        {
            case PacketType.Test:
            {
                C2LoginTest packet = Constants.ByteArrayToObject<C2LoginTest>(buffer);
                Console.WriteLine("[" + packet.port + "] : " + packet.str);
            } break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        // TODO 
        // Instead of receiving, Send some hand shake packet
        listener.BeginReceive(DataReceived, null);
    }
}