using System.Net;
using System.Net.Sockets;

class LoginServer
{
    private Socket socket;
    private List<Socket> connectedClients = new List<Socket>();
    public bool isConnected = false;

    public void Start()
    {
        Init();
        
        try
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Any, Constants.PORT_NUM);
            socket.Bind(serverEndPoint);
            socket.Listen(10);  // connection queue
            socket.BeginAccept(AcceptCallback, null);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public void Close()
    {
        if (socket != null)
        {
            socket.Close();
            socket.Dispose();
        }

        foreach (var iter in connectedClients)
        {
            iter.Close();
            iter.Dispose();
        }
        
        connectedClients.Clear();
    }

    public class AsyncObject
    {
        public byte[] buffer;
        public Socket workingSocket;
        public readonly int bufferSize;

        public AsyncObject(int _bufferSize)
        {
            bufferSize = _bufferSize;
            buffer = new byte[(long)bufferSize];
        }

        public void ClearBuffer()
        {
            Array.Clear(buffer, 0, bufferSize);
        }
    }

    private void AcceptCallback(IAsyncResult asyncResult)
    {
        try
        {
            Socket client = socket.EndAccept(asyncResult);
            AsyncObject obj = new AsyncObject(1920 * 1080 * 3);
            obj.workingSocket = client;
            connectedClients.Add(client);
            client.BeginReceive(obj.buffer, 0, 1920 * 1080 * 3, 0, DataReceived, obj);

            socket.BeginAccept(AcceptCallback, null);
            isConnected = true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private void DataReceived(IAsyncResult asyncResult)
    {
        AsyncObject obj = (AsyncObject)asyncResult.AsyncState;
        int received = obj.workingSocket.EndReceive(asyncResult);
        byte[] buffer = new byte[received];
        Array.Copy(obj.buffer, 0, buffer, 0, received);
    }
    private void Init()
    {
        isConnected = false;
        socket = null;
        connectedClients.Clear();
        
        Console.WriteLine("////////////////////////");
        Console.WriteLine("LoginServer Init");
        Console.WriteLine("////////////////////////");
    }

    public void Send(byte[] msg)
    {
        socket.Send(msg);
    }
}