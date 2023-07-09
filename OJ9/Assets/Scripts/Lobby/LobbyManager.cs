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
    
    public void OnWaitingWidgetClicked()
    {
        waitingWidget.SetActive(false);
    }
    
    public void OnStartButtonClicked()
    {
        switch (selectedGameType)
        {
            case GameType.Soccer:
            {
                GameManager.Get().ReqStart(GameType.Soccer, OnSoccerGameStart);
                
                // TODO : Show waiting widget ( Queue ~)
            }
                break;
            default:
            {
                throw new System.SystemException("Invalid GameType");
            }
        }
    }

    private void Start()
    {
        OnGameSelected((int)GameType.Soccer);
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

    private void OnSoccerGameStart(byte[] _buffer)
    {
        SceneManager.LoadScene("SoccerScene");
    }
}