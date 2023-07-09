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

    private readonly ConcurrentQueue<Action> actions = new ConcurrentQueue<Action>();

    public void OnGameSelected(int gameIndex /* Not using now */)
    {
        // TODO : Not implemented
    }
    
    public void OnWaitingWidgetClicked()
    {
        waitingWidget.SetActive(false);
    }
    
    public void OnStartButtonClicked()
    {
        GameManager.Get().ReqStart(OnSoccerGameStart);
    }

    private void Start()
    {
        OnGameSelected(0);
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