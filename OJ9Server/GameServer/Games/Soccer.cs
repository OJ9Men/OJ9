﻿using System.Net;
using System.Net.Sockets;
using System.Text;

public class Soccer
{
    private readonly Dictionary<int /* =roomNumber */, Room> rooms;

    public Soccer()
    {
        rooms = new Dictionary<int, Room>();
    }
    
    public void Start()
    {
        Console.WriteLine("Soccer start");

        var listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        listener.Bind(new IPEndPoint(IPAddress.Any, OJ9Const.SOCCER_SERVER_PORT_NUM));
        listener.Listen();
        listener.BeginAccept(OJ9Const.RECEIVE_SIZE, AcceptReceiveCallback, listener);
    }

    public void AcceptReceiveCallback(IAsyncResult _asyncResult)
    {
        var listener = (Socket)_asyncResult.AsyncState!;
        var clientSocket = listener.EndAccept(out var _, _asyncResult);
        
        StateObject stateObject = new StateObject();
        stateObject.socket = clientSocket;
        clientSocket.BeginReceive(
            stateObject.buffer,
            0,
            stateObject.buffer.Length,
            SocketFlags.None,
            ClientDataReceived,
            stateObject
        );

        listener.BeginAccept(OJ9Const.RECEIVE_SIZE, AcceptReceiveCallback, listener);
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
                            //found.clientA.Send(turnPacket);
                            //found.clientB.Send(waitPacket);
                        }
                            break;
                        case Turn.B:
                        {
                            //found.clientA.Send(turnPacket);
                            //found.clientB.Send(waitPacket);
                        }
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                    break;
                case PacketType.Shoot:
                {
                    var packet = OJ9Function.ByteArrayToObject<C2SShoot>(buffer);
                    if (!rooms.TryGetValue(packet.roomNumber, out var found))
                    {
                        throw new FormatException("There is no room, How could you send this packet?");
                    }

                    //var newPacket = OJ9Function.ObjectToByteArray(new S2CShoot(-packet.dir, packet.paddleId));
                    //var otherUser = found.GetOtherClient(packet.guid);
                    //otherUser.Send(newPacket);
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

public enum Turn
{
    A,
    B
}

public struct Room
{
    public Client first, second;
    public int elapsedTime = 0;

    public Room()
    {
        first = default;
        second = default;
        elapsedTime = 0;
    }

    public Room(Client _first, Client _second)
    {
        first = _first;
        second = _second;
    }

    public void AddPlayer(Client _client)
    {
        if (first.userInfo.guid == _client.userInfo.guid)
        {
            if (second.userInfo.guid == _client.userInfo.guid)
            {
                throw new FormatException("Same player entered");
            }

            second = _client;
        }

        if (second.userInfo.guid == _client.userInfo.guid)
        {
            if (first.userInfo.guid == _client.userInfo.guid)
            {
                throw new FormatException("Same player entered");
            }

            first = _client;
        }
    }
}
