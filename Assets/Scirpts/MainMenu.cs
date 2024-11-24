using CustomAttribute;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class MainMenuController : MonoBehaviour
{
    [Header("Screens")] 
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject levelSelectMenu;
    [SerializeField] private GameObject optionsMenu;
    [SerializeField] private GameObject creditsMenu;

    [Header("Main Menu")]
    [SerializeField] private Button buttonStart;
    [SerializeField] private Button buttonLevelSelect;
    [SerializeField] private Button buttonOptions;
    [SerializeField] private Button buttonCredits;
    [SerializeField] private Button buttonQuit;
    
    [Header("Options Menu")]
    [SerializeField] private Button buttonOptionsBack;

    
    private void Start()
    {
        CameraController.Instance?.SetTarget(mainMenu.transform);
        SetupMainMenu();
        SetupOptionsMenu();
    }
    
    

    private void SetupMainMenu()
    {
        if (buttonStart != null)
        {
            buttonStart.onClick.AddListener(() => SoundManager.Instance?.PlaySoundFX("ButtonClick"));
            buttonStart.onClick.AddListener(() => SceneManager.LoadScene(1));
        }
        
        if (buttonLevelSelect != null)
        {
            buttonLevelSelect.onClick.AddListener(() => SoundManager.Instance?.PlaySoundFX("ButtonClick"));
            buttonLevelSelect.onClick.AddListener(() => CameraController.Instance?.SetTarget(levelSelectMenu.transform));
        }

        if (buttonOptions != null)
        {
            buttonOptions.onClick.AddListener(() => SoundManager.Instance?.PlaySoundFX("ButtonClick"));
            buttonOptions.onClick.AddListener(() => CameraController.Instance?.SetTarget(optionsMenu.transform));

        }

        if (buttonCredits != null)
        {
            buttonCredits.onClick.AddListener(() => SoundManager.Instance?.PlaySoundFX("ButtonClick"));
            buttonCredits.onClick.AddListener(() => CameraController.Instance?.SetTarget(creditsMenu.transform));

        }

        if (buttonQuit != null)
        {
            buttonQuit.onClick.AddListener(() => SoundManager.Instance?.PlaySoundFX("ButtonClick"));
            buttonQuit.onClick.AddListener(() => QuitGame());
        }
    }
    
    
    
    private void SetupOptionsMenu()
    {
        if (buttonOptionsBack != null)
        {
            buttonOptionsBack.onClick.AddListener(() => SoundManager.Instance?.PlaySoundFX("ButtonClick"));
            buttonOptionsBack.onClick.AddListener(() => CameraController.Instance?.SetTarget(mainMenu.transform));
        }
    }
    

    private void QuitGame()
    {
        Application.Quit();

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.ExitPlaymode();
        #endif
    }
    
}