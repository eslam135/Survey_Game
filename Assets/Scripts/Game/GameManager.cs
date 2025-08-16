using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public enum GamePhase { WaitingForPlayers, Voting, Question, Answering, Ranking }

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance { get; private set; }

    public GamePhase CurrentState { get; private set; } = GamePhase.WaitingForPlayers;
    public string LastWinnerCategory { get; private set; }
    public int CurrentRound { get; private set; } = 1;
    public bool IsMostCommonRound { get; private set; } = true;

    [SerializeField] private VotingManager votingManager;
    [SerializeField] private QuestionPhaseManager questionPhaseManager;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (photonView == null)
            Debug.LogError("[GameManager] Missing PhotonView on GameManager object!");
    }

    void Start()
    {
        if (PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient && CurrentState == GamePhase.WaitingForPlayers)
        {
            Debug.Log("[GameManager] Start(): already in room, kicking off Voting.");
            ChangeState(GamePhase.Voting);
        }
    }

    public override void OnJoinedRoom()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("[GameManager] OnJoinedRoom(): master starting Voting.");
            ChangeState(GamePhase.Voting);
        }
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (PhotonNetwork.IsMasterClient && CurrentState == GamePhase.WaitingForPlayers)
        {
            Debug.Log("[GameManager] Master switched; starting Voting.");
            ChangeState(GamePhase.Voting);
        }
    }

    public void ChangeState(GamePhase newState, string winnerCategory = "")
    {
        if (!string.IsNullOrEmpty(winnerCategory))
            LastWinnerCategory = winnerCategory;

        CurrentState = newState;
        Debug.Log($"[GameManager] Changing state → {CurrentState} | Round {CurrentRound} | Most? {IsMostCommonRound}");
        photonView.RPC("RPC_OnStateChanged", RpcTarget.All, (int)newState, LastWinnerCategory, IsMostCommonRound, CurrentRound);
    }

    [PunRPC]
    void RPC_OnStateChanged(int newStateInt, string winnerCategory, bool mostRoundFlag, int roundNum)
    {
        CurrentState = (GamePhase)newStateInt;
        LastWinnerCategory = winnerCategory;
        IsMostCommonRound = mostRoundFlag;
        CurrentRound = roundNum;

        Debug.Log($"[GameManager] State changed → {CurrentState}");

        switch (CurrentState)
        {
            case GamePhase.Voting:
                votingManager?.StartVotingPhase();
                break;
            case GamePhase.Question:
                questionPhaseManager?.StartQuestionPhase(winnerCategory, mostRoundFlag);
                break;
            case GamePhase.Answering:
                questionPhaseManager?.StartAnsweringPhase();
                break;
            case GamePhase.Ranking:
                questionPhaseManager?.ShowRankingPhase();
                break;
        }
    }

    public void NextRound()
    {
        if (CurrentRound < 4)
        {
            CurrentRound++;
            IsMostCommonRound = CurrentRound <= 2;
            Debug.Log(LastWinnerCategory);
            ChangeState(GamePhase.Question, LastWinnerCategory);
        }
        else
        {
            PrintFinalScores();
            UIManager.Instance.OnBackButtonPress();
        }
    }

    private void PrintFinalScores()
    {
        Debug.Log("=== Final Scores ===");
        foreach (var kvp in questionPhaseManager.GetTotalScores())
        {
            string playerName = PhotonNetwork.CurrentRoom.GetPlayer(kvp.Key)?.NickName ?? $"Player {kvp.Key}";
            Debug.Log($"{playerName}: {kvp.Value} pts");
        }
    }
}
