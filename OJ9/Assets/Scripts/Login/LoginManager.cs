using System.Net.Sockets;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoginManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField idText;

    [SerializeField] private TMP_InputField pwText;

    private void OnLoginButtonClicked()
    {
        if (!TryLogin())
        {
            Debug.LogError("Login Failed");
            return;
        }
        
        SceneManager.LoadScene("LobbyScene");
    }

    public bool TryLogin()
    {
        Socket socket = new Socket(
            AddressFamily.InterNetwork,
            SocketType.Dgram,
            ProtocolType.Udp
        );

        byte[] sendBuff = OJ9Function.ObjectToByteArray(new C2LLogin(idText.text, pwText.text));
        System.Net.IPEndPoint endPoint = OJ9Function.CreateIPEndPoint("127.0.0.1:" + OJ9Const.PORT_NUM);
        socket.SendTo(sendBuff, endPoint);

        return false;
    }
}
