using UnityEngine;
using CustomAttribute;
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

[System.Serializable]
public class Collectible
{
    public SceneField connectedLevel;
    public bool counts = true;
    [CustomAttribute.ReadOnly] public bool collected;
    
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
    [SerializeField] public bool showDebugInfo;
    [CustomAttribute.ReadOnly] public Camera cam;
    
    
    [Header("Collectibles")]
    [SerializeField] public Collectible[] collectibles;
    public bool googlyEyesModeReceived {get ; private set;}
    public bool bonusLevel1Received {get ; private set;}
    public bool bonusLevel2Received {get ; private set;}

    
    [Tab("References")] // ----------------------------------------------------------------------
    public InputManager inputManagerPrefab;
    public SoundManager soundManagerPrefab;
    public UIManager uiManagerPrefab;
    public VFXManager vfxManagerPrefab;
    
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
        if (VFXManager.Instance == null) { Instantiate(vfxManagerPrefab); }

    }
    
    private void OnEnable()
    {
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
    }

    private void OnDisable()
    {
        SceneManager.activeSceneChanged -= OnActiveSceneChanged;
    }

    private void OnActiveSceneChanged(Scene currentScene, Scene nextScene)
    {
        LoadCollectibles();
        
        UIManager.Instance.ToggleDebugUI(showDebugInfo);
        UIManager.Instance.UpdateUI();
        
        VFXManager.Instance?.ToggleChromaticAberration(false);
        VFXManager.Instance?.ToggleLensDistortion(false);
        
        if (nextScene.name == "MainMenu") {
            VFXManager.Instance?.ToggleMotionBlur(true, 0.3f);
        } else {
            VFXManager.Instance?.ToggleMotionBlur(false);
        }
        
    }
    
    
    private void Update() {
        UpdateTextInfo();
        if (Input.GetKeyUp(quitGameKey)) { CustomSceneManager.Instance?.ExitGame(); }
        if (Input.GetKeyUp(restartSceneKey)) { SceneManager.LoadScene(SceneManager.GetActiveScene().name); }
        if (Input.GetKeyUp(toggleDebugText)) { ToggleDebugText(); }
        if (InputManager.TogglePauseWasPressed) { TogglePause(); }
    }
    
    public void QuitGame()
    {
        Application.Quit();

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.ExitPlaymode();
        #endif
    }

    #region GameStates

    public void TogglePause() {
        
        if (currentGameState == GameStates.GamePlay) {
            SetGameState(GameStates.Paused);
        } else if (currentGameState == GameStates.Paused) {
            SetGameState(GameStates.GamePlay);
        }
    }
    
    
    public void SetGameState(GameStates state) {
        currentGameState = state;
        UIManager.Instance.UpdateUI();
        
        switch (state) {
            case GameStates.GamePlay:
                Time.timeScale = 1;
                
                break;
            case GameStates.Paused:
                Time.timeScale = 0;
                
                break;
            case GameStates.GameOver:
                Time.timeScale = 0;
                break;
            
                case GameStates.None:
                Time.timeScale = 1;
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
    
    #region Collectibles
    
    public void CollectCollectible(string connectedLevelName) {
        foreach (Collectible collectible in collectibles) {
            if (collectible.connectedLevel.SceneName == connectedLevelName) {
                collectible.collected = true;
                SaveManager.Instance.SaveBool("Collectible " + collectible.connectedLevel.SceneName, collectible.collected);
                return;
            }
            Debug.LogError("Collectible not found");
        }
    }
    
    
    public bool IsCollectibleCollected(string connectedLevelName) {
        foreach (Collectible collectible in collectibles) {
            if (collectible.connectedLevel.SceneName == connectedLevelName) {
                return collectible.collected;
            }
        }
        Debug.LogError("Collectible not found");
        return false;
    }

    public int TotalCollectiblesAmount()
    {
        int total = 0;
        foreach (Collectible collectible in collectibles) {
            if (collectible.counts) {
                total++;
            }
        }
        return total;
    }
    
    public int TotalCollectiblesCollected()
    {
        int total = 0;
        foreach (Collectible collectible in collectibles) {
            if (collectible.collected && collectible.counts) {
                total++;
            }
        }
        return total;
    }

    private void LoadCollectibles()
    {
        foreach (Collectible collectible in collectibles) {
            
            collectible.collected = SaveManager.Instance.LoadBool("Collectible " + collectible.connectedLevel.SceneName);
        }

        if (TotalCollectiblesCollected() > 2)
        {
            googlyEyesModeReceived = true;
        }
        
        if (TotalCollectiblesCollected() > 4)
        {
           bonusLevel1Received = true;
        }
        
        if (TotalCollectiblesCollected() > 6)
        {
            bonusLevel2Received = true;
        }
    }
    
    public void ResetCollectibles() {
        foreach (Collectible collectible in collectibles) {
            collectible.collected = false;
            SaveManager.Instance.SaveBool("Collectible " + collectible.connectedLevel.SceneName, collectible.collected);
        }
    }
    
    [Button] public void CollectAllCollectibles() {
        foreach (Collectible collectible in collectibles) {
            collectible.collected = true;
            SaveManager.Instance.SaveBool("Collectible " + collectible.connectedLevel.SceneName, collectible.collected);
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    
    #endregion Collectibles
    
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


