using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Text;

public class SoccerServer : GameServer
{
    private enum Turn
    {
        A,
        B
    }
    private struct Room
    {
        public Client clientA, clientB;
        public int elapsedTime = 0;

        public Room()
        {
            clientA = default;
            clientB = default;
            elapsedTime = 0;
        }

        public Room(Client _clientA, Client _clientB)
        {
            clientA = _clientA;
            clientB = _clientB;
        }

        public Client GetOtherClient(Guid _guid)
        {
            if (clientA.userInfo.guid != _guid)
            {
                if (clientB.userInfo.guid == _guid)
                {
                    return clientB;
                }
                else
                {
                    throw new FormatException("No User");
                }
            }

            if (clientB.userInfo.guid != _guid)
            {
                if (clientA.userInfo.guid == _guid)
                {
                    return clientA;
                }
                else
                {
                    throw new FormatException("No User");
                }
            }

            throw new FormatException("Cannot found client by entered _userInfo");
        }
        
        public void AddPlayer(Client _client)
        {
            if (clientA.userInfo.guid == _client.userInfo.guid)
            {
                                
            }
        }
    }

    private readonly Dictionary<int /* =roomNumber */, Room> rooms = new Dictionary<int, Room>();

    public SoccerServer()
    {
        gameType = GameType.Soccer;
    }

    public override void Start()
    {
        Console.WriteLine("Listening Starts");

        listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        listener.Bind(OJ9Function.CreateIPEndPoint("127.0.0.1" + ":" + OJ9Const.SOCCER_SERVER_PORT_NUM));
        listener.Listen();
        listener.BeginAccept(new AsyncCallback(AcceptReceiveCallback), null);
    }

    public void AcceptReceiveCallback(IAsyncResult _asyncResult)
    {
        Socket clientSocket = listener.EndAccept(out var buffer, _asyncResult);
        StateObject stateObject = new StateObject();
        stateObject.socket = clientSocket;
        clientSocket.BeginReceive(stateObject.buffer, 0, OJ9Const.BUFFER_SIZE, 0, new AsyncCallback(ClientDataReceived), stateObject);
        
        listener.BeginAccept(new AsyncCallback(AcceptReceiveCallback), null);
    }

    public void ClientDataReceived(IAsyncResult _asyncResult)
    {
        String content = String.Empty;
        StateObject stateObject = (StateObject)_asyncResult.AsyncState;
        Socket socket = stateObject.socket;
        var byteRead = socket.EndReceive(_asyncResult);
        if (byteRead <= 0)
        {
            return;
        }
        
        stateObject.stringBuilder.Append(Encoding.UTF8.GetString(
            stateObject.buffer, 0, byteRead));
        content = stateObject.stringBuilder.ToString();
        if (content.Length != 0)
        {
            var buffer = Encoding.UTF8.GetBytes(content);
            var packetBase = OJ9Function.ByteArrayToObject<PacketBase>(buffer);
            switch (packetBase.packetType)
            {
                case PacketType.Ready:
                {
                    var packet = OJ9Function.ByteArrayToObject<C2GReady>(buffer);
                    var clinet = new Client(socket, packet.userInfo);
                    if (!rooms.TryGetValue(packet.roomNumber, out var found))
                    {
                        found = new Room();
                        found.AddPlayer(clinet);
                        rooms.Add(packet.roomNumber, found);

                        // Waiting for left player 
                        return;
                    }
                    found.AddPlayer(clinet);
                    
                    // Start
                    Random random = new Random();
                    var turn = random.Next(2) == 0 ? Turn.A : Turn.B;
                    var turnPacket = OJ9Function.ObjectToByteArray(new G2CStart(true));
                    var waitPacket = OJ9Function.ObjectToByteArray(new G2CStart(false));

                    switch (turn)
                    {
                        case Turn.A:
                        {
                            found.clientA.Send(turnPacket);
                            found.clientB.Send(waitPacket);
                        }
                            break;
                        case Turn.B:
                        {
                            found.clientA.Send(turnPacket);
                            found.clientB.Send(waitPacket);
                        }
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                    break;
                case PacketType.Shoot:
                {
                    var packet = OJ9Function.ByteArrayToObject<C2GShoot>(buffer);
                    if (!rooms.TryGetValue(packet.roomNumber, out var found))
                    {
                        throw new FormatException("There is no room, How could you send this packet?");
                    }

                    var newPacket = OJ9Function.ObjectToByteArray(new G2CShoot(packet.dir, packet.paddleId));
                    var otherUser = found.GetOtherClient(packet.userInfo.guid);
                    otherUser.Send(newPacket);
                }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        else
        {
            // Read more data
            socket.BeginReceive(
                stateObject.buffer,
                0,
                OJ9Const.BUFFER_SIZE,
                0,
                new AsyncCallback(ClientDataReceived),
                stateObject);
        }
    }
}