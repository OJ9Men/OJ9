using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoginManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField idText;

    [SerializeField] private TMP_InputField pwText;

    public void OnLoginButtonClicked()
    {
        Debug.Log("ID : " + idText.text + " PW : " + pwText.text);
        SceneManager.LoadScene("LobbyScene");
    }
}
