using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using TMPro;
using NUnit.Framework;
using System.Collections.Generic;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    public TMP_Text roomCodeText;
    public Button startGameButton;
    public Transform playersContainer;
    public GameObject playerNameEntryPrefab;
    public Sprite[] avatarSprites;
    private PhotonView view;

    void Awake()
    {
        view = GetComponent<PhotonView>();
        PhotonNetwork.AutomaticallySyncScene = true;
    }


    void Start()
    {
        roomCodeText.text = "Room Code: " + PhotonNetwork.CurrentRoom.Name;
        startGameButton.gameObject.SetActive(PhotonNetwork.IsMasterClient);
        startGameButton.onClick.AddListener(StartGame);
        int avatarIndex = PlayerPrefs.GetInt("avatarIndex", 0);

        if (!PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("ReceiveAvatarIndex", RpcTarget.MasterClient, PhotonNetwork.LocalPlayer.ActorNumber, avatarIndex);
        }
        RefreshPlayerList();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        RefreshPlayerList();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        RefreshPlayerList();
    }

    void RefreshPlayerList()
    {
        foreach (Transform child in playersContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (Player p in PhotonNetwork.PlayerList)
        {
            GameObject entry = Instantiate(playerNameEntryPrefab, playersContainer);

            TMP_Text nameText = entry.GetComponentInChildren<TMP_Text>();
            nameText.text = p.NickName;

            int avatarIndex = 0;
            if (p.CustomProperties.ContainsKey("avatarIndex"))
            {
                avatarIndex = (int)p.CustomProperties["avatarIndex"];
            }

            Debug.Log("Refresh " + avatarIndex);
            Image avatarImage = entry.GetComponentInChildren<Image>();
            if (avatarImage != null && avatarIndex < avatarSprites.Length)
            {
                avatarImage.sprite = avatarSprites[avatarIndex];
            }
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (changedProps.ContainsKey("avatarIndex"))
        {
            RefreshPlayerList();
        }
    }

    [PunRPC]
    void ReceiveAvatarIndex(int senderActorNumber, int avatarIndex)
    {
        Player player = PhotonNetwork.CurrentRoom.GetPlayer(senderActorNumber);
        if (player != null)
        {
            ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable
        {
            { "avatarIndex", avatarIndex }
        };
            player.SetCustomProperties(props); 
        }
    }

    void StartGame()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC(nameof(RpcSwitchState), RpcTarget.All, (int)GameState.Game);
        }
    }

    [PunRPC]
    void RpcSwitchState(int stateIndex)
    {
        UIManager.Instance.SwitchState((GameState)stateIndex);
    }

}
public class PlayerData
{
    public int actornum {  get; set; }
    public int avaterIndex { get; set; }    

    public PlayerData(int actornum, int avaterIndex) 
    { 
        this.actornum = actornum;
        this.avaterIndex = avaterIndex;
    }
}