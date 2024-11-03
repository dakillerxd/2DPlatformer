using System;
using UnityEngine;
using CustomAttribute;
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

    [Header("Game Settings")]
    public GameStates currentGameState = GameStates.GamePlay;
    public GameDifficulty currentGameDifficulty = GameDifficulty.None;

    [Header("Managers")]
    public InputManager inputManager;
    public SoundManager soundManager;
    public UIManager uiManager;
    
    [Header("Debug")]
    [SerializeField] private KeyCode quitGameKey = KeyCode.F1;
    [SerializeField] private KeyCode restartSceneKey = KeyCode.F2;
    [SerializeField] private KeyCode toggleDebugText = KeyCode.F3;
    [SerializeField] [Range(1,999)] private int targetFPS = 120;
    [SerializeField] [Min(0)] private int vSync = 0;
    [SerializeField] private bool showFps;
    [SerializeField] private bool showDebugInfo;
    private Camera cam;

    
    private void Awake() {
        
        if (Instance != null && Instance != this) 
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        // Set settings
        QualitySettings.vSyncCount = vSync;
        Application.targetFrameRate = targetFPS;
        
        // Check that all managers are instanced
        if (InputManager.Instance == null) { Instantiate(inputManager); }
        if (SoundManager.Instance == null) { Instantiate(soundManager); }
        if (UIManager.Instance == null) { Instantiate(uiManager); }

        
    }
    
    private void Start() {
        UIManager.Instance.UpdateUI();
    }
    
    
    private void Update() {
        UpdateTextInfo();
        HandleTargetSelection();
        if (Input.GetKeyUp(quitGameKey)) { CustomSceneManager.Instance?.ExitGame(); }
        if (Input.GetKeyUp(restartSceneKey)) { SceneManager.LoadScene(SceneManager.GetActiveScene().name); }
        if (Input.GetKeyUp(toggleDebugText)) { ToggleDebugText(); }
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
    
    private void HandleTargetSelection()
    {
        if (Input.GetMouseButtonDown(0)) {
            Vector2 mousePosition = cam.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);

            if (hit) {
                if (hit.collider.CompareTag("Player")) {
                    CameraController2D.Instance.SetTarget(hit.collider.transform.parent.parent);
                    Debug.Log("Set camera target to: " + hit.collider.transform.parent.parent.name);
                }
                else if (hit.collider.CompareTag("Enemy")) {
                    CameraController2D.Instance.SetTarget(hit.collider.transform.parent.parent);
                }
                else if (hit.collider.CompareTag("Checkpoint")) {
                    CheckpointManager2D.Instance.ActivateCheckpoint(hit.collider.gameObject.GetComponent<Checkpoint2D>());
                }
                else {
                    Debug.Log("Clicked on: " + hit.collider.gameObject.name);
                }
            }
        }
    }

    private void ToggleDebugText() {
        showDebugInfo = !showDebugInfo;
        UIManager.Instance.playerDebugText.enabled = showDebugInfo;
        UIManager.Instance.cameraDebugText.enabled = showDebugInfo;
    }

    private void UpdateTextInfo() {
        
        if (showFps) { UpdateFpsText(); }
        
        if (showDebugInfo) {
            PlayerController2D.Instance.UpdateDebugText(UIManager.Instance.playerDebugText);
            if (!cam) { cam = Camera.main;}
            CameraController2D.Instance.UpdateDebugText(UIManager.Instance.cameraDebugText);
        }
    }
    
    private readonly StringBuilder fpsStringBuilder = new StringBuilder(256);
    private void UpdateFpsText() {

        fpsStringBuilder.Clear();

        float deltaTime = 0.0f;
        deltaTime += Time.unscaledDeltaTime - deltaTime;
        float fps = 1.0f / deltaTime;

        fpsStringBuilder.AppendFormat("FPS: {0}\n", (int)fps);
        fpsStringBuilder.AppendFormat("VSync: {0}\n", QualitySettings.vSyncCount);

        UIManager.Instance.fpsText.text = fpsStringBuilder.ToString();
    }

    
    #endregion Debugging functions

}


