using UnityEngine;
using UnityEngine.SceneManagement;
using VInspector;



public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Tab("Settings")] // ------------------------------------------
    public GameStates gameState = GameStates.GamePlay;
    public GameDifficulty gameDifficulty = GameDifficulty.None;
    public bool funnyMode; 
    public bool debugMode;
    [SerializeField] private KeyCode quitGameKey = KeyCode.F1;
    [SerializeField] private KeyCode restartSceneKey = KeyCode.F2;
    [SerializeField] private KeyCode toggleDebugMode = KeyCode.F3;
    [SerializeField] private KeyCode toggleFunnyMode = KeyCode.F4;
    [SerializeField] private KeyCode finishGame = KeyCode.F5;
    
    // public Level[] levels;
    public Unlock[] unlocks;
    public Collectible[] collectibles;
    [EndTab]
    
    [Tab("References")] // ------------------------------------------
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
            
            SetGameState(GameStates.None);
            SetGameDifficulty(GameDifficulty.None);
            LoadCollectibles();
        } else {
            SetGameState(GameStates.GamePlay);
            SetGameDifficulty(GameDifficulty.None);
        }
    }
    
    
    private void Update() {
        if (Input.GetKeyUp(quitGameKey)) { CustomSceneManager.Instance?.ExitGame(); }
        if (Input.GetKeyUp(restartSceneKey)) { SceneManager.LoadScene(SceneManager.GetActiveScene().name); }
        if (Input.GetKeyUp(toggleDebugMode)) { ToggleDebugMode(); SoundManager.Instance?.PlaySoundFX("Toggle");}
        if (Input.GetKeyUp(toggleFunnyMode)) { ToggleFunnyMode(); SoundManager.Instance?.PlaySoundFX("Toggle");}
        if (Input.GetKeyUp(finishGame)) { FinishGame(); SoundManager.Instance?.PlaySoundFX("Toggle");}
        if (InputManager.TogglePauseWasPressed) { TogglePause(); }
        
    }
    

    public void QuitGame()
    {
        Application.Quit();

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.ExitPlaymode();
        #endif
    }
    
    [Button] public void FinishGame() {
        
        
        // Collect all collectibles
        CollectAllCollectibles();

        
        // Set highest level
        SaveManager.Instance?.SaveInt("HighestLevel", SceneManager.sceneCountInBuildSettings - 3);
        SaveManager.Instance?.SaveString("SavedLevel", "Level1");
        SaveManager.Instance?.SaveInt("TotalCollectibles", collectibles.Length);
        
        // Restart scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    
    
    
    [Button] public void DeleteSave()
    {
        SaveManager.Instance?.DeleteAllKeys();
        SaveManager.Instance?.SaveInt("HighestLevel", 1);
        SaveManager.Instance?.SaveString("SavedLevel", "Level1");
        SaveManager.Instance?.SaveInt("SavedCheckpoint", 0);
        SettingsManager.Instance?.LoadAllSettings();
        ResetCollectibles();
        ResetUnlocks();
        

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    #region GameStates // -----------------------------------------

    private void TogglePause() {
        
        if (gameState == GameStates.GamePlay) {
            SetGameState(GameStates.Paused);
        } else if (gameState == GameStates.Paused) {
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
        UIManager.Instance?.ToggleDebugUI(debugMode);
    }
    
    
    public void SetGameState(GameStates state) {
        gameState = state;
        UIManager.Instance?.UpdateUI();
        
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
        gameDifficulty = gameMode;

        switch(gameMode)
        {
            case GameDifficulty.Easy:

                break;
            case GameDifficulty.Normal:

                break;
            case GameDifficulty.Hard:

                break;
            
            case GameDifficulty.None:
                break;
            default:
                Debug.LogError("Invalid Game Mode, Setting Normal");
                break;
        }
    }
    
    

    #endregion GameStates
    
    #region Unlockss // ---------------------------------

    private void CheckUnlocks()
    {
        
        foreach (Unlock unlock in unlocks)
        {
            if (TotalCollectiblesCollected() >= unlock.unlockedAtCollectible)
            {
                unlock.unlockReceived = TotalCollectiblesCollected() >= unlock.unlockedAtCollectible;
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
                return unlock.unlockReceived;
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
    
    public void ResetUnlocks()
    {
        foreach (Unlock unlock in unlocks)
        {
            unlock.unlockReceived = false;
        }
    }
    
    #endregion Unlocks
    
    #region Collectibles // -----------------------------------------------
    
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

    private void ResetCollectibles()
    {
            foreach (Collectible collectible in collectibles) 
            {
                collectible.collected = false;
                SaveManager.Instance.SaveBool("Collectible " + collectible.connectedLevel.SceneName, collectible.collected);
            }
            
            CheckUnlocks();
    }

    private void CollectAllCollectibles()
    {
        
            foreach (Collectible collectible in collectibles) 
            {
                collectible.collected = true;
                SaveManager.Instance.SaveBool("Collectible " + collectible.connectedLevel.SceneName, collectible.collected);
            }

            CheckUnlocks();
    }
    
    #endregion Collectibles
    
}


