using CustomAttribute;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class MenuPageMain : MenuPage
{

    [Header("UI Elements")]
    [SerializeField] private Button buttonStart;
    [SerializeField] private Button buttonLevelSelect;
    [SerializeField] private Button buttonCollectibles;
    [SerializeField] private Button buttonOptions;
    [SerializeField] private Button buttonCredits;
    [SerializeField] private Button buttonQuit;
    
    
    
    protected override void Start()
    {
        SetupButtons();
        base.Start();
    }


    
    private void SetupButtons()
    {
        if (buttonStart)
        {
            buttonStart.onClick.RemoveAllListeners();
            buttonStart.onClick.AddListener(() => SoundManager.Instance?.PlaySoundFX("ButtonClick"));
            
            var buttonText = buttonStart.GetComponentInChildren<TextMeshProUGUI>();
            int highestLevel = SaveManager.LoadInt("HighestLevel");
            int savedCheckpoint = SaveManager.LoadInt("SavedCheckpoint");
            string savedLevel = SaveManager.LoadString("SavedLevel");
            SceneField level1 = GameManager.Instance.levels[0].connectedScene;
            
            // Check save
            if (highestLevel <= 1) // At level 1
            {
                buttonStart.onClick.AddListener(() => SceneManager.LoadScene(level1.BuildIndex));
                buttonText.text = savedCheckpoint > 0 ? "Resume" : "Start";
                
            } else { // At higher level then 1

                if (savedLevel == level1) // if there is the last level was level 1
                {
                    buttonStart.onClick.AddListener(() => SceneManager.LoadScene(level1.BuildIndex));
                    buttonText.text = savedCheckpoint > 0 ? "Resume" : "Start";
                    
                } else {
                    buttonStart.onClick.AddListener(() => SceneManager.LoadScene(savedLevel));
                    buttonText.text = "Resume";
                }
            }
        }
        
        
        if (buttonLevelSelect)
        {
            buttonLevelSelect.onClick.RemoveAllListeners();
            buttonLevelSelect.onClick.AddListener(() => SoundManager.Instance?.PlaySoundFX("ButtonClick"));
            buttonLevelSelect.onClick.AddListener(() => menuCategoryMain.SelectPage(menuCategoryMain.levelSelectMenuPage));
        }
        
        if (buttonCollectibles)
        {
            buttonCollectibles.onClick.RemoveAllListeners();
            buttonCollectibles.onClick.AddListener(() => SoundManager.Instance?.PlaySoundFX("ButtonClick"));
            buttonCollectibles.onClick.AddListener(() => menuCategoryMain.SelectPage(menuCategoryMain.collectiblesMenuPage));
        }

        if (buttonOptions)
        {
            buttonOptions.onClick.RemoveAllListeners();
            buttonOptions.onClick.AddListener(() => SoundManager.Instance?.PlaySoundFX("ButtonClick"));
            buttonOptions.onClick.AddListener(() => menuCategoryMain.SelectPage(menuCategoryMain.optionsMenuPage));
        }

        if (buttonCredits)
        {
            buttonCredits.onClick.RemoveAllListeners();
            buttonCredits.onClick.AddListener(() => SoundManager.Instance?.PlaySoundFX("ButtonClick"));
            buttonCredits.onClick.AddListener(() => menuCategoryMain.SelectPage(menuCategoryMain.creditsMenuPage));
        }

        if (buttonQuit)
        {
            buttonQuit.onClick.RemoveAllListeners();
            buttonQuit.onClick.AddListener(() => SoundManager.Instance?.PlaySoundFX("ButtonClick"));
            buttonQuit.onClick.AddListener(() => GameManager.Instance?.QuitGame());
        }
    }
}