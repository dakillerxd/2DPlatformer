using UnityEngine;
using CustomAttribute;
using System.Text;
using TMPro;
using UnityEngine.SceneManagement;

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
    public GameStates currentGameState = GameStates.None;
    public GameDifficulty currentGameDifficulty = GameDifficulty.None;

    [Header("Debug")]
    [SerializeField] private KeyCode quitGameKey = KeyCode.F1;
    [SerializeField] private KeyCode restartSceneKey = KeyCode.F2;
    [SerializeField] private KeyCode toggleDebugText = KeyCode.F3;
    [SerializeField] [Min(15)] private int targetFPS = 120;
    [SerializeField] [Min(0)] private int vSync = 0;
    [SerializeField] private bool showFps;
    [SerializeField] private bool showDebugInfo;
    private TextMeshProUGUI fpsText;
    private TextMeshProUGUI playerDebugText;
    private TextMeshProUGUI cameraDebugText;
    private Camera cam;

    
    private void Awake() {
        if (Instance != null && Instance != this) {

            Destroy(gameObject);

        } else {

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
    
    private void Start() {
        QualitySettings.vSyncCount = vSync;
        Application.targetFrameRate = targetFPS;
        fpsText = GameObject.Find("FpsText").GetComponent<TextMeshProUGUI>();
        playerDebugText = GameObject.Find("PlayerDebugText").GetComponent<TextMeshProUGUI>();
        cameraDebugText = GameObject.Find("CameraDebugText").GetComponent<TextMeshProUGUI>();
        cam = Camera.main;
    }
    
    
    private void Update() {
        UpdateFpsText();
        UpdateDebugTextInfo();
        HandleTargetSelection();
        if (Input.GetKeyUp(quitGameKey)) { CustomSceneManager.Instance?.ExitGame(); }
        if (Input.GetKeyUp(restartSceneKey)) { SceneManager.LoadScene(SceneManager.GetActiveScene().name); }
        if (Input.GetKeyUp(toggleDebugText)) { ToggleDebugText(); }
    }


    #region GameStates

    private void TogglePause() {
        
        if (currentGameState != GameStates.GamePlay) return;

        switch (currentGameState) 
        {
            case GameStates.GamePlay:
                currentGameState = GameStates.Paused;
                Time.timeScale = 0;
                
                break;
            case GameStates.Paused:
                currentGameState = GameStates.GamePlay;
                Time.timeScale = 1;
                
                break;
        }

    }
    private void SetGameDifficulty(GameDifficulty gameMode)
    {
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
    private void SetGameState(GameStates state) {
        currentGameState = state;
        
        switch (state) 
        {
            case GameStates.GamePlay:
                currentGameState = GameStates.Paused;
                Time.timeScale = 0;
                
                break;
            case GameStates.Paused:
                currentGameState = GameStates.GamePlay;
                Time.timeScale = 1;
                
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
        if (!playerDebugText) { GameObject.Find("PlayerDebugText").GetComponent<TextMeshProUGUI>(); }
        if (!cameraDebugText) { GameObject.Find("CameraDebugText").GetComponent<TextMeshProUGUI>(); }
        if (!cam) { cam = Camera.main;}
    }

    private void UpdateDebugTextInfo()
    {
        if (playerDebugText)
        {
            PlayerController2D.Instance.UpdateDebugText(playerDebugText, showDebugInfo);
        }

        if (cam && cameraDebugText)
        {
            CameraController2D.Instance.UpdateDebugText(cameraDebugText, showDebugInfo);
        }
    }



    private readonly StringBuilder fpsStringBuilder = new StringBuilder(256);
    private void UpdateFpsText() {

        if (fpsText) {
            fpsText.enabled = showFps;
            if (showFps) {  

                fpsStringBuilder.Clear();

                float deltaTime = 0.0f;
                deltaTime += Time.unscaledDeltaTime - deltaTime;
                float fps = 1.0f / deltaTime;

                fpsStringBuilder.AppendFormat("FPS: {0}\n", (int)fps);
                fpsStringBuilder.AppendFormat("VSync: {0}\n", QualitySettings.vSyncCount);

                fpsText.text = fpsStringBuilder.ToString();
            }
        }
    }

    
    #endregion Debugging functions

}


