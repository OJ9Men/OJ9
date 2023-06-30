using System;
using System.Net;
using System.Timers;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoginManager : MonoBehaviour
{
    private enum LoginState
    {
        None,
        Try,
        Success,
        Fail
    }
    
    [SerializeField] private TMP_InputField idText;
    [SerializeField] private TMP_InputField pwText;
    [SerializeField] private GameObject connectingWidget;

    private Timer loginTryTimer;
    private LoginState loginState = LoginState.None;

    private void OnLoginButtonClicked()
    {
        if (loginState != LoginState.None)
        {
            throw new FormatException("login state is not 'None'");
        }

        loginState = LoginState.Try;
        GameManager.Get().ReqLogin(idText.text, pwText.text, OnLogin);
    }

    private void Update()
    {
        if (loginState != LoginState.Success)
        {
            if (loginState == LoginState.Fail)
            {
                throw new FormatException("Login Failed.");
            }
            return;
        }

        SceneManager.LoadScene("LobbyScene");
    }

    private void OnLogin(byte[] _packet)
    {
        var packet = OJ9Function.ByteArrayToObject<S2CLogin>(_packet);
        if (!packet.isSuccess)
        {
            loginState = LoginState.Fail;
            return;
        }
        
        GameManager.Get().userInfo = packet.userInfo;
        loginState = LoginState.Success;
    }
}