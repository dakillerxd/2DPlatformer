using UnityEngine;
using CustomAttribute;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
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
public class Unlock
{
    public string unlockName;
    public bool unlockState;
    public int unlockedAtCollectible;
    [CustomAttribute.ReadOnly] public bool received;
}

[System.Serializable]
public class Collectible
{
    public SceneField connectedLevel;
    public bool countsTowardsUnlocks = true;
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
    public Unlock[] unlocks;
    [Space(10)]
    public Collectible[] collectibles;
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
        if (Input.GetKeyUp(quitGameKey)) { CustomSceneManager.Instance?.ExitGame(); }
        if (Input.GetKeyUp(restartSceneKey)) { SceneManager.LoadScene(SceneManager.GetActiveScene().name); }
        if (Input.GetKeyUp(toggleDebugMode)) { ToggleDebugMode(); }
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

    private void TogglePause() {
        
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
    
    private void ToggleDebugMode() {
        debugMode = !debugMode;
        UIManager.Instance.ToggleDebugUI(debugMode);
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
    
    #region Collectibles/Unlocks // ---------------------------------

    private void CheckUnlocks()
    {
        
        foreach (Unlock unlock in unlocks)
        {
            if (TotalCollectiblesCollected() >= unlock.unlockedAtCollectible)
            {
                unlock.received = TotalCollectiblesCollected() >= unlock.unlockedAtCollectible;
            }
        }
    }
    
    public void ToggleUnlock(string unlockName, bool state)
    {
        foreach (Unlock unlock in unlocks)
        {
            if (unlock.unlockName == unlockName)
            {
                unlock.unlockState = state;
                SaveManager.Instance.SaveBool(unlockName, unlock.unlockState);
                PlayerController.Instance?.ToggleAllCosmetics();
                return;
            }
        }
        Debug.LogError("Unlock not found");
    }

    public bool CheckUnlockReceived(string unlockName)
    {
        foreach (Unlock unlock in unlocks)
        {
            if (unlock.unlockName == unlockName)
            {
                return unlock.received;
            }
        }
        Debug.LogError("Unlock not found");
        return false;
    }
    
    public bool CheckUnlockActive(string unlockName)
    {
        foreach (Unlock unlock in unlocks)
        {
            if (unlock.unlockName == unlockName)
            {
                return unlock.unlockState;
            }
        }
        Debug.LogError("Unlock not found");
        return false;
    }
    

    
    public void CollectCollectible(string connectedLevelName) {
        
        if (IsCollectibleCollected(connectedLevelName)) { return; }
        
        foreach (Collectible collectible in collectibles) {
            if (collectible.connectedLevel.SceneName == connectedLevelName) {
                collectible.collected = true;
                SaveManager.Instance.SaveBool("Collectible " + collectible.connectedLevel.SceneName, collectible.collected);
                return;
            }
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

    public int TotalCollectiblesAmount() // The total amount of collectibles in the game
    {
        int total = 0;
        foreach (Collectible collectible in collectibles) {
            if (collectible.countsTowardsUnlocks) {
                total++;
            }
        }
        return total;
    }
    
    public int TotalCollectiblesCollected() // The total amount of the collected collectibles
    {
        int total = 0;
        foreach (Collectible collectible in collectibles) {
            if (collectible.collected && collectible.countsTowardsUnlocks) {
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

        CheckUnlocks();

    }
    
    public void ResetCollectibles() {
        foreach (Collectible collectible in collectibles) {
            collectible.collected = false;
            SaveManager.Instance.SaveBool("Collectible " + collectible.connectedLevel.SceneName, collectible.collected);
        }
        
        LoadCollectibles();
    }
    
    [Button] public void CollectAllCollectibles() {
        
        foreach (Collectible collectible in collectibles) {
            collectible.collected = true;
            SaveManager.Instance.SaveBool("Collectible " + collectible.connectedLevel.SceneName, collectible.collected);
        }
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    
    #endregion Collectibles

    

}


