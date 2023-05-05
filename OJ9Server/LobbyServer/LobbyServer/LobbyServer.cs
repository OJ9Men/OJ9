﻿using System.Collections.Concurrent;
using System.Configuration;
using System.Net;
using System.Net.Sockets;
using MySql.Data.MySqlClient;

public class LobbyServer
{
    private static int INVALID_INDEX = -1;
    private UdpClient udpClient;
    private MySqlConnection mysql;
    private readonly ConcurrentQueue<ConnectionInfo>[] waitingPlayers = new ConcurrentQueue<ConnectionInfo>[(int)GameType.Max];
    private int roomNumber;
    private readonly ConcurrentBag<UserInfo> userInfos = new ConcurrentBag<UserInfo>();

    private readonly object lockObject = new object();

    public void Start()
    {
        try
        {
            roomNumber = 0;
            userInfos.Clear();
            
            for (var i = 0; i < (int)GameType.Max; i++)
            {
                waitingPlayers[i] = new ConcurrentQueue<ConnectionInfo>();
            }

            StartDB();
            udpClient = new UdpClient(
                Convert.ToInt32(
                    ConfigurationManager.AppSettings.Get("lobbyServerPort")
                )
            );
            
            Console.WriteLine("Listening started");
            udpClient.BeginReceive(DataReceived, null);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private void StartDB()
    {
        string dbServerString = string.Format(
            "Server=localhost;" +
            "Port={0};" +
            "Database={1};" +
            "Uid={2};" +
            "Pwd={3}",
            ConfigurationManager.AppSettings.Get("dbPort"),
            ConfigurationManager.AppSettings.Get("lobbyDatabase"),
            ConfigurationManager.AppSettings.Get("dbUserId"),
            ConfigurationManager.AppSettings.Get("dbUserPw")
        );

        mysql = new MySqlConnection(dbServerString);
        mysql.Open();
        
        Console.WriteLine("DB started");
    }

    private void DataReceived(IAsyncResult _asyncResult)
    {
        IPEndPoint ipEndPoint = null;
        var buffer = udpClient.EndReceive(_asyncResult, ref ipEndPoint);
        Console.WriteLine("Get from[login server] : " + ipEndPoint);
        var packBase = OJ9Function.ByteArrayToObject<PacketBase>(buffer);
        switch (packBase.packetType)
        {
            case PacketType.L2BError:
            {
                L2BError packet = OJ9Function.ByteArrayToObject<L2BError>(buffer);

                byte[] clientBuff =
                    OJ9Function.ObjectToByteArray(new B2CError(packet.errorType));
                udpClient.Send(clientBuff, clientBuff.Length, OJ9Function.CreateIPEndPoint(packet.clientEndPoint));
            }
                break;
            case PacketType.CheckLobbyAccount:
            {
                L2BCheckAccount packet = OJ9Function.ByteArrayToObject<L2BCheckAccount>(buffer);
                if (!packet.IsLoginSuccess())
                {
                    byte[] sendBuff =
                        OJ9Function.ObjectToByteArray(new B2CError(ErrorType.Unknown));
                    udpClient.Send(sendBuff, sendBuff.Length, OJ9Function.CreateIPEndPoint(packet.clientEndPoint));
                }
                try
                {
                    UserInfo userInfo = GetAccount(packet.guid);
                    if (!userInfo.IsValid())
                    {
                        userInfo = AddAccountDb(packet.guid);
                        Console.WriteLine("New account was added : " + userInfo.guid + ", Client ip : " + packet.clientEndPoint);
                    }
                    else
                    {
                        Console.WriteLine("Already exist account. Pass creating new account.");
                    }

                    if (!userInfo.IsValid())
                    {
                        throw new FormatException("Userinfo does not exist and cannot create");
                    }

                    userInfos.Add(userInfo);
                    EnterLobby(userInfo, OJ9Function.CreateIPEndPoint(packet.clientEndPoint));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
                break;
            case PacketType.QueueGame:
            {
                C2BQueueGame packet = OJ9Function.ByteArrayToObject<C2BQueueGame>(buffer);
                if (ipEndPoint == null)
                {
                    throw new FormatException("ipEndPoint is not valid");
                }
                waitingPlayers[(int)packet.gameType].Enqueue(
                    new ConnectionInfo(packet.userInfo, ipEndPoint)
                );
                Console.WriteLine(packet.userInfo.nickname + " is now in queue.");
            }
                break;
            case PacketType.CancelQueue:
            {
                C2BCancelQueue packet = OJ9Function.ByteArrayToObject<C2BCancelQueue>(buffer);
                if (ipEndPoint == null)
                {
                    throw new FormatException("ipEndPoint is not valid");
                }

                var players = waitingPlayers[(int)packet.gameType];
                foreach (var iter in players)
                {
                    if (iter.userInfo.guid == packet.userInfo.guid)
                    {
                        // TODO : Remove from concurrent queue
                    }
                }
            }
                break;
            default:
                throw new FormatException("Invalid packet type in LoginServer");
        }

        udpClient.BeginReceive(DataReceived, null);
    }

    private UserInfo GetAccount(Guid _guid)
    {
        MySqlCommand sqlCommand = new MySqlCommand(
            string.Format("SELECT * FROM user WHERE guid = '{0}';", _guid),
            mysql
        );
        sqlCommand.ExecuteNonQuery();
        var reader = sqlCommand.ExecuteReader();

        UserInfo userInfo = new UserInfo();
        while (reader.Read())
        {
            userInfo.guid = _guid;
            userInfo.nickname = reader["nickname"].ToString()!;

            var rating = reader["rating"].ToString();
            userInfo.rating = string.IsNullOrEmpty(rating) ? 0 : Convert.ToInt32(rating);

            var charType = reader["charType"].ToString();
            userInfo.charType = string.IsNullOrEmpty(charType) ? 0 : Convert.ToInt32(charType);
            break;  // it's unique
        }
        reader.Close();

        return userInfo;
    }

    private UserInfo AddAccountDb(Guid _guid)
    {
        var rand = new Random();
        string dummyUserName = "플레이어" + rand.Next();
        MySqlCommand sqlCommand = new MySqlCommand(
            string.Format("INSERT INTO user (guid, nickname, charType) VALUES ('{0}', '{1}', {2})", _guid, dummyUserName, OJ9Const.INVALID_CHAR_TYPE),
            mysql);
        
        if (sqlCommand.ExecuteNonQuery() != 1)
        {
            throw new FormatException("insert data failed");
        }

        return new UserInfo(_guid, dummyUserName, 0, OJ9Const.INVALID_CHAR_TYPE);
    }

    private void EnterLobby(UserInfo _userInfo, IPEndPoint _ipEndPoint)
    {
        var sendBuff =
            OJ9Function.ObjectToByteArray(new B2CEnterLobby(_userInfo));
        udpClient.Send(sendBuff, sendBuff.Length, _ipEndPoint);
    }

    public void Update()
    {
        SpinQueue();
    }

    private void SpinQueue()
    {
        var gameIndex = INVALID_INDEX;
        for (var index = 0; index < waitingPlayers.Length; index++)
        {
            if (waitingPlayers[index].Count < 2)
            {
                continue;
            }
            
            gameIndex = index;
            break;
        }

        if (gameIndex == INVALID_INDEX) // No game which is ready to go
        {
            return;
        }

        if (!waitingPlayers[gameIndex].TryDequeue(out var first) || !waitingPlayers[gameIndex].TryDequeue(out var second))
        {
            throw new FormatException("dequeue failed");
        }
        
        // Get 2 players

        lock (lockObject)
        {
            byte[] buffer =
                OJ9Function.ObjectToByteArray(
                    new B2CGameMatched((GameType)gameIndex, roomNumber)
                );

            udpClient.Send(
                buffer,
                buffer.Length,
                first.ipEndPoint
            );
            
            udpClient.Send(
                buffer,
                buffer.Length,
                second.ipEndPoint
            );
            
            ++roomNumber;

            if (roomNumber == int.MaxValue)
            {
                roomNumber = 0;
            }
        }
    }
}