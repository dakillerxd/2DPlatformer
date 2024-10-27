using UnityEngine;
using UnityEngine.UI;


public class MainMenuManager : MonoBehaviour
{
     [Header("Menu Screens")]
    public GameObject mainMenuScreen;
    public GameObject optionsScreen;
    public GameObject creditsScreen;
    public GameObject startScreen;

    [Header("Buttons")]
    [SerializeField] private Button buttonStart;
    [SerializeField] private Button buttonOptions;
    [SerializeField] private Button buttonCredits;
    [SerializeField] private Button buttonQuit;
    [SerializeField] private Button buttonOptionsBack;
    [SerializeField] private Button buttonCreditsBack;



    private void Start()
    {
        SetupButtons();
        ShowMainMenu();
    }

    private void SetupButtons()
    {
        if (buttonStart != null)
        {
            buttonStart.onClick.RemoveAllListeners();
            buttonStart.onClick.AddListener(() => SoundManager.Instance?.PlaySound("ButtonClick"));
            buttonStart.onClick.AddListener(() => CustomSceneManager.Instance?.LoadScene(2, false));
            buttonStart.onClick.AddListener(() => GameManager.Instance.chosenGameDifficulty = GameDifficulty.Normal);
            buttonStart.onClick.AddListener(() => GameManager.Instance.ResetGame());

        }

        if (buttonOptions != null)
        {
            buttonOptions.onClick.RemoveAllListeners();
            buttonOptions.onClick.AddListener(() => SoundManager.Instance?.PlaySound("ButtonClick"));
            buttonOptions.onClick.AddListener(() => ShowOptions());

        }

        if (buttonCredits != null)
        {
            buttonCredits.onClick.RemoveAllListeners();
            buttonCredits.onClick.AddListener(() => SoundManager.Instance?.PlaySound("ButtonClick"));
            buttonCredits.onClick.AddListener(() => ShowCredits());

        }

        if (buttonQuit != null)
        {
            buttonQuit.onClick.RemoveAllListeners();
            buttonQuit.onClick.AddListener(() => SoundManager.Instance?.PlaySound("ButtonClick"));
            buttonQuit.onClick.AddListener(() => CustomSceneManager.Instance?.ExitGame());
        }

        if (buttonOptionsBack != null)
        {
            buttonOptionsBack.onClick.RemoveAllListeners();
            buttonOptionsBack.onClick.AddListener(() => SoundManager.Instance?.PlaySound("ButtonClick"));
            buttonOptionsBack.onClick.AddListener(() => ShowMainMenu());
        }

        if (buttonCreditsBack != null)
        {
            buttonCreditsBack.onClick.RemoveAllListeners();
            buttonCreditsBack.onClick.AddListener(() => SoundManager.Instance?.PlaySound("ButtonClick"));
            buttonCreditsBack.onClick.AddListener(() => ShowMainMenu());
        }

        
    }


    public void ShowMainMenu()
    {
        mainMenuScreen.SetActive(true);
        optionsScreen.SetActive(false);
        creditsScreen.SetActive(false);
    }

    public void ShowOptions()
    {
        mainMenuScreen.SetActive(false);
        optionsScreen.SetActive(true);
        creditsScreen.SetActive(false);

    }

    public void ShowCredits()
    {
        mainMenuScreen.SetActive(false);
        optionsScreen.SetActive(false);
        creditsScreen.SetActive(true);

    }


    public void QuitGame()
    {
        Application.Quit();

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.ExitPlaymode();
        #endif
    }



}