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
    
    // Widget
    [SerializeField] private GameObject gameSelectWidget;
    [SerializeField] private GameObject characterSelectWidget;
    [SerializeField] private GameObject waitingWidget;

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

    public void OnCharacterSelectCanceled()
    {
        SetCharacterSelectVisible(false);
    }

    public void OnCharacterSelectDone(int _charType)
    {
        SetCharacterSelectVisible(false);
        
        // TODO : Send character select packet
    }

    public void OnWaitingWidgetClicked()
    {
        waitingWidget.SetActive(false);
        
        C2BCancelQueue packet = new C2BCancelQueue(
            GameManager.instance.userInfo,
            GameType.Soccer
        );
        var buffer = OJ9Function.ObjectToByteArray(packet);
        GameManager.instance.Send(ServerType.Lobby, buffer);
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
                GameManager.instance.Send(ServerType.Lobby, buffer);
                waitingWidget.SetActive(true);
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
        GameManager.instance.BeginReceive(DataReceived, null);

        var doCharacterSelect = GameManager.instance.userInfo.charType == OJ9Const.INVALID_CHAR_TYPE;
        SetCharacterSelectVisible(doCharacterSelect);
    }

    private void SetCharacterSelectVisible(bool _visible)
    {
        characterSelectWidget.SetActive(_visible);
        gameSelectWidget.SetActive(!_visible);
    }
    
    private void DataReceived(IAsyncResult _asyncResult)
    {
        IPEndPoint ipEndPoint = null;
        var recvBuffer = GameManager.instance.EndReceive(_asyncResult, ref ipEndPoint);
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