using UnityEngine;
using Photon.Pun;

public enum GamePhase
{
    Voting,
    Question,
    Answering,
    Ranking
}

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance { get; private set; }

    public GamePhase CurrentState { get; private set; }
    public string LastWinnerCategory { get; private set; }

    [SerializeField] private VotingManager votingManager;

    [SerializeField] private QuestionPhaseManager questionPhaseManager;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    private void Start()
    {
        ChangeState(GamePhase.Voting);
    }
    public void ChangeState(GamePhase newState, string winnerCategory = "")
    {
        if (!PhotonNetwork.IsMasterClient)
            return; 

        if (!string.IsNullOrEmpty(winnerCategory))
            LastWinnerCategory = winnerCategory;

        Debug.Log($"[GameManager] Changing state to: {newState}");

        CurrentState = newState;
        photonView.RPC("RPC_OnStateChanged", RpcTarget.All, (int)newState, LastWinnerCategory);
    }

    [PunRPC]
    void RPC_OnStateChanged(int newStateInt, string winnerCategory)
    {
        CurrentState = (GamePhase)newStateInt;
        LastWinnerCategory = winnerCategory;

        Debug.Log($"[GameManager] State changed to: {CurrentState}, WinnerCategory: {winnerCategory}");

        switch (CurrentState)
        {
            case GamePhase.Voting:
                votingManager.gameObject.SetActive(true);
                questionPhaseManager.gameObject.SetActive(false);
                votingManager?.StartVotingPhase();
                break;

            case GamePhase.Question:
                votingManager.gameObject.SetActive(false);
                questionPhaseManager.gameObject.SetActive(true);
                questionPhaseManager?.StartQuestionPhase(winnerCategory);
                break;

            case GamePhase.Answering:
                questionPhaseManager?.StartAnsweringPhase();
                break;

            case GamePhase.Ranking:
                questionPhaseManager?.ShowRankingPhase();
                break;
        }
    }

}
