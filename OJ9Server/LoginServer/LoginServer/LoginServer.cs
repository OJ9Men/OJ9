using System.Net;
using System.Net.Sockets;

class LoginServer
{
    private Socket? listeningSocket;
    private readonly List<Socket> connectedClients = new List<Socket>();
    private Thread? disconnectDetectorThread;

    private void DisconnectDetector()
    {
        while (true)
        {
            foreach (var iter in connectedClients.ToList())
            {
                if (!(iter.Poll(1, SelectMode.SelectRead) && iter.Available == 0))
                {
                    continue;
                }
                
                Console.WriteLine(iter.RemoteEndPoint + " is disconnected");
                connectedClients.Remove(iter);
            }
        }
        // ReSharper disable once FunctionNeverReturns
    }

    public void Start()
    {
        Init();
        
        disconnectDetectorThread = new Thread(DisconnectDetector);
        disconnectDetectorThread.Start();
        
        try
        {
            listeningSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var serverEndPoint = new IPEndPoint(IPAddress.Any, Constants.PORT_NUM);
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
            workingSocket = null!;
        }

        public void ClearBuffer()
        {
            Array.Clear(buffer, 0, bufferSize);
        }
    }

    private void AcceptCallback(IAsyncResult _asyncResult)
    {
        try
        {
            if (listeningSocket == null)
            {
                Console.WriteLine("listening socket does not exist.");
                return;
            }
            
            var client = listeningSocket.EndAccept(_asyncResult);
            Console.WriteLine("Client accepted : " + client.RemoteEndPoint);
            var obj = new AsyncObject(1920 * 1080 * 3)
            {
                workingSocket = client
            };
            connectedClients.Add(client);
            client.BeginReceive(obj.buffer, 0, 1920 * 1080 * 3, 0, DataReceived, obj);

            listeningSocket.BeginAccept(AcceptCallback, null);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private void DataReceived(IAsyncResult _asyncResult)
    {
        var obj = (AsyncObject)_asyncResult.AsyncState!;
        var received = obj.workingSocket.EndReceive(_asyncResult);
        var buffer = new byte[received];
        Array.Copy(obj.buffer, 0, buffer, 0, received);
    }
    private void Init()
    {
        listeningSocket = null;
        connectedClients.Clear();
        
        Console.WriteLine("////////////////////////");
        Console.WriteLine("LoginServer Init");
        Console.WriteLine("////////////////////////");
    }

    public void Send(byte[] _msg)
    {
        connectedClients[0].Send(_msg);
    }
}