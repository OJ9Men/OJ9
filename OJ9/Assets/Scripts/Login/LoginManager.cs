using System;
using System.Net;
using System.Timers;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class LoginManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField idText;
    [SerializeField] private TMP_InputField pwText;
    [SerializeField] private GameObject connectionFailedWidget;

    private Timer loginTryTimer;
    private NetState netState = NetState.None;

    private void OnLoginButtonClicked()
    {
        if (netState != NetState.Connected)
        {
            throw new FormatException("Network does not connect");
        }

        GameManager.Get().Request(new C2SLogin(idText.text, pwText.text), OnLogin);
    }

    private void Start()
    {
        GameManager.Get().BindNetStateChangedHandler(OnNetStateChanged);
    }

    private void OnDestroy()
    {
        GameManager.Get().BindNetStateChangedHandler(null);
    }

    private void Update()
    {
        switch (netState)
        {
            case NetState.Closed:
            {
                connectionFailedWidget.SetActive(true);
            }
                break;
            case NetState.LoginFailed:
            {
                connectionFailedWidget.SetActive(true);
            }
                break;
            case NetState.LoginSucceed:
            {
                SceneManager.LoadScene("LobbyScene");
            }
                break;
            default:
            {
                // Do nothing
            }
                break;
        }

    }

    private void OnLogin(byte[] _packet)
    {
        var packet = OJ9Function.ByteArrayToObject<S2CLogin>(_packet);
        if (!packet.isSuccess)
        {
            netState = NetState.LoginFailed;
            Debug.LogError("Login Failed");
            return;
        }
        
        GameManager.Get().userInfo = packet.userInfo;
        netState = NetState.LoginSucceed;
    }

    private void OnNetStateChanged(NetState _netState)
    {
        netState = _netState;
    }
}