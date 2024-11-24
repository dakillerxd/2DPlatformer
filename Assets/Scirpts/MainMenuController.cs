using CustomAttribute;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class MainMenuController : MonoBehaviour
{
    [Header("General")] 
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
    
    [Header("Level Select Menu")]
    [SerializeField] private Button buttonLevelSelectBack;
    [SerializeField] private Button buttonLevelPrefab;
    [SerializeField] private GameObject levelsContainer;
    [SerializeField] private SceneField[] levels;
    
    [Header("Options Menu")]
    [SerializeField] private Button buttonOptionsBack;

    
    private void Start()
    {
        CameraController.Instance?.SetTarget(mainMenu.transform);
        SetupMainMenu();
        SetupLevelSelect();
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
    

    private void SetupLevelSelect()
    {
        if (buttonLevelSelectBack != null)
        {
            buttonLevelSelectBack.onClick.AddListener(() => SoundManager.Instance?.PlaySoundFX("ButtonClick"));
            buttonLevelSelectBack.onClick.AddListener(() => CameraController.Instance?.SetTarget(mainMenu.transform));
        }

        if (levels.Length == 0 || !buttonLevelPrefab || !levelsContainer) return;
        
        
        
        
        foreach (SceneField level in levels)
        {
            GameObject button = Instantiate(buttonLevelPrefab.gameObject, levelsContainer.transform);
            button.GetComponent<Button>().onClick.AddListener(() => SoundManager.Instance?.PlaySoundFX("ButtonClick"));
            button.GetComponent<Button>().onClick.AddListener(() => SceneManager.LoadScene(level));
            button.GetComponentInChildren<TextMeshProUGUI>().text = level.SceneName.Replace("Level", "").Trim();
            
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