using System;
using System.Collections.Concurrent;
using System.Net;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour
{
    [SerializeField] private GameObject gameHolder;

    [SerializeField] private TMP_Text gameName;

    private GameType selectedGameType;
    private readonly ConcurrentQueue<Action> actions = new ConcurrentQueue<Action>();

    public void OnGameSelected(int gameIndex)
    {
        if (gameIndex >= (int)GameType.Max)
        {
            throw new System.SystemException("Invalid game type");
        }

        selectedGameType = (GameType)gameIndex;
        gameName.text = selectedGameType + " Game";
    }

    public void OnStartButtonClicked()
    {
        switch (selectedGameType)
        {
            case GameType.Soccer:
            {
                C2BQueueGame packet = new C2BQueueGame(
                    GameManager.instance.userInfo,
                    GameType.Soccer
                );
                byte[] buffer = OJ9Function.ObjectToByteArray(packet);
                GameManager.instance.udpClient.Send(buffer, buffer.Length,
                    OJ9Function.CreateIPEndPoint(OJ9Const.SERVER_IP + ":" + OJ9Const.LOBBY_SERVER_PORT_NUM)
                );
                
                // TODO : Show waiting ui
                Debug.Log("Now in queue");
            }
                break;
            default:
            {
                throw new System.SystemException("Invalid GameType");
            }
        }
    }

    // Start is called before the first frame update
    private void Start()
    {
        OnGameSelected((int)GameType.Soccer);
        GameManager.instance.udpClient.BeginReceive(DataReceived, null);
    }

    private void DataReceived(IAsyncResult asyncResult)
    {
        IPEndPoint ipEndPoint = null;
        var recvBuffer = GameManager.instance.udpClient.EndReceive(asyncResult, ref ipEndPoint);
        var packetBase = OJ9Function.ByteArrayToObject<PacketBase>(recvBuffer);
        switch (packetBase.packetType)
        {
            case PacketType.Matched:
            {
                var recvPacket = OJ9Function.ByteArrayToObject<B2CGameMatched>(recvBuffer);
                InitGame(recvPacket.gameType, recvPacket.roomNumber);
            }
                break;
            default:
            {
                throw new FormatException("cannot receive other packet in LobbyManager");
            }
        }
    }

    private void InitGame(GameType _gameType, int _roomNumber)
    {
        GameManager.instance.SetGameInfo(new GameInfo(_gameType, _roomNumber));
        
        actions.Enqueue(() =>   // for dispatch to main thread
        {
            SceneManager.LoadScene("SoccerScene");
        });
    }

    private void Update()
    {
        while (actions.Count > 0)
        {
            if (actions.TryDequeue(out var action))
            {
                action?.Invoke();
            }
        }
    }
}