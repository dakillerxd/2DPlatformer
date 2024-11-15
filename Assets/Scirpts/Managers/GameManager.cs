using System;
using UnityEngine;
using System.Text;
using UnityEngine.SceneManagement;
using VInspector;

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

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Tab("GameManager")] // ----------------------------------------------------------------------
    [Header("Settings")]
    public GameStates currentGameState = GameStates.GamePlay;
    public GameDifficulty currentGameDifficulty = GameDifficulty.None;
    
    [Header("Debug")]
    [SerializeField] private KeyCode quitGameKey = KeyCode.F1;
    [SerializeField] private KeyCode restartSceneKey = KeyCode.F2;
    [SerializeField] private KeyCode toggleDebugText = KeyCode.F3;
    [SerializeField] [Range(1,999)] private int targetFPS = 120;
    [SerializeField] [Min(0)] private int vSync = 0;
    [SerializeField] public bool showDebugInfo = false;
    [ReadOnly] public InputManager inputManager;
    [ReadOnly] public SoundManager soundManager;
    [ReadOnly] public UIManager uiManager;
    [ReadOnly] public Camera cam;
    
    [Tab("References")] // ----------------------------------------------------------------------
    public InputManager inputManagerPrefab;
    public SoundManager soundManagerPrefab;
    public UIManager uiManagerPrefab;
    
    private void Awake() {
        
        if (Instance != null && Instance != this) 
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    private void Start() {
        // Check that all managers are instanced
        if (InputManager.Instance == null) { Instantiate(inputManagerPrefab); }
        if (SoundManager.Instance == null) { Instantiate(soundManagerPrefab); }
        if (UIManager.Instance == null) { Instantiate(uiManagerPrefab); }
        
        // Set instances
        inputManager = InputManager.Instance;
        soundManager = SoundManager.Instance;
        uiManager = UIManager.Instance;
        
        // Set settings
        QualitySettings.vSyncCount = vSync;
        Application.targetFrameRate = targetFPS;
        
        // Update UI
        UIManager.Instance.ToggleDebugUI(showDebugInfo);
        UIManager.Instance.UpdateUI();
    }
    
    
    private void Update() {
        UpdateTextInfo();
        if (Input.GetKeyUp(quitGameKey)) { CustomSceneManager.Instance?.ExitGame(); }
        if (Input.GetKeyUp(restartSceneKey)) { SceneManager.LoadScene(SceneManager.GetActiveScene().name); }
        if (Input.GetKeyUp(toggleDebugText)) { ToggleDebugText(); }
        if (Input.GetKeyUp(KeyCode.F)) { CameraController.Instance.ShakeCamera(5f, 2f, 2f,  2f); }
        if (InputManager.TogglePauseWasPressed) { TogglePause(); }
    }
    


    #region GameStates

    private void TogglePause() {

        if (currentGameState == GameStates.GamePlay) {
            SetGameState(GameStates.Paused);
        } else if (currentGameState == GameStates.Paused) {
            SetGameState(GameStates.GamePlay);
        }
    }
    
    
    private void SetGameState(GameStates state) {
        currentGameState = state;
        UIManager.Instance.UpdateUI();
        
        switch (state) {
            case GameStates.GamePlay:
                Time.timeScale = 1;
                
                break;
            case GameStates.Paused:
                Time.timeScale = 0;
                
                break;
        }
    }
    
    private void SetGameDifficulty(GameDifficulty gameMode) {
        currentGameDifficulty = gameMode;

        switch(gameMode)
        {
            case GameDifficulty.Easy:

                break;
            case GameDifficulty.Normal:

                break;
            case GameDifficulty.Hard:

                break;
            default:
                Debug.LogError("Invalid Game Mode, Setting Normal");
                break;
        }
    }

    #endregion GameStates
    
    #region Debugging functions

    private void ToggleDebugText() {
        showDebugInfo = !showDebugInfo;
        UIManager.Instance.ToggleDebugUI(showDebugInfo);
    }

    private void UpdateTextInfo() {
        
        if (!showDebugInfo) return;
        if (!cam) { cam = Camera.main;}
        
        UIManager.Instance.UpdateDebugUI();
    }
    

    
    #endregion Debugging functions

}


