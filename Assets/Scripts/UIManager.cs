using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [SerializeField] private GameObject[] screens;
    [SerializeField] private static GameState currState = GameState.onBoarding;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        int avatarIndex = PlayerPrefs.GetInt("avatarIndex", -1);
        string playerName = PlayerPrefs.GetString("name", "");

        if (!string.IsNullOrEmpty(playerName) && avatarIndex != -1)
        {
            currState = GameState.MainMenu;
        }

        SwitchState(currState);
    } 

    public void SwitchState(GameState state)
    {
        for (int i = 0; i < screens.Length; i++)
        {
            if (screens[i] != null)
                screens[i].SetActive(false);
        }

        int index = (int)state;

        if (index >= 0 && index < screens.Length && screens[index] != null)
        {
            currState = state;
            screens[index].SetActive(true);
        }
        else
        {
            Debug.LogWarning($"UIManager: No screen assigned for state {state}");
        }
    }
    public static GameState getCurrState()
    {
        return currState;
    }
}

public enum GameState
{
    onBoarding,
    MainMenu,
    JoinRoom,
    Lobby,
    Game
}
