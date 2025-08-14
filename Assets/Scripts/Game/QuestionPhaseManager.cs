using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using AdvancedInputFieldPlugin;

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
    [SerializeField] private Button backButton;


    [Header("Category Files")]
    [SerializeField] private TextAsset[] categoryJsonFiles;

    private Dictionary<int, string> playerAnswers = new Dictionary<int, string>();
    private string currentCategory;
    private float answerTime;
    private bool isAnsweringPhase = false;

    public void StartQuestionPhase(string categoryName)
    {
        currentCategory = categoryName;
        playerAnswers.Clear();

        foreach (var file in categoryJsonFiles)
        {
            if (file.name == categoryName)
            {
                CategoryData data = JsonUtility.FromJson<CategoryData>(file.text);
                questionText.text = data.MostQuestion;
                break;
            }
        }

        questionText.gameObject.SetActive(true);
        rankingText.gameObject.SetActive(false);
        answerInput.SetText("");
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

            photonView.RPC("RPC_ReceiveAnswer", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber, answer);

            submitAnswerButton.interactable = false;
        }
    }

    [PunRPC]
    void RPC_ReceiveAnswer(int playerId, string answer)
    {
        if (!playerAnswers.ContainsKey(playerId))
            playerAnswers[playerId] = answer;
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

        string rankings = "Results:\n";
        foreach (var kvp in playerAnswers)
        {
            int score = answerScores.ContainsKey(kvp.Value) ? answerScores[kvp.Value] : 0;
            rankings += $"Player {kvp.Key}: {kvp.Value} ({score} pts)\n";
        }

        photonView.RPC("RPC_ShowRankings", RpcTarget.All, rankings);
    }

    [PunRPC]
    void RPC_ShowRankings(string rankings)
    {
        rankingText.text = rankings;
        rankingText.gameObject.SetActive(true);
        questionText.gameObject.SetActive(false);
        answerInput.gameObject.SetActive(false);
        submitAnswerButton.gameObject.SetActive(false);
        backButton.gameObject.SetActive(true);
    }
}
