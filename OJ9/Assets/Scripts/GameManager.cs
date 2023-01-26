using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Singleton
    public static GameManager instance;
    
    public UdpClient udpClient;
    public UserInfo userInfo;
        
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
}
