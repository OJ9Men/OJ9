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

    private UdpClient udpClient;
    private bool loginSuccess = false;

    private void OnLoginButtonClicked()
    {
        ReqLogin();
    }

    private void ReqLogin()
    {
        udpClient = new UdpClient();

        byte[] sendBuff =
            OJ9Function.ObjectToByteArray(new C2LLogin(idText.text, pwText.text));
        IPEndPoint endPoint = OJ9Function.CreateIPEndPoint("127.0.0.1:" + OJ9Const.SERVER_PORT_NUM);
        udpClient.Send(sendBuff, sendBuff.Length, endPoint);

        StartListen();
    }

    private void StartListen()
    {

        try
        {
            udpClient.BeginReceive(DataReceived, null);
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
        var buffer = udpClient.EndReceive(asyncResult, ref groupEndPoint);
        var packBase = OJ9Function.ByteArrayToObject<IPacketBase>(buffer);
        switch (packBase.packetType)
        {
            case PacketType.Login:
            {
                L2CLogin packet = OJ9Function.ByteArrayToObject<L2CLogin>(buffer);
                if (packet.guid == Guid.Empty)
                {
                    // TODO : pw is wrong, show failed message
                    Debug.LogError("Login Failed : pw is wrong");
                }
                else
                {
                    Debug.Log("[" + packet.guid + "]");
                    loginSuccess = true;
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