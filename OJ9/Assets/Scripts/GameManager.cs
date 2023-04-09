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
    
    public UdpClient udpClient;
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
}
