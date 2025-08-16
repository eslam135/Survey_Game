using AdvancedInputFieldPlugin;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ArabicSupport; // Added for ArabicFixer

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

    // Lock behavior after submit
    private bool hasSubmittedThisRound = false;

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
        // Re-enable input each round
        if (answerInput != null)
        {
            answerInput.enabled = true; // make sure component is active for typing
            answerInput.SetText("");
        }
        hasSubmittedThisRound = false;

        answerInput.gameObject.SetActive(true);
        submitAnswerButton.gameObject.SetActive(true);
        submitAnswerButton.interactable = true;

        answerTime = 30f;
        isAnsweringPhase = true;
    }

    public void SubmitAnswer()
    {
        if (isAnsweringPhase && !hasSubmittedThisRound && !string.IsNullOrWhiteSpace(answerInput.GetText()))
        {
            string answer = answerInput.GetText().Trim();
            photonView.RPC("RPC_ReceiveAnswer", RpcTarget.MasterClient, PhotonNetwork.LocalPlayer.ActorNumber, answer);

            hasSubmittedThisRound = true;
            submitAnswerButton.interactable = false;

            // Clear and hide input immediately after sending
            if (answerInput != null)
            {
                answerInput.SetText("");
                answerInput.enabled = false; // block further interaction
                answerInput.gameObject.SetActive(false); // hide completely
            }
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

        // Build results payload (structured to avoid bidi issues)
        var names = new List<string>();
        var answersList = new List<string>();
        var freqs = new List<int>();
        var totals = new List<int>();
        foreach (var entry in playerResults)
        {
            string playerName = PhotonNetwork.CurrentRoom.GetPlayer(entry.playerId)?.NickName ?? $"Player {entry.playerId}";
            names.Add(playerName);
            answersList.Add(entry.answer);
            freqs.Add(entry.frequency);
            totals.Add(totalScores[entry.playerId]);
        }

        photonView.RPC("RPC_ShowRankings", RpcTarget.All, names.ToArray(), answersList.ToArray(), freqs.ToArray(), totals.ToArray());

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

    // Helper for Arabic detection
    static bool ContainsArabicChars(string s)
    {
        if (string.IsNullOrEmpty(s)) return false;
        foreach (char c in s)
        {
            if ((c >= '\u0600' && c <= '\u06FF') || (c >= '\u0750' && c <= '\u077F') || (c >= '\u08A0' && c <= '\u08FF'))
                return true;
        }
        return false;
    }

    [PunRPC]
    void RPC_ShowRankings(string[] names, string[] answersArr, int[] freqs, int[] totals)
    {
        // Ensure the auto-localizer treats this TMP as dynamic content
        var disp = rankingText != null ? rankingText.GetComponent<DisplayLocalizedText>() : null;
        if (disp != null) disp.treatAsDynamic = true;

        bool isArabic = ArabicEnglishManager.Instance != null && ArabicEnglishManager.Instance.CurrentLanguage == ArabicEnglishManager.Language.Arabic;
        if (isArabic)
        {
            const char LRM = '\u200E'; // Left-to-right mark

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("النتائج:");

            for (int i = 0; i < names.Length && i < answersArr.Length && i < freqs.Length && i < totals.Length; i++)
            {
                string name = names[i] ?? string.Empty;
                string ans = answersArr[i] ?? string.Empty;

                // Wrap purely non-Arabic segments with LRM to keep ordering
                if (!ContainsArabicChars(name)) name = string.Concat(LRM, name, LRM);
                if (!ContainsArabicChars(ans)) ans = string.Concat(LRM, ans, LRM);

                sb.AppendLine($"• الاسم: {name}");
                sb.AppendLine($"  الإجابة: {ans}");
                sb.AppendLine($"  التكرار: {freqs[i]}");
                sb.AppendLine($"  المجموع: {totals[i]} نقاط");
                sb.AppendLine();
            }

            string shaped = ArabicSupport.ArabicFixer.Fix(sb.ToString(), showTashkeel: false, useHinduNumbers: true);
            rankingText.alignment = TextAlignmentOptions.Right;
            rankingText.text = shaped;
        }
        else
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("Results:");
            for (int i = 0; i < names.Length && i < answersArr.Length && i < freqs.Length && i < totals.Length; i++)
            {
                sb.AppendLine($"• Name: {names[i]}");
                sb.AppendLine($"  Answer: {answersArr[i]}");
                sb.AppendLine($"  Frequency: {freqs[i]}");
                sb.AppendLine($"  Total: {totals[i]} pts");
                sb.AppendLine();
            }
            rankingText.alignment = TextAlignmentOptions.Left;
            rankingText.text = sb.ToString();
        }

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
