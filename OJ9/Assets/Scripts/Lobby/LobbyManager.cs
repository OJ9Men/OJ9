using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour
{
    [SerializeField] private GameObject gameHolder;

    [SerializeField] private TMP_Text gameName;

    private GameType selectedGameType;

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
                C2BEnterGame packet = new C2BEnterGame(
                    GameManager.instance.userInfo.guid,
                    GameType.Soccer
                );
                // TODO : Send packet by GameManager
            }
                break;
            case GameType.Dummy1:
            case GameType.Dummy2:
            case GameType.Dummy3:
            {
                Debug.Log("dummy game selectedS");
            }
                break;
            default:
            {
                throw new System.SystemException("Invalid GameType");
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        OnGameSelected((int)GameType.Soccer);
    }

    // Update is called once per frame
    void Update()
    {
    }
}