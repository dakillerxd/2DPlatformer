using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuPagePause : MenuPage
{
    
    [Header("UI Elements")]
    [SerializeField] private Button buttonResume;
    [SerializeField] private Button buttonOptions;
    [SerializeField] private Button buttonMainMenu;
    [SerializeField] private Button buttonQuit;
    [SerializeField] private  TMP_Text pauseCollectiblesText;
    

    private void Start()
    {
        SetupButtons();
        UpdatePauseScreenInfo();
    }


    private void UpdatePauseScreenInfo()
    {
        if (pauseCollectiblesText)
        {
            pauseCollectiblesText.text = GameManager.Instance.IsCollectibleCollected(SceneManager.GetActiveScene().name) ? "1/1" : "0/1";
        }
        
    }
    
    private void SetupButtons()
    {
        
        
        if (buttonResume)
        {
            buttonResume.onClick.RemoveAllListeners();
            buttonResume.onClick.AddListener(() => GameManager.Instance?.SetGameState(GameStates.GamePlay));
            buttonResume.onClick.AddListener(() => SoundManager.Instance?.PlaySoundFX("ButtonClick"));
        }

        if (buttonOptions)
        {
            buttonOptions.onClick.RemoveAllListeners();
            buttonOptions.onClick.AddListener(() => SoundManager.Instance?.PlaySoundFX("ButtonClick"));
            buttonOptions.onClick.AddListener(() => _menuCategoryPause.SelectPage(_menuCategoryPause.optionsMenuPage));
        }
        
        if (buttonMainMenu)
        {
            buttonMainMenu.onClick.RemoveAllListeners();
            buttonMainMenu.onClick.AddListener(() => GameManager.Instance?.SetGameState(GameStates.None));
            buttonMainMenu.onClick.AddListener(() => SoundManager.Instance?.PlaySoundFX("ButtonClick"));
            buttonMainMenu.onClick.AddListener(() => SceneManager.LoadScene(0));
            
        }

        if (buttonQuit)
        {
            buttonQuit.onClick.RemoveAllListeners();
            buttonQuit.onClick.AddListener(() => SoundManager.Instance?.PlaySoundFX("ButtonClick"));
            buttonQuit.onClick.AddListener(() => GameManager.Instance?.QuitGame());
        }
        
    }
    
}
