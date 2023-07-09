using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.Assertions;

public readonly struct GameInfo
{
    private readonly int roomNumber;

    public GameInfo(int _roomNumber)
    {
        roomNumber = _roomNumber;
    }

    public int GetRoomNumber()
    {
        return roomNumber;
    }
}

public class GameManager : MonoBehaviour
{
    public static GameManager Get()
    {
        if (instance is null)
        {
            Debug.LogAssertion("No instance");
            return null;
        }
        
        return instance;
    }
    
    private static GameManager instance;
    private NetworkManager networkManager;
    
    public UserInfo userInfo;
    private GameInfo gameInfo;

    void Start()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(instance);

        networkManager = new NetworkManager(BlockUI);
    }

    private void OnDestroy()
    {
        networkManager.Disconnect();
    }

    private void BlockUI(bool _isBlock)
    {
        // TODO : Block ui till receive packet.
    }

    public void ReqLogin(string _id, string _pw, Action<byte[]> _action)
    {
        var packet = new C2SLogin(_id, _pw);
        networkManager.SendAndBindHandler(packet, _action);
    }

    public void ReqStart(Action<byte[]> _action)
    {
        var packet = new C2SStartGame(userInfo.guid);
        networkManager.SendAndBindHandler(packet, _action);
    }
}
