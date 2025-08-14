using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using TMPro;
using AdvancedInputFieldPlugin;

public class RoomJoiner : MonoBehaviourPunCallbacks
{
    [SerializeField] private AdvancedInputField roomCodeInput;
    [SerializeField] private TMP_Text error;
    [SerializeField] private Button joinRoomButton;

    private void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    private void Start()
    {
        if (!PhotonNetwork.IsConnected)
            PhotonNetwork.ConnectUsingSettings();

        joinRoomButton.onClick.AddListener(JoinRoom);
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Photon (RoomJoiner).");
    }

    private void JoinRoom()
    {
        int avatarIndex = PlayerPrefs.GetInt("avatarIndex", 0);
        if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("avatarIndex"))
        {
            avatarIndex = (int)PhotonNetwork.LocalPlayer.CustomProperties["avatarIndex"];
        }

        string code = (roomCodeInput != null ? roomCodeInput.Text : string.Empty).Trim().ToUpper();
        PhotonNetwork.NickName = string.IsNullOrEmpty(PlayerPrefs.GetString("name"))
            ? GenerateRandomName()
            : PlayerPrefs.GetString("name");

        PhotonNetwork.JoinRoom(code);
    }

    public override void OnJoinedRoom()
    {
        UIManager.Instance.SwitchState(GameState.Lobby);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError("Join Room Failed: " + message);
        if (error != null)
        {
            error.text = message;
            error.color = Color.red;
        }
    }

    private string GenerateRandomName()
    {
        return "Player" + Random.Range(1000, 9999);
    }
}
