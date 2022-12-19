using System.Net;
using System.Net.Sockets;
using System.Text;

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
                if (IsConnected(iter))
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
        listeningSocket = null;
        connectedClients.Clear();
        disconnectDetectorThread = new Thread(DisconnectDetector);
        disconnectDetectorThread.Start();

        try
        {
            listeningSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var serverEndPoint = new IPEndPoint(IPAddress.Any, Constants.PORT_NUM);
            listeningSocket.Bind(serverEndPoint);
            listeningSocket.Listen(10); // connection queue
            listeningSocket.BeginAccept(AcceptCallback, null);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        
        Console.WriteLine("Done!");
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

            // Say Hi !
            string helloMessage = "Hi there! " + client.RemoteEndPoint;
            client.Send(Encoding.UTF8.GetBytes(helloMessage));

            var obj = new AsyncObject(Constants.BUFFER_SIZE)
            {
                workingSocket = client
            };
            connectedClients.Add(client);
            client.BeginReceive(obj.buffer, 0, Constants.BUFFER_SIZE, 0, DataReceived, obj);

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
        if (!IsConnected(obj.workingSocket))
        {
            return;
        }

        var received = obj.workingSocket.EndReceive(_asyncResult);
        var buffer = new byte[received];
        Array.Copy(obj.buffer, 0, buffer, 0, received);

        string receivedMessage = Encoding.UTF8.GetString(buffer);
        Console.WriteLine("[" + obj.workingSocket.RemoteEndPoint + "] : " + receivedMessage);
        
        obj.workingSocket.BeginReceive(obj.buffer, 0, Constants.BUFFER_SIZE, 0, DataReceived, obj);
    }

    private bool IsConnected(Socket _socket)
    {
        return !(_socket.Poll(1, SelectMode.SelectRead) && _socket.Available == 0); 
    }
}