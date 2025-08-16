using AdvancedInputFieldPlugin;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class CategoryData
{
    public string MostQuestion;
    public string LeastQuestion;
    public string arabicMostQuestion;
    public string arabicLeastQuestion;
    public int total_responses;
    public List<AnswerData> answers;
}

[System.Serializable]
public class AnswerData
{
    public string answer;
    public int frequency;
}

public class QuestionPhaseManager : MonoBehaviourPunCallbacks
{
    [Header("UI References")]
    [SerializeField] private TMP_Text questionText;
    [SerializeField] private AdvancedInputField answerInput;
    [SerializeField] private Button submitAnswerButton;
    [SerializeField] private TMP_Text rankingText;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private GameObject questionParent;
    [SerializeField] private GameObject votingParent;

    [Header("Category Files")]
    [SerializeField] private TextAsset[] categoryJsonFiles;

    private Dictionary<int, string> roundAnswers = new Dictionary<int, string>();
    private Dictionary<int, int> totalScores = new Dictionary<int, int>();

    private string currentCategory;
    private float answerTime;
    private bool isAnsweringPhase = false;
    private bool isMostCommonRound = true;

    public void StartQuestionPhase(string categoryName, bool mostCommonRound)
    {
        Debug.Log("QuestionPhaseStarted");
        votingParent.SetActive(false);
        questionParent.SetActive(true);

        currentCategory = categoryName;
        isMostCommonRound = mostCommonRound;
        roundAnswers.Clear();
        questionText.text = "";
        answerInput.SetText("");
        questionText.gameObject.SetActive(true);

        foreach (var file in categoryJsonFiles)
        {
            if (file.name == categoryName)
            {
                CategoryData data = JsonUtility.FromJson<CategoryData>(file.text);
                Debug.Log(mostCommonRound);
                questionText.text = mostCommonRound ? data.MostQuestion : data.LeastQuestion;
                break;
            }
        }
        
        rankingText.gameObject.SetActive(false);
        answerInput.gameObject.SetActive(false);
        submitAnswerButton.gameObject.SetActive(false);

        if (PhotonNetwork.IsMasterClient)
            GameManager.Instance.ChangeState(GamePhase.Answering);
    }

    public void StartAnsweringPhase()
    {
        answerInput.gameObject.SetActive(true);
        submitAnswerButton.gameObject.SetActive(true);
        submitAnswerButton.interactable = true;

        answerTime = 30f;
        isAnsweringPhase = true;
    }

    public void SubmitAnswer()
    {
        if (isAnsweringPhase && !string.IsNullOrWhiteSpace(answerInput.GetText()))
        {
            string answer = answerInput.GetText().Trim();
            photonView.RPC("RPC_ReceiveAnswer", RpcTarget.MasterClient, PhotonNetwork.LocalPlayer.ActorNumber, answer);
            submitAnswerButton.interactable = false;
        }
    }

    [PunRPC]
    void RPC_ReceiveAnswer(int playerId, string answer)
    {
        if (!roundAnswers.ContainsKey(playerId))
            roundAnswers[playerId] = answer;
    }

    void Update()
    {
        if (isAnsweringPhase)
        {
            answerTime -= Time.deltaTime;
            timerText.text = Mathf.CeilToInt(answerTime).ToString();

            if (answerTime <= 0)
            {
                isAnsweringPhase = false;
                if (PhotonNetwork.IsMasterClient)
                    GameManager.Instance.ChangeState(GamePhase.Ranking);
            }
        }
    }

    public void ShowRankingPhase()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        // Load frequency data for the current category
        Dictionary<string, int> answerScores = new Dictionary<string, int>();
        foreach (var file in categoryJsonFiles)
        {
            if (file.name == currentCategory)
            {
                CategoryData data = JsonUtility.FromJson<CategoryData>(file.text);
                foreach (var ans in data.answers)
                    answerScores[ans.answer] = ans.frequency;
                break;
            }
        }

        // Pair each player with their frequency
        List<(int playerId, string answer, int frequency)> playerResults = new List<(int, string, int)>();
        foreach (var kvp in roundAnswers)
        {
            int freq = answerScores.ContainsKey(kvp.Value) ? answerScores[kvp.Value] : 0;
            playerResults.Add((kvp.Key, kvp.Value, freq));
        }

        // Sort based on round type
        if (isMostCommonRound)
            playerResults.Sort((a, b) => b.frequency.CompareTo(a.frequency)); // descending
        else
            playerResults.Sort((a, b) => a.frequency.CompareTo(b.frequency)); // ascending

        // Assign scores halving each time
        int points = 1000;
        foreach (var entry in playerResults)
        {
            if (!totalScores.ContainsKey(entry.playerId))
                totalScores[entry.playerId] = 0;

            totalScores[entry.playerId] += points;

            string playerName = PhotonNetwork.CurrentRoom.GetPlayer(entry.playerId)?.NickName ?? $"Player {entry.playerId}";
            Debug.Log($"{playerName} answered {entry.answer} ({entry.frequency}) → +{points}, total: {totalScores[entry.playerId]}");

            points = Mathf.Max(points / 2, 1); // never drop below 1
        }

        // Build results message
        string rankings = "Results:\n";
        foreach (var entry in playerResults)
        {
            string playerName = PhotonNetwork.CurrentRoom.GetPlayer(entry.playerId)?.NickName ?? $"Player {entry.playerId}";
            rankings += $"{playerName}: {entry.answer} ({entry.frequency}) → Total: {totalScores[entry.playerId]} pts\n";
        }

        photonView.RPC("RPC_ShowRankings", RpcTarget.All, rankings);

        StartCoroutine(ProceedToNextRound());
    }

    private IEnumerator ProceedToNextRound()
    {
        yield return new WaitForSeconds(5f);

        if (PhotonNetwork.IsMasterClient)
        {
            GameManager.Instance.NextRound();
        }
    }

    [PunRPC]
    void RPC_ShowRankings(string rankings)
    {
        rankingText.text = rankings;
        rankingText.gameObject.SetActive(true);
        questionText.gameObject.SetActive(false);
        answerInput.gameObject.SetActive(false);
        submitAnswerButton.gameObject.SetActive(false);
    }

    public Dictionary<int, int> GetTotalScores()
    {
        return new Dictionary<int, int>(totalScores);
    }
}
