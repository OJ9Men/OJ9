using System;
using System.Net;
using System.Net.Sockets;
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

    private Timer loginTryTimer;
    private LoginState loginState = LoginState.None;
    private int loginTryCount = 0;

    private void OnLoginButtonClicked()
    {
        if (loginState != LoginState.None)
        {
            throw new FormatException("login state is not 'None'");
        }

        StartListen();
        loginState = LoginState.Try;
        loginTryTimer = new Timer();
        loginTryTimer.Interval = 2000;  // 2 Sec
        loginTryTimer.Elapsed += new ElapsedEventHandler(TryLogin);
        loginTryTimer.Start();
    }

    private void TryLogin(object sender, ElapsedEventArgs e)
    {
        if (loginTryCount >= OJ9Const.LOGIN_TRY_COUNT)
        {
            loginTryTimer.Stop();
            loginState = LoginState.Fail;
            return;
        }
        
        var buffer = OJ9Function.ObjectToByteArray(new C2LLogin(idText.text, pwText.text));
        GameManager.instance.Send(ServerType.Login, buffer);
        ++loginTryCount;
    }

    private void StartListen()
    {
        try
        {
            GameManager.instance.BeginReceive(DataReceived, null);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private void DataReceived(IAsyncResult asyncResult)
    {
        IPEndPoint groupEndPoint = null;
        var buffer = GameManager.instance.EndReceive(asyncResult, ref groupEndPoint);
        var packBase = OJ9Function.ByteArrayToObject<PacketBase>(buffer);
        switch (packBase.packetType)
        {
            case PacketType.EnterLobby:
            {
                B2CEnterLobby packet = OJ9Function.ByteArrayToObject<B2CEnterLobby>(buffer);
                Debug.Log("[" + packet.userInfo.nickname + "] : Login Success");
                GameManager.instance.SetUserInfo(packet.userInfo);
                
                loginState = LoginState.Success;
                loginTryTimer.Stop();
            }
                break;
            case PacketType.B2CError:
            {
                B2CError packet = OJ9Function.ByteArrayToObject<B2CError>(buffer);
                if (packet.errorType == ErrorType.WrongPassword)
                {
                    Debug.LogError("Login failed : wrong password");
                }

                loginState = LoginState.Fail;
            }
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void Update()
    {
        if (loginState != LoginState.Success)
        {
            if (loginState == LoginState.Fail)
            {
                Debug.LogError("Login failed");
            }
            return;
        }

        SceneManager.LoadScene("LobbyScene");
    }
}