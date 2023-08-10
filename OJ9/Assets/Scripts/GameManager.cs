using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.Assertions;

public struct GameInfo
{
    public int roomNumber;
    public UserInfo enemyInfo;
    public bool isMyTurn;
    public GameInfo(int _roomNumber, UserInfo _enemyInfo, bool _isMyTurn)
    {
        roomNumber = _roomNumber;
        enemyInfo = _enemyInfo;
        isMyTurn = _isMyTurn;
    }

    public void Reset()
    {
        roomNumber = 0;
        enemyInfo = new UserInfo();
        isMyTurn = false;
    }

    public void SetIsMyTurn(bool _isMyTurn)
    {
        isMyTurn = _isMyTurn;
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
    private GameInfo? gameInfo = null;

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
    
    public void ReqShoot(float _x, float _y, int _paddleId, Action<byte[]> _action)
    {
        var packet = new C2SShoot(
            gameInfo.Value.roomNumber,
            userInfo.guid, 
            new System.Numerics.Vector2(_x, _y),
            _paddleId
        );
        
        networkManager.SendAndBindHandler(packet, _action);
    }

    public void BindNetStateChangedHandler(Action<NetState> _action)
    {
        networkManager.netStateChangedHandler = _action;
    }

    public void SetGameInfo(S2CStartGame _packet)
    {
        gameInfo = new GameInfo(_packet.roomNumber, _packet.enemy, _packet.isMyTurn);
    }

    public GameInfo GetGameInfo()
    {
        if (!gameInfo.HasValue)
        {
            throw new FormatException("GameInfo didn't set");
        }

        return gameInfo.Value;
    }

    public bool HasGameInfo()
    {
        return gameInfo.HasValue;
    }

    public void SetIsMyTurn(bool _isMyTurn)
    {
        gameInfo.Value.SetIsMyTurn(_isMyTurn);
    }
}
