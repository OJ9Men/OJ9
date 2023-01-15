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

    private UdpClient listener;
    private bool loginSuccess = false;

    private void OnLoginButtonClicked()
    {
        ReqLogin();
    }

    private void ReqLogin()
    {
        Socket socket = new Socket(
            AddressFamily.InterNetwork,
            SocketType.Dgram,
            ProtocolType.Udp
        );

        byte[] sendBuff =
            OJ9Function.ObjectToByteArray(new C2LLogin(idText.text, pwText.text, OJ9Function.GetLocalIpAddr()));
        IPEndPoint endPoint = OJ9Function.CreateIPEndPoint("127.0.0.1:" + OJ9Const.SERVER_PORT_NUM);
        socket.SendTo(sendBuff, endPoint);

        StartListen();
    }

    private void StartListen()
    {
        listener = new UdpClient(OJ9Const.CLIENT_PORT_NUM);

        try
        {
            listener.BeginReceive(DataReceived, null);
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
        var buffer = listener.EndReceive(asyncResult, ref groupEndPoint);
        var packBase = OJ9Function.ByteArrayToObject<IPacketBase>(buffer);
        switch (packBase.packetType)
        {
            case PacketType.Login:
            {
                L2CLogin packet = OJ9Function.ByteArrayToObject<L2CLogin>(buffer);
                Console.WriteLine("[" + packet.dbId + "] : " + packet.welcomeMsg);
                loginSuccess = true;
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