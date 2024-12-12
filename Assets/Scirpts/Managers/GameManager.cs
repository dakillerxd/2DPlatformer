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

public enum CosmeticItems
{
    GooglyEye,
    PropellerHat,
    CurlyMustache
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
    public bool funnyMode;
    public bool debugMode;
    [SerializeField] private KeyCode quitGameKey = KeyCode.F1;
    [SerializeField] private KeyCode restartSceneKey = KeyCode.F2;
    [SerializeField] private KeyCode toggleDebugMode = KeyCode.F3;
    [SerializeField] private KeyCode toggleFunnyMode = KeyCode.F4;
    [EndTab]
    
    
    [Tab("Unlocks-Collectibles")] // ----------------------------------------------------------------------
    [Header("Unlocks")]
    public bool googlyEyes;
    public bool propellerHat;
    public bool curlyMustache;
    [Space(10)]
    public Collectible[] collectibles;
    public bool googlyEyesModeReceived {get ; private set;}
    public bool propellerHatReceived {get ; private set;}
    public bool curlyMustacheReceived {get ; private set;}
    public int collectiblesForUnlock1 {get ; private set;} // Set in awake
    public int collectiblesForUnlock2 {get ; private set;} // Set in awake
    public int collectiblesForUnlock3 {get ; private set;} // Set in awake
    [EndTab]
    
    [Tab("References")] // ----------------------------------------------------------------------
    public InputManager inputManagerPrefab;
    public SoundManager soundManagerPrefab;
    public UIManager uiManagerPrefab;
    public VFXManager vfxManagerPrefab;
    private Camera _camera;
    [EndTab]
    
    private void Awake() {
        
        if (Instance != null && Instance != this) 
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        collectiblesForUnlock1 = 2;
        collectiblesForUnlock2 = 4;
        collectiblesForUnlock3 = 6;
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
        if (!_camera) { _camera = Camera.main;}
        
        if (nextScene.name == "MainMenu") {
            
            LoadCollectibles();
        }
    }
    
    
    private void Update() {
        UpdateTextInfo();
        if (Input.GetKeyUp(quitGameKey)) { CustomSceneManager.Instance?.ExitGame(); }
        if (Input.GetKeyUp(restartSceneKey)) { SceneManager.LoadScene(SceneManager.GetActiveScene().name); }
        if (Input.GetKeyUp(toggleDebugMode)) { ToggleDebugText(); }
        if (Input.GetKeyUp(toggleFunnyMode)) { ToggleFunnyMode(); }
        if (InputManager.TogglePauseWasPressed) { TogglePause(); }
        
    }
    

    public void QuitGame()
    {
        Application.Quit();

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.ExitPlaymode();
        #endif
    }

    #region GameStates // -----------------------------------------

    public void TogglePause() {
        
        if (currentGameState == GameStates.GamePlay) {
            SetGameState(GameStates.Paused);
        } else if (currentGameState == GameStates.Paused) {
            SetGameState(GameStates.GamePlay);
        }
    }
    
    private void ToggleFunnyMode()
    {
        funnyMode = !funnyMode;
        UIManager.Instance?.UpdateUI();
    }
    
    private void ToggleDebugText() {
        debugMode = !debugMode;
        UIManager.Instance.ToggleDebugUI(debugMode);
    }

    private void UpdateTextInfo() {
        
        if (!debugMode) return;
        
        UIManager.Instance?.UpdateDebugUI();
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
    
    public void SetGameDifficulty(GameDifficulty gameMode) {
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
    
    #region Collectibles // ---------------------------------
    
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

        propellerHatReceived = TotalCollectiblesCollected() >= collectiblesForUnlock1;
        googlyEyesModeReceived = TotalCollectiblesCollected() >= collectiblesForUnlock2;
        curlyMustacheReceived = TotalCollectiblesCollected() >= collectiblesForUnlock3;

    }
    
    public void ResetCollectibles() {
        foreach (Collectible collectible in collectibles) {
            collectible.collected = false;
            SaveManager.Instance.SaveBool("Collectible " + collectible.connectedLevel.SceneName, collectible.collected);
        }
        
        googlyEyes = false;
        LoadCollectibles();
    }
    
    [Button] public void CollectAllCollectibles() {
        foreach (Collectible collectible in collectibles) {
            collectible.collected = true;
            SaveManager.Instance.SaveBool("Collectible " + collectible.connectedLevel.SceneName, collectible.collected);
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    
    #endregion Collectibles

    #region Cosmetics // ----------------------------------


    public void ToggleGooglyEyeMode(bool state) {
        
        googlyEyes = state;
        PlayerController.Instance?.ToggleCosmetics();
    }

    public void TogglePropellerHat(bool state) {

        propellerHat = state;
        PlayerController.Instance?.ToggleCosmetics();
    }
    
    public void ToggleCurlyMustache(bool state) {

        curlyMustache = state;
        PlayerController.Instance?.ToggleCosmetics();
    }

    #endregion

    

}


