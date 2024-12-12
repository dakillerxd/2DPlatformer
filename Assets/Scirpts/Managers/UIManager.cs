using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Text;
using UnityEngine.SceneManagement;
using VInspector;


public class UIManager : MonoBehaviour
{   
    public static UIManager Instance { get; private set; }

    
    [Tab("UI Settings")] // ----------------------------------------------------------------------
    [Header("Screens")]
    [SerializeField] private GameObject gamePlayUI;
    [SerializeField] private GameObject pauseScreenUI;
    [SerializeField] private GameObject gameOverUI;
    
    [Header("Debug")]
    [SerializeField] private TextMeshProUGUI playerDebugText;
    [SerializeField] private TextMeshProUGUI cameraDebugText;
    public TextMeshProUGUI fpsText;
    
    [Tab("UI Gameplay")] // ----------------------------------------------------------------------
    [Header("Level Title")]
    [SerializeField] private  TMP_Text levelTitleText;
    
    [Header("Time")]
    [SerializeField] private TMP_Text[] timerTexts;
    [SerializeField] private  Color timerWarningColor = Color.red;
    private Color timerOriginalColor;
    
    [Header("Score")]
    [SerializeField] private  TMP_Text[] scoreTexts;

    [Header("Abilities")] 
    [SerializeField] private  Color abilityUnlocked = Color.white;
    [SerializeField] private  Color abilityLocked = new Color(1f, 1f, 1f, 0.5f);
    [SerializeField] private GameObject doubleJump;
    [SerializeField] private GameObject wallSlide;
    [SerializeField] private GameObject wallJump;
    [SerializeField] private GameObject dash;

    [Tab("UI Pause")] // ----------------------------------------------------------------------
    [SerializeField] private GameObject panelMain;
    [SerializeField] private GameObject panelOptions;
    [SerializeField] private Button buttonResume;
    [SerializeField] private Button buttonOptions;
    [SerializeField] private Button buttonMainMenu;
    [SerializeField] private Button buttonQuit;
    [SerializeField] private Button buttonOptionsBack;
    [SerializeField] private  TMP_Text pauseTimeText;
    [SerializeField] private  TMP_Text pauseScoreText;
    [SerializeField] private  TMP_Text pauseAmmoText;
    
    [Tab("UI Game Over")] // ----------------------------------------------------------------------
    [SerializeField] private  TMP_Text gameOverTimeText;
    [SerializeField] private  TMP_Text gameOverScoreText;
    [SerializeField] private  TMP_Text gameOverTitleText;
    [SerializeField] private  TMP_Text gameOverMessageText;
    [SerializeField] private List<string> loseMessages = new List<string> { "1!", "2", };
    [SerializeField] private List<string> winMessages;
    [SerializeField] private List<string> perfectWinMessages;

    

    private void Awake() {
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
        InitializeUI();


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
        ToggleDebugUI(GameManager.Instance.debugMode);
        UpdateUI();
        
        if (nextScene.name == "MainMenu") return;
        UpdateAbilitiesUI();
        StartLevelTitleEffect(1, SceneManager.GetActiveScene().name.Replace("Level", "Level ").Trim());

    }

    
    
    
    private void InitializeUI()
    {
        
        if (levelTitleText)
        {
            levelTitleText.CrossFadeAlpha(0, 0, false);
        }
        
        if (timerTexts.Length > 0)
        {
            foreach (var timerText in timerTexts)
            {
                timerOriginalColor = timerText.color;
            }
        }
        
        if (buttonResume != null)
        {
            buttonResume.onClick.RemoveAllListeners();
            buttonResume.onClick.AddListener(() => GameManager.Instance.SetGameState(GameStates.GamePlay));
        }

        if (buttonOptions != null)
        {
            buttonOptions.onClick.RemoveAllListeners();
            buttonOptions.onClick.AddListener(ShowPanelOptions);
        }
        
        if (buttonMainMenu != null)
        {
            buttonMainMenu.onClick.RemoveAllListeners();
            buttonMainMenu.onClick.AddListener(() => GameManager.Instance.SetGameState(GameStates.None));
            buttonMainMenu.onClick.AddListener(() => SceneManager.LoadScene(0));
        }

        if (buttonQuit != null)
        {
            buttonQuit.onClick.RemoveAllListeners();
            buttonQuit.onClick.AddListener(() => GameManager.Instance.QuitGame());
        }

        ShowPanelMain();

    }
    

    public void UpdateUI() {
        
        gamePlayUI.SetActive(false);
        pauseScreenUI.SetActive(false);
        gameOverUI.SetActive(false);
        
        switch (GameManager.Instance.currentGameState)
        {
            case GameStates.GamePlay:
                gamePlayUI.SetActive(true);
                UpdateDebugUI();
                break;
            case GameStates.Paused:
                pauseScreenUI.SetActive(true);
                break;
            case GameStates.GameOver:
                gameOverUI.SetActive(true);
                break;
        }


    }

    
    

#region  Gameplay Screen

    public void UpdateAbilitiesUI() {
        if (PlayerController.Instance == null) return;
        doubleJump.SetActive(PlayerController.Instance.doubleJumpAbility);
        wallSlide.SetActive(PlayerController.Instance.wallSlideAbility);
        wallJump.SetActive(PlayerController.Instance.wallJumpAbility);
        dash.SetActive(PlayerController.Instance.dashAbility);

    }
    
    
    public void UpdateScoreUI()
    {
        if (scoreTexts.Length > 0)
        {
            foreach (var studentsText in scoreTexts)
            {
                studentsText.text = ScoreManager.Instance.currentScore.ToString();
            }
        }
    }
    
    public void UpdateTimeUI() 
    {
        if (GameManager.Instance.currentGameDifficulty == GameDifficulty.Easy)
        {
            foreach (var timerText in timerTexts)
            {
                timerText.text = "∞";
                timerText.color = Color.yellow;
            }
        }
        else {
            if (timerTexts.Length > 0)
            {
                int timeLeftInt = Mathf.CeilToInt(TimerManager.Instance.currentGameTime);

                foreach (var timerText in timerTexts)
                {
                    timerText.text = timeLeftInt.ToString();
                }

                Color timerColor = (timeLeftInt <= TimerManager.Instance.warningTime) ? timerWarningColor : timerOriginalColor;
                foreach (var timerText in timerTexts)
                {
                    timerText.color = timerColor;
                }
            }

        }

    }
    
    public void StartLevelTitleEffect(float duration, string title) {
        if (!levelTitleText) return;
        StartCoroutine(LevelTitleEffect(duration, title));
    }
    
    private IEnumerator LevelTitleEffect(float duration, string title) {
        
        if (!levelTitleText) yield break;
        levelTitleText.CrossFadeAlpha(0, 0, false);
        levelTitleText.text = title;
        levelTitleText.CrossFadeAlpha(1, 1, false);
        yield return new WaitForSeconds(1);
        levelTitleText.CrossFadeAlpha(0, duration, false);
    }
    
    

#endregion

#region Pause Screen


    
    private void UpdatePauseScreenInfo()
    {
        if (pauseTimeText != null)
        {
            if (GameManager.Instance.currentGameDifficulty == GameDifficulty.Easy) {

                pauseTimeText.text = "∞";
                pauseTimeText.color = Color.yellow;

            }
            else if (GameManager.Instance.currentGameDifficulty == GameDifficulty.Normal) {

                int timeLeftInt = Mathf.CeilToInt(TimerManager.Instance.currentGameTime);
                int totalTimeInt = Mathf.CeilToInt(TimerManager.Instance.gameTime);
                pauseTimeText.text = $"{timeLeftInt} / {totalTimeInt}";
            }
            else if (GameManager.Instance.currentGameDifficulty == GameDifficulty.Hard) {

                int timeLeftInt = Mathf.CeilToInt(TimerManager.Instance.currentGameTime);
                int totalTimeInt = Mathf.CeilToInt(TimerManager.Instance.gameTimeHard);
                pauseTimeText.text = $"{timeLeftInt} / {totalTimeInt}";
            }

        }

        if (pauseScoreText != null)
        {

            pauseScoreText.text = ScoreManager.Instance.currentScore.ToString();
        }
        

    }

    public void ShowPanelMain()
    {
        panelMain.SetActive(true);
        panelOptions.SetActive(false);
    }

    public void ShowPanelOptions()
    {
        panelMain.SetActive(false);
        panelOptions.SetActive(true);
    }


#endregion

#region GameOver Screen


    private void UpdateGameOverInfo()
    {
        if (gameOverTimeText != null)
        {
            if (GameManager.Instance.currentGameDifficulty == GameDifficulty.Easy) {

                gameOverTimeText.text = "∞";
                gameOverTimeText.color = Color.yellow;

            }
            else if (GameManager.Instance.currentGameDifficulty == GameDifficulty.Normal) {

                int timeLeftInt = Mathf.CeilToInt(TimerManager.Instance.currentGameTime);
                int totalTimeInt = Mathf.CeilToInt(TimerManager.Instance.gameTime);
                gameOverTimeText.text = $"{timeLeftInt} / {totalTimeInt}";
            }
            else if (GameManager.Instance.currentGameDifficulty == GameDifficulty.Hard) {

                int timeLeftInt = Mathf.CeilToInt(TimerManager.Instance.currentGameTime);
                int totalTimeInt = Mathf.CeilToInt(TimerManager.Instance.gameTimeHard);
                gameOverTimeText.text = $"{timeLeftInt} / {totalTimeInt}";
            }
        }

        if (gameOverScoreText != null)
        {
            
            gameOverScoreText.text = ScoreManager.Instance.currentScore.ToString();
        }
        

    }


    private string GetRandomMessage(List<string> messages)
    {
        if (messages == null || messages.Count == 0)
        {
            return "No message available.";
        }
        return messages[Random.Range(0, messages.Count)];
    }

#endregion

#region Debug UI

    public void ToggleDebugUI(bool state) {
        
        playerDebugText.enabled = state;
        cameraDebugText.enabled = state;
        fpsText.enabled = state;
    }
    
    public void UpdateDebugUI() {

        if (GameManager.Instance.debugMode)
        {
            if (playerDebugText)
            {
                PlayerController.Instance?.UpdateDebugText(playerDebugText);
            }

            if (cameraDebugText)
            {
                CameraController.Instance?.UpdateDebugText(cameraDebugText);
            }

            if (fpsText)
            {
                UpdateFpsText();
            }
        }
    }
    
    
    private readonly StringBuilder fpsStringBuilder = new StringBuilder(256);
    public void UpdateFpsText() {

        fpsStringBuilder.Clear();

        float deltaTime = 0.0f;
        deltaTime += Time.unscaledDeltaTime - deltaTime;
        float fps = 1.0f / deltaTime;

        fpsStringBuilder.AppendFormat("FPS: {0}\n", (int)fps);
        fpsStringBuilder.AppendFormat("VSync: {0}\n", QualitySettings.vSyncCount);

        fpsText.text = fpsStringBuilder.ToString();
    }

#endregion


}