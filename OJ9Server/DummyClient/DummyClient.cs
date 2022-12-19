using System.Net;
using System.Net.Sockets;
using System.Text;

public class DummyClient
{
    private Socket socket;
    public bool isConnected = false; 

    public void Connect()
    {
        isConnected = false;
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        IPAddress serverAddr = IPAddress.Parse("127.0.0.1");
        IPEndPoint endPoint = new IPEndPoint(serverAddr, Constants.PORT_NUM);
        socket.BeginConnect(endPoint, new AsyncCallback(ConnectCallback), socket);
    }

    public void Close()
    {
        if (socket != null)
        {
            socket.Close();
            socket.Dispose();
        }
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

    void ConnectCallback(IAsyncResult _result)
    {
        try
        {
            Socket client = (Socket)_result.AsyncState;
            client.EndConnect(_result);
            Console.WriteLine("Server connected : " + client.RemoteEndPoint);
            
            AsyncObject obj = new AsyncObject(Constants.BUFFER_SIZE);
            obj.workingSocket = socket;
            socket.BeginReceive(obj.buffer, 0, obj.bufferSize, 0, DataReceived, obj);
            isConnected = true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Close();
            throw;
        }
    }

    void DataReceived(IAsyncResult _result)
    {
        AsyncObject obj = (AsyncObject)_result.AsyncState;
        int recived = obj.workingSocket.EndReceive(_result);
        byte[] buffer = new byte[recived];
        Array.Copy(obj.buffer, 0, buffer, 0, recived);

        string result = Encoding.UTF8.GetString(buffer);
        Console.WriteLine("[Server] : " + result);
    }

    private void Send(byte[] msg)
    {
        socket.Send(msg);
    }

    public void SendString(string? _input)
    {
        if (_input == null)
        {
            return;
        }

        byte[] msg = Encoding.UTF8.GetBytes(_input);
        Send(msg);
    }
}