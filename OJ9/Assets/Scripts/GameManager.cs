using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public readonly struct GameInfo
{
    private readonly GameType gameType;
    private readonly int roomNumber;

    public GameInfo(GameType _gameType, int _roomNumber)
    {
        gameType = _gameType;
        roomNumber = _roomNumber;
    }

    public GameType GetGameType()
    {
        return gameType;
    }

    public int GetRoomNumber()
    {
        return roomNumber;
    }
}

public class GameManager : MonoBehaviour
{
    // Singleton
    public static GameManager instance;
    private UdpClient udpClient;
    public UserInfo userInfo;

    private GameInfo gameInfo;
        
    // Start is called before the first frame update
    void Start()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(instance);

        udpClient = new UdpClient();
    }

    public void SetUserInfo(UserInfo _userInfo)
    {
        userInfo = _userInfo;
    }

    public void SetGameInfo(GameInfo _gameInfo)
    {
        gameInfo = _gameInfo;
    }

    public GameInfo GetGameInfo()
    {
        return gameInfo;
    }

    public void Send(ServerType _serverType, byte[] _buffer)
    {
        int portNum = 0;
        switch (_serverType)
        {
            case ServerType.Login:
            {
                portNum = OJ9Const.LOGIN_SERVER_PORT_NUM;
            }
                break;
            case ServerType.Lobby:
            {
                portNum = OJ9Const.LOBBY_SERVER_PORT_NUM;
            }
                break;
            case ServerType.Soccer:
            {
                portNum = OJ9Const.SOCCER_SERVER_PORT_NUM;
            }
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(_serverType), _serverType, null);
        }
        
        udpClient.Send(_buffer, _buffer.Length,
            OJ9Function.CreateIPEndPoint(OJ9Const.SERVER_IP + ":" + portNum)
        );
    }

    public void BeginReceive(AsyncCallback _requestCallback, object _state)
    {
        udpClient.BeginReceive(_requestCallback, _state);
    }

    public byte[] EndReceive(IAsyncResult _asyncResult, ref IPEndPoint _remoteEp)
    {
        return udpClient.EndReceive(_asyncResult, ref _remoteEp);
    }
}
