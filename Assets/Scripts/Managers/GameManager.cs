using System.Collections.Generic;
using UnityEngine;
using CustomAttribute;
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Settings")]
    [ReadOnly] public GameStates currentGameState = GameStates.None;
    [ReadOnly] public GameDifficulty currentGameDifficulty = GameDifficulty.None;
    [ReadOnly] public GameOverState currentGameOverState = GameOverState.None;
    public GameDifficulty chosenGameDifficulty = GameDifficulty.None;

    [Header("Debug Buttons")]
    [SerializeField] private KeyCode quitGameKey = KeyCode.F1;
    [SerializeField] private KeyCode restartSceneKey = KeyCode.F2;


    [Header("Cursor Settings")]
    public bool useCursorControl = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }


    private void Start() {
        InputManager.Instance.OnTogglePause += HandleTogglePause;

        if (CustomSceneManager.Instance.GetActiveSceneIndex() == 0) {
            SoundManager.Instance?.PlayMusic("MainMenu");
        }
        else if (CustomSceneManager.Instance.GetActiveSceneIndex() == 1) {
            SoundManager.Instance?.PlayMusic("LoadScreen");
        }
        else if (CustomSceneManager.Instance.GetActiveSceneIndex() >= 2) {
            ResetGame();
        }
        
    }


    private void Update() {
        if (Input.GetKeyUp(quitGameKey)) { CustomSceneManager.Instance?.ExitGame(); }
        if (Input.GetKeyUp(restartSceneKey) && (CustomSceneManager.Instance.GetActiveSceneIndex() >= 2)) { CustomSceneManager.Instance?.ResetScene(false); ResetGame();}
    }
    private void OnDestroy()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnTogglePause -= HandleTogglePause;
        }
    }

    private void HandleRestart()
    {
        CustomSceneManager.Instance?.ResetScene();
    }

    private void HandleQuit()
    {
        CustomSceneManager.Instance?.ExitGame();
    }

    private void HandleTogglePause()
    {
        TogglePause();
    }

    public void TogglePause() 
    {
        if (currentGameState != GameStates.GamePlay)  
        return;

        switch (currentGameState) 
        {
            case GameStates.GamePlay:
                currentGameState = GameStates.Paused;
                if (useCursorControl) SetCursorState(true);
                Time.timeScale = 0;
                break;
            case GameStates.Paused:
                currentGameState = GameStates.GamePlay;
                if (useCursorControl) SetCursorState(false);
                Time.timeScale = 1;
                break;
        }
        
        UIManager.Instance?.UpdateUI();
    }

    public void CycleGameMode()
    {
        switch (currentGameDifficulty)
        {
            case GameDifficulty.Easy:
                SetGameDifficulty(GameDifficulty.Easy);
                break;
            case GameDifficulty.Normal:
                SetGameDifficulty(GameDifficulty.Hard);
                break;
            case GameDifficulty.Hard:
                SetGameDifficulty(GameDifficulty.Easy);
                break;
            default:
                SetGameDifficulty(GameDifficulty.Normal);
                break;
        }

        UIManager.Instance?.UpdateAmmoUI();
        UIManager.Instance?.UpdateTimeUI();
        Debug.Log("Game Mode: " + currentGameDifficulty);
    }

    private void SetCursorState(bool visible)
    {
        if (useCursorControl)
        {
            Cursor.visible = visible;
            Cursor.lockState = visible ? CursorLockMode.None : CursorLockMode.Locked;
        }
    }

    public void SetGameDifficulty(GameDifficulty gameMode)
    {
        TimerManager.Instance.paused = false;
        currentGameDifficulty = gameMode;

        switch(gameMode)
        {
            case GameDifficulty.Easy:
                TimerManager.Instance.ResetTime(false);
                break;
            case GameDifficulty.Normal:
                TimerManager.Instance.ResetTime(false);
                break;
            case GameDifficulty.Hard:
                TimerManager.Instance.ResetTime(true);
                break;
            default:
                Debug.LogError("Invalid Game Mode, Setting Normal");
                break;
        }
    }

    public void ResetGame()
    {
        if (chosenGameDifficulty == GameDifficulty.None) { SetGameDifficulty(GameDifficulty.Normal); }
        else { SetGameDifficulty(chosenGameDifficulty); }
        
        if (useCursorControl) SetCursorState(false);

        currentGameState = GameStates.GamePlay;
        ScoreManager.Instance.ResetScore();
        Time.timeScale = 1;
        UIManager.Instance?.UpdateUI();
        SoundManager.Instance?.PlayMusic("Gameplay");
        UIManager.Instance?.UpdateAmmoUI();
    }

    public void CheckGameOver()
    {
        if (currentGameState == GameStates.GamePlay) {
            // EndGame(GameOverState.None);
        }
    }

    void EndGame(GameOverState state)
    {
        currentGameState = GameStates.GameOver;
        currentGameOverState = state;
        
        if (useCursorControl) SetCursorState(true);

        UIManager.Instance?.UpdateUI();
        SoundManager.Instance?.PlaySound("GameOverWin", 2f);

        Debug.Log(state.ToString());
    }
}

public enum GameStates {
    None,
    GamePlay,
    Paused,
    GameOver
}

public enum GameDifficulty {
    None,
    Easy,
    Normal,
    Hard,
}

public enum GameOverState {
    None,
    Lose,
    Win,
    PerfectWin
}