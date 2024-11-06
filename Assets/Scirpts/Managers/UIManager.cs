using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Text;
using VInspector;


public class UIManager : MonoBehaviour
{   
    public static UIManager Instance { get; private set; }

    
    [Tab("UI Settings")] // ----------------------------------------------------------------------
    [Header("Screens")]
    [SerializeField] private  GameObject gamePlayUI;
    [SerializeField] private  GameObject pauseScreenUI;
    [SerializeField] private  GameObject gameOverUI;
    
    [Header("Debug")]
    [SerializeField] private TextMeshProUGUI playerDebugText;
    [SerializeField] private TextMeshProUGUI cameraDebugText;
    public TextMeshProUGUI fpsText;
    
    [Tab("UI Gameplay")] // ----------------------------------------------------------------------
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

    private void Start() {
        if (timerTexts.Length > 0)
        {
            foreach (var timerText in timerTexts)
            {
                timerOriginalColor = timerText.color;
            }
        }
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
        if (PlayerController2D.Instance == null) return;
        doubleJump.SetActive(PlayerController2D.Instance.doubleJumpAbility);
        wallSlide.SetActive(PlayerController2D.Instance.wallSlideAbility);
        wallJump.SetActive(PlayerController2D.Instance.wallJumpAbility);
        dash.SetActive(PlayerController2D.Instance.dashAbility);

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

        if (GameManager.Instance.showDebugInfo)
        {
            if (playerDebugText)
            {
                PlayerController2D.Instance?.UpdateDebugText(playerDebugText);
            }

            if (cameraDebugText)
            {
                CameraController2D.Instance?.UpdateDebugText(cameraDebugText);
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