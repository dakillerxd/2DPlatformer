using CustomAttribute;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class MainMenu : MonoBehaviour
{
    [Header("Screens")] 
    [SerializeField] private GameObject mainMenuPosition;
    [SerializeField] private GameObject levelSelectMenuPosition;
    [SerializeField] private GameObject collectiblesMenuPosition;
    [SerializeField] private GameObject optionsMenuPosition;
    [SerializeField] private GameObject creditsMenuPosition;

    [Header("Button")]
    [SerializeField] private Button buttonStart;
    [SerializeField] private Button buttonResume;
    [SerializeField] private Button buttonLevelSelect;
    [SerializeField] private Button buttonCollectibles;
    [SerializeField] private Button buttonOptions;
    [SerializeField] private Button buttonCredits;
    [SerializeField] private Button buttonQuit;
    
    
    private void Start()
    {
        SetupButtons();
        CameraController.Instance?.SetTarget(mainMenuPosition.transform);
    }
    
    

    private void SetupButtons()
    {
        
        if (buttonResume != null)
        {
            if (SaveManager.Instance.LoadString("SavedLevel") != "")
            {
                buttonResume.gameObject.SetActive(true);
                buttonResume.onClick.AddListener(() => SoundManager.Instance?.PlaySoundFX("ButtonClick"));
                buttonResume.onClick.AddListener(() => GameManager.Instance.SetGameState(GameStates.GamePlay));
                buttonResume.onClick.AddListener(() => CameraController.Instance?.SetTarget(mainMenuPosition.transform));
                buttonResume.onClick.AddListener(() => SceneManager.LoadScene(SaveManager.Instance.LoadString("SavedLevel")));
            } else {
                buttonResume.gameObject.SetActive(false);
            }

        }
        
        
        if (buttonStart != null)
        {
            buttonStart.onClick.AddListener(() => SoundManager.Instance?.PlaySoundFX("ButtonClick"));
            buttonStart.onClick.AddListener(() => SaveManager.Instance.SaveInt("SavedCheckpoint", 0));
            buttonStart.onClick.AddListener(() => GameManager.Instance.SetGameState(GameStates.GamePlay));
            buttonStart.onClick.AddListener(() => SceneManager.LoadScene(1));
        }
        
        if (buttonLevelSelect != null)
        {
            buttonLevelSelect.onClick.AddListener(() => SoundManager.Instance?.PlaySoundFX("ButtonClick"));
            buttonLevelSelect.onClick.AddListener(() => CameraController.Instance?.SetTarget(levelSelectMenuPosition.transform));
        }
        
        if (buttonCollectibles != null)
        {
            buttonCollectibles.onClick.AddListener(() => SoundManager.Instance?.PlaySoundFX("ButtonClick"));
            buttonCollectibles.onClick.AddListener(() => CameraController.Instance?.SetTarget(collectiblesMenuPosition.transform));
        }

        if (buttonOptions != null)
        {
            buttonOptions.onClick.AddListener(() => SoundManager.Instance?.PlaySoundFX("ButtonClick"));
            buttonOptions.onClick.AddListener(() => CameraController.Instance?.SetTarget(optionsMenuPosition.transform));

        }

        if (buttonCredits != null)
        {
            buttonCredits.onClick.AddListener(() => SoundManager.Instance?.PlaySoundFX("ButtonClick"));
            buttonCredits.onClick.AddListener(() => CameraController.Instance?.SetTarget(creditsMenuPosition.transform));

        }

        if (buttonQuit != null)
        {
            buttonQuit.onClick.AddListener(() => SoundManager.Instance?.PlaySoundFX("ButtonClick"));
            buttonQuit.onClick.AddListener(() => GameManager.Instance.QuitGame());
        }
    }
    
    
    
    
}