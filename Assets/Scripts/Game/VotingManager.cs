using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class VotingManager : MonoBehaviourPunCallbacks
{
    [Header("UI References")]
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text result;
    [SerializeField] private TMP_Text[] categoryTexts;
    [SerializeField] private Button[] allButtons;
    [SerializeField] private GameObject questionParent;
    [SerializeField] private GameObject votingParent;


    [Header("Category Files")]
    [SerializeField] private TextAsset[] categoryJsonFiles;

    private string[] selectedCategories = new string[3];
    private Dictionary<string, int> votes = new Dictionary<string, int>();
    private float votingTime;
    private bool isVotingActive = false;

    public void StartVotingPhase()
    {
        result.text = "";
        Debug.Log("Started");
        if (PhotonNetwork.IsMasterClient)
            StartCoroutine(DelayedStartVoting());
        votingParent.SetActive(true);
        questionParent.SetActive(false);
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
        votes.Clear();

        for (int i = 0; i < categories.Length; i++)
        {
            categoryTexts[i].text = categories[i];
            votes[categories[i]] = 0;
        }

        foreach (Button button in allButtons)
            button.interactable = true;

        votingTime = 10f;
        isVotingActive = true;
    }

    public void VoteForCategory(int index)
    {
        if (isVotingActive)
        {
            foreach (Button button in allButtons)
                button.interactable = false;

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

    void Update()
    {
        if (isVotingActive)
        {
            votingTime -= Time.deltaTime;
            timerText.text = Mathf.CeilToInt(votingTime).ToString();

            if (votingTime <= 0)
                EndVoting();
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
        result.text = resultsSummary + "\nWinner: " + winner;

        if (PhotonNetwork.IsMasterClient)
            GameManager.Instance.ChangeState(GamePhase.Question, winner);
    }
}
