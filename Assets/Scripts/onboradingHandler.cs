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

        if (avatarIndex == -1 || string.IsNullOrEmpty(playerName))
        {
            Debug.LogWarning("Avatar or Name not set.");
            return;
        }

        PlayerPrefs.SetString("name", playerName);
        PlayerPrefs.SetInt("avatarIndex", avatarIndex);
        PlayerPrefs.Save();

        ExitGames.Client.Photon.Hashtable playerProps = new ExitGames.Client.Photon.Hashtable() { { "avatarIndex", avatarIndex } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProps);

        SceneManager.LoadScene(1);
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
