using System.Net;
using System.Net.Sockets;

class LoginServer
{
    private Socket listeningSocket;
    private List<Socket> connectedClients = new List<Socket>();
    public bool isConnected = false;
    private Thread disconnectDetectorThread;

    private void DisconnectDetector()
    {
        while (true)
        {
            foreach (var iter in connectedClients.ToList())
            {
                if (iter == null)
                {
                    continue;
                }
                
                if (iter.Poll(1, SelectMode.SelectRead) && iter.Available == 0)
                {
                    Console.WriteLine(iter.RemoteEndPoint + " is disconnected");
                    connectedClients.Remove(iter);
                }
            }
        }
    }

    public void Start()
    {
        Init();
        
        disconnectDetectorThread = new Thread(DisconnectDetector);
        disconnectDetectorThread.Start();
        
        try
        {
            listeningSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Any, Constants.PORT_NUM);
            listeningSocket.Bind(serverEndPoint);
            listeningSocket.Listen(10);  // connection queue
            listeningSocket.BeginAccept(AcceptCallback, null);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public void Close()
    {
        if (listeningSocket != null)
        {
            listeningSocket.Close();
            listeningSocket.Dispose();
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
            Socket client = listeningSocket.EndAccept(asyncResult);
            Console.WriteLine("Client accepted : " + client.RemoteEndPoint);
            AsyncObject obj = new AsyncObject(1920 * 1080 * 3);
            obj.workingSocket = client;
            connectedClients.Add(client);
            client.BeginReceive(obj.buffer, 0, 1920 * 1080 * 3, 0, DataReceived, obj);

            listeningSocket.BeginAccept(AcceptCallback, null);
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
        listeningSocket = null;
        connectedClients.Clear();
        
        Console.WriteLine("////////////////////////");
        Console.WriteLine("LoginServer Init");
        Console.WriteLine("////////////////////////");
    }

    public void Send(byte[] msg)
    {
        connectedClients[0].Send(msg);
    }
}