using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{   
    public static UIManager Instance { get; private set; }

    [Header("UI Screens")]
    [SerializeField] private  GameObject gameplayUI;
    [SerializeField] private  GameObject pauseScreenUI;
    [SerializeField] private  GameObject gameoverUI;

    [Header("Gameplay UI")]
    [SerializeField] private TMP_Text[] timerTexts;
    [SerializeField] private  Animation clockAnimation;
    [SerializeField] private  Color timerWarningColor = Color.red;
    private Color timerOriginalColor;
    [SerializeField] private  TMP_Text[] scoreTexts;
    [SerializeField] private  Animation scoreAnimation;
    [SerializeField] private  TMP_Text[] ammoTexts;
    [SerializeField] private  Animation ammoAnimation;

    [Header("Pause Screen")]
    [SerializeField] private GameObject panelMain;
    [SerializeField] private GameObject panelOptions;
    [SerializeField] private  TMP_Text pauseTimeText;
    [SerializeField] private  TMP_Text pauseScoreText;
    [SerializeField] private  TMP_Text pauseAmmoText;


    [Header("Game Over Screen")]
    [SerializeField] private  TMP_Text gameOverTimeText;
    [SerializeField] private  TMP_Text gameOverScoreText;
    [SerializeField] private  TMP_Text gameOverTitleText;
    [SerializeField] private  TMP_Text gameOverMessageText;
    [SerializeField] private List<string> loseMessages = new List<string>
    {
        "You didn't teach enough students. Try again!",
        "The bell rang before you could finish. Better luck next time!",
        "Looks like you need to brush up on your teaching skills.",
        "Not quite there yet. Give it another shot!"
    };
    [SerializeField] private List<string> winMessages = new List<string>
    {
        "You taught enough students. Well done!",
        "Great job! Your students learned a lot today.",
        "You're a natural teacher. Keep up the good work!",
        "Success! Your classroom management skills are impressive."
    };
    [SerializeField] private List<string> perfectWinMessages = new List<string>
    {
        "You taught all the students! Outstanding job!",
        "Perfect score! You're the teacher of the year!",
        "Incredible! Every student is now a little genius thanks to you.",
        "Flawless victory! Your teaching skills are unmatched!"
    };
    
    


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start() 
    {
        if (timerTexts.Length > 0)
        {
            foreach (var timerText in timerTexts)
            {
                timerOriginalColor = timerText.color;
            }
        }

    }


    public void UpdateUI() 
    {
        gameplayUI.SetActive(false);
        pauseScreenUI.SetActive(false);
        gameoverUI.SetActive(false);

        switch (GameManager.Instance.currentGameState)
        {
            case GameStates.GamePlay:
                gameplayUI.SetActive(true);
                UpdateScoreUI();
                UpdateTimeUI();
                UpdateAmmoUI();
                break;
            case GameStates.Paused:
                pauseScreenUI.SetActive(true);
                ShowPanelMain();
                UpdatePauseScreenInfo();
                break;
            case GameStates.GameOver:
                gameoverUI.SetActive(true);
                UpdateGameOverInfo();
                break;
        }


    }



#region  Gameplay UI

    public void UpdateScoreUI()
    {
        if (scoreTexts.Length > 0)
        {
            foreach (var studentsText in scoreTexts)
            {
                studentsText.text = ScoreManager.Instance.currentScore.ToString();
            }
        }

        PlayScoreUpdateAnimation();
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

    public void UpdateAmmoUI()
    {
        if (ammoTexts.Length > 0)
        {
            if (GameManager.Instance.currentGameDifficulty == GameDifficulty.Easy) {

                foreach (var weaponAmmoText in ammoTexts)
                {
                    weaponAmmoText.text = "∞";
                    weaponAmmoText.color = Color.yellow;
                }

            }
            else {

                foreach (var weaponAmmoText in ammoTexts)
                {
                    // weaponAmmoText.text = WeaponHandController.Instance.CurrentAmmo.ToString();
                    weaponAmmoText.color = timerOriginalColor;
                }

            }
        }
    }



    public void PlayTimerAnimation(string name) {
        clockAnimation?.Play(name);
    }

    public void PlayAmmoAnimation(string name) {
        ammoAnimation?.Play(name);
    }


    public void PlayScoreUpdateAnimation() {
        scoreAnimation?.Play();
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

        if (pauseAmmoText != null)
        {
            if (GameManager.Instance.currentGameDifficulty == GameDifficulty.Easy) {

                pauseAmmoText.text = "∞";
                pauseAmmoText.color = Color.yellow;

            }
            else {
                
                // int currentAmmo = WeaponHandController.Instance.CurrentAmmo;
                // int maxAmmo = WeaponHandController.Instance.MaxAmmo;
                // pauseAmmoText.text = $"{currentAmmo} / {maxAmmo}";

            }
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



#region GameOver UI


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

        if (gameOverTitleText != null && gameOverMessageText != null)
        {
            switch (GameManager.Instance.currentGameOverState)
            {
                case GameOverState.Lose:
                    gameOverTitleText.text = "You lose";
                    gameOverMessageText.text = GetRandomMessage(loseMessages);
                    break;
                case GameOverState.Win:
                    gameOverTitleText.text = "Congratulations!";
                    gameOverMessageText.text = GetRandomMessage(winMessages);
                    break;
                case GameOverState.PerfectWin:
                    gameOverTitleText.text = "Perfect!";
                    gameOverMessageText.text = GetRandomMessage(perfectWinMessages);
                    break;
            }
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



}