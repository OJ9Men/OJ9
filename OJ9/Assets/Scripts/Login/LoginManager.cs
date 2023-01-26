using System;
using System.Net;
using System.Net.Sockets;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoginManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField idText;

    [SerializeField] private TMP_InputField pwText;

    private bool loginSuccess = false;

    private void OnLoginButtonClicked()
    {
        ReqLogin();
    }

    private void ReqLogin()
    {
        byte[] sendBuff =
            OJ9Function.ObjectToByteArray(new C2LLogin(idText.text, pwText.text));
        IPEndPoint endPoint = OJ9Function.CreateIPEndPoint("127.0.0.1:" + OJ9Const.LOGIN_SERVER_PORT_NUM);
        GameManager.instance.udpClient.Send(sendBuff, sendBuff.Length, endPoint);
        
        StartListen();
    }

    private void StartListen()
    {
        try
        {
            GameManager.instance.udpClient.BeginReceive(DataReceived, null);
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
        var buffer = GameManager.instance.udpClient.EndReceive(asyncResult, ref groupEndPoint);
        var packBase = OJ9Function.ByteArrayToObject<IPacketBase>(buffer);
        switch (packBase.packetType)
        {
            case PacketType.EnterLobby:
            {
                B2CEnterLobby packet = OJ9Function.ByteArrayToObject<B2CEnterLobby>(buffer);
                Debug.Log("[" + packet.userInfo.nickname + "] : Login Success");
                GameManager.instance.SetUserInfo(packet.userInfo);
                
                loginSuccess = true;
            }
                break;
            case PacketType.B2CError:
            {
                B2CError packet = OJ9Function.ByteArrayToObject<B2CError>(buffer);
                if (packet.errorType == ErrorType.WrongPassword)
                {
                    Debug.LogError("Login failed : wrong password");
                }
            }
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void Update()
    {
        if (!loginSuccess)
        {
            return;
        }

        SceneManager.LoadScene("LobbyScene");
    }
}