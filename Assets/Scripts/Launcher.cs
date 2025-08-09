using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using TMPro;

public class Launcher : MonoBehaviourPunCallbacks
{
    public TMP_InputField roomCodeInput;
    public Button createRoomButton;
    public Button joinRoomButton;


    private void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    private void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
        PhotonNetwork.AutomaticallySyncScene = true;

        createRoomButton.onClick.AddListener(CreateRoom);
        joinRoomButton.onClick.AddListener(JoinRoom);
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Photon.");
    }

    void CreateRoom()
    {
        int avatarIndex = PlayerPrefs.GetInt("avatarIndex", 0);
        Debug.Log("Create " +avatarIndex);
        ExitGames.Client.Photon.Hashtable customProps = new ExitGames.Client.Photon.Hashtable
        {
            {
                "avatarIndex", avatarIndex
            }
        };

        PhotonNetwork.LocalPlayer.SetCustomProperties(customProps);
        string code = GenerateRoomCode(6);
        PhotonNetwork.NickName = string.IsNullOrEmpty(PlayerPrefs.GetString("name")) ? GenerateRandomName() : PlayerPrefs.GetString("name");
        RoomOptions options = new RoomOptions { MaxPlayers = 5 };
        PhotonNetwork.CreateRoom(code, options);
    }

    void JoinRoom()
    {
        int avatarIndex = PlayerPrefs.GetInt("avatarIndex", 0);
        if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("avatarIndex"))
        {
            Debug.Log("Inside");
            avatarIndex = (int)PhotonNetwork.LocalPlayer.CustomProperties["avatarIndex"];
        }
        else
        {
            Debug.Log("No");
        }
            Debug.Log("Join " + avatarIndex);

        string code = roomCodeInput.text.Trim().ToUpper();
        PhotonNetwork.NickName = string.IsNullOrEmpty(PlayerPrefs.GetString("name")) ? GenerateRandomName() : PlayerPrefs.GetString("name");
        PhotonNetwork.JoinRoom(code);
    }

    public override void OnJoinedRoom()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Lobby");
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError("Join Room Failed: " + message);
    }

    string GenerateRoomCode(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        for (int i = 0; i < length; i++)
        {
            sb.Append(chars[Random.Range(0, chars.Length)]);
        }
        return sb.ToString();
    }

    string GenerateRandomName()
    {
        return "Player" + Random.Range(1000, 9999);
    }
}
