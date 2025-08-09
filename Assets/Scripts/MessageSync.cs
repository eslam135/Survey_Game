using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using TMPro;

public class MessageSync : MonoBehaviourPun
{
    public TMP_InputField messageInputField;
    public Button sendButton;
    public TMP_Text receivedMessageText;

    private void Start()
    {
        sendButton.onClick.AddListener(SendMessageToOtherPlayer);
    }

    void SendMessageToOtherPlayer()
    {
        if (photonView == null || messageInputField == null)
        {
            Debug.LogError("UI or PhotonView is not assigned!");
            return;
        }

        string msg = messageInputField.text.Trim();
        if (!string.IsNullOrEmpty(msg))
        {
            photonView.RPC("ReceiveMessage", RpcTarget.Others, msg);
        }

        messageInputField.text = "";
    }


    [PunRPC]
    void ReceiveMessage(string msg, PhotonMessageInfo info)
    {
        receivedMessageText.text = $"{info.Sender.NickName}: {msg}";
    }

}
