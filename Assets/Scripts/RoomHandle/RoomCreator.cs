using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using TMPro;

public class RoomCreator : MonoBehaviourPunCallbacks
{
    [SerializeField] private Button createRoomButton;
    [SerializeField] private TMP_Text error;

    private void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    private void Start()
    {
        createRoomButton.interactable = false;

        if (!PhotonNetwork.IsConnected)
            PhotonNetwork.ConnectUsingSettings();
        else
            createRoomButton.interactable = true; 

        createRoomButton.onClick.AddListener(CreateRoom);
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Photon (RoomCreator).");
        createRoomButton.interactable = true; 
    }

    private void CreateRoom()
    {
        int avatarIndex = PlayerPrefs.GetInt("avatarIndex", 0);
        ExitGames.Client.Photon.Hashtable customProps = new ExitGames.Client.Photon.Hashtable
        {
            { "avatarIndex", avatarIndex }
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(customProps);

        string code = GenerateRoomCode(6);
        PhotonNetwork.NickName = string.IsNullOrEmpty(PlayerPrefs.GetString("name"))
            ? GenerateRandomName()
            : PlayerPrefs.GetString("name");

        RoomOptions options = new RoomOptions { MaxPlayers = 5 };
        PhotonNetwork.CreateRoom(code, options);
    }

    public override void OnJoinedRoom()
    {
        UIManager.Instance.SwitchState(GameState.Lobby);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError("Room Creation Failed: " + message);
        if (error != null)
        {
            error.text = message;
            error.color = Color.red;
        }
    }

    private string GenerateRoomCode(int length)
    {
        const string chars = "0123456789";
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        for (int i = 0; i < length; i++)
        {
            sb.Append(chars[Random.Range(0, chars.Length)]);
        }
        return sb.ToString();
    }

    private string GenerateRandomName()
    {
        return "Player" + Random.Range(1000, 9999);
    }

    public void onJoinClick()
    {
        UIManager.Instance.SwitchState(GameState.JoinRoom);
    }
}
