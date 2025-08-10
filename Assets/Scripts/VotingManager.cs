using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class VotingManager : MonoBehaviourPunCallbacks
{
    [Header("UI References")]
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text result;
    [SerializeField] private TMP_Text[] categoryTexts;
    [SerializeField] private Button[] allButtons;

    [Header("Category Files")]
    [SerializeField] private TextAsset[] categoryJsonFiles; 

    private string[] selectedCategories = new string[3];
    private Dictionary<string, int> votes = new Dictionary<string, int>();
    private float votingTime = 60f;
    private bool isVotingActive = false;
    void Start()
    {
        result.text = "";
        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(DelayedStartVoting());
        }
    }

    IEnumerator DelayedStartVoting()
    {
        yield return new WaitForSeconds(2f); 
        PickRandomCategories();
        photonView.RPC("RPC_SetCategories", RpcTarget.All, selectedCategories);
    }

    void PickRandomCategories()
    {
        List<TextAsset> tempList = new List<TextAsset>(categoryJsonFiles);

        for (int i = 0; i < 3 && tempList.Count > 0; i++)
        {
            int randIndex = Random.Range(0, tempList.Count);
            selectedCategories[i] = tempList[randIndex].name;
            tempList.RemoveAt(randIndex);
        }
    }

    [PunRPC]
    void RPC_SetCategories(string[] categories)
    {
        selectedCategories = categories;

        for (int i = 0; i < categories.Length; i++)
        {
            categoryTexts[i].text = categories[i];
            votes[categories[i]] = 0;
        }

        StartVoting();
    }

    public void VoteForCategory(int index)
    {
        if (isVotingActive)
        {
            foreach(Button button in allButtons)
            {
                button.interactable = false;
            }

            photonView.RPC("RPC_RegisterVote", RpcTarget.MasterClient, selectedCategories[index]);
        }
    }

    [PunRPC]
    void RPC_RegisterVote(string category)
    {
        if (!PhotonNetwork.IsMasterClient) return; 

        if (votes.ContainsKey(category))
            votes[category]++;
    }


    void StartVoting()
    {
        isVotingActive = true;
        votingTime = 30f;
    }

    void Update()
    {
        if (isVotingActive)
        {
            votingTime -= Time.deltaTime;
            timerText.text = Mathf.CeilToInt(votingTime).ToString();

            if (votingTime <= 0)
            {
                EndVoting();
            }
        }
    }

    void EndVoting()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        isVotingActive = false;

        string winner = null;
        int maxVotes = -1;
        string resultsSummary = "Results:\n";

        foreach (var kvp in votes)
        {
            resultsSummary += $"{kvp.Key}: {kvp.Value} votes\n";
            if (kvp.Value > maxVotes)
            {
                maxVotes = kvp.Value;
                winner = kvp.Key;
            }
        }

        photonView.RPC("RPC_AnnounceWinner", RpcTarget.All, resultsSummary, winner);
    }

    [PunRPC]
    void RPC_AnnounceWinner(string resultsSummary, string winner)
    {
        Debug.Log("Winner: " + winner);
        result.text = resultsSummary + "\nWinner: " + winner;
    }


}
