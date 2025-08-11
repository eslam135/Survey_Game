using Photon.Pun;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OnboardingHandler : MonoBehaviourPunCallbacks
{
    private int avatarIndex;
    private string playerName;

    [SerializeField] private TMP_InputField nameInput;
    [SerializeField] private TMP_Text error;


    private void Start()
    {
        avatarIndex = PlayerPrefs.GetInt("avatarIndex", -1);
        playerName = PlayerPrefs.GetString("name", "");

        //if (!string.IsNullOrEmpty(playerName) && avatarIndex != -1)
        //{
        //    SceneManager.LoadScene(1);
        //}
    }

    public void OnContinuePress()
    {
        playerName = nameInput.text.Trim();

        if (avatarIndex == -1)
        {
            Debug.LogWarning("Avatar not set.");
            error.text = "Avatar is not set";
            error.color = Color.red;
            return;
        }
        if (string.IsNullOrEmpty(playerName))
        {
            Debug.LogWarning("Name not set.");
            error.text = "Name is not set";
            error.color = Color.red;
        }

        PlayerPrefs.SetString("name", playerName);
        PlayerPrefs.SetInt("avatarIndex", avatarIndex);
        PlayerPrefs.Save();

        ExitGames.Client.Photon.Hashtable playerProps = new ExitGames.Client.Photon.Hashtable() { { "avatarIndex", avatarIndex } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProps);

        UIManager.Instance.SwitchState(GameState.MainMenu);
    }

    public void OnAvatarSelectionPress(int chosenIndex)
    {
        avatarIndex = chosenIndex;
    }
    public void OnInputFieldChange()
    {
        playerName = nameInput.text.Trim();
    }
}
