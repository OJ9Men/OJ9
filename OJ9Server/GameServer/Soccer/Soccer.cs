using System.Net;
using System.Net.Sockets;
using System.Numerics;

public class Soccer : GameServer
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

        public void AddPlayer(Client _client)
        {
            if (!clientA.IsValid())
            {
                clientA = _client;
                return;
            }

            if (!clientB.IsValid())
            {
                clientB = _client;
                return;
            }

            throw new FormatException("Room is full");
        }
    }

    private Dictionary<int /* =roomNumber */, Room> rooms;

    public Soccer(int _inMaxRoomNumber) : base(_inMaxRoomNumber)
    {
        gameType = GameType.Soccer;
    }

    public override void Start()
    {
        Console.WriteLine("Listening Starts");

        listenEndPoint = OJ9Function.CreateIPEndPoint("127.0.0.1:" + OJ9Const.SOCCER_SERVER_PORT_NUM);
        listener = new Socket(
            listenEndPoint.AddressFamily,
            SocketType.Stream,
            ProtocolType.Tcp
        );

        listener.Bind(listenEndPoint);
        listener.Listen(maxRoomNumber);
        listener.BeginAccept(OnAccepted, null);

        rooms = new Dictionary<int, Room>();
    }

    private void OnAccepted(IAsyncResult _asyncResult)
    {
        Client client = new Client(listener.EndAccept(_asyncResult));
        client.BeginReceive(OnReceived);
        
        clients.Add(client);
        listener.BeginAccept(OnAccepted, null);
    }

    private void OnReceived(byte[] _buffer, ref Client _client)
    {
        IPacketBase packetBase = OJ9Function.ByteArrayToObject<IPacketBase>(_buffer);
        switch (packetBase.packetType)
        {
            case PacketType.Ready:
            {
                var packet = OJ9Function.ByteArrayToObject<C2GReady>(_buffer);
                _client.InitUserInfo(packet.userInfo);
                
                if (!rooms.TryGetValue(packet.roomNumber, out var found))
                {
                    found = new Room();
                    found.AddPlayer(_client);
                    rooms.Add(packet.roomNumber, found);

                    // Waiting for left player 
                    return;
                }
                found.AddPlayer(_client);
                
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
                // TODO : Process both clients
                var packet = OJ9Function.ByteArrayToObject<C2GShoot>(_buffer);
                
                
            }
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}