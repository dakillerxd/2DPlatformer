using CustomAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuPageLevelSelection : MenuPage
{
    
    [Header("Buttons")]
    [SerializeField] private Button buttonBack;
    [SerializeField] private Button buttonShowcaseLevel;
    [SerializeField] private Button buttonTestLevel;
    [SerializeField] private Button buttonLevelPrefab;
    
    [Header("Levels")]
    [SerializeField] private GameObject levelsContainer;
    [SerializeField] private SceneField[] levels;
    
    private  MenuCategoryMainMenu _menuCategoryMain;
    
    
    
    private void Start()
    {
        _menuCategoryMain = GetComponentInParent<MenuCategoryMainMenu>();
        
        
        SetupButtons();
        SetupLevelButtons();

    }

    private void SetupButtons()
    {
        if (buttonBack != null)
        {
            buttonBack.onClick.AddListener(() => SoundManager.Instance?.PlaySoundFX("ButtonClick"));
            buttonBack.onClick.AddListener(() => _menuCategory.SelectPage(_menuCategoryMain.mainMenuPage));
        }
        if (buttonShowcaseLevel != null)
        {
            buttonShowcaseLevel.onClick.AddListener(() => SoundManager.Instance?.PlaySoundFX("ButtonClick"));
            buttonShowcaseLevel.onClick.AddListener(() => SaveManager.Instance?.SaveInt("SavedCheckpoint", 0));
            buttonShowcaseLevel.onClick.AddListener(() => GameManager.Instance?.SetGameState(GameStates.GamePlay));
            buttonShowcaseLevel.onClick.AddListener(() => SceneManager.LoadScene("ShowcaseLevel"));
        }
        
        if (buttonTestLevel != null)
        {
            buttonTestLevel.onClick.AddListener(() => SoundManager.Instance?.PlaySoundFX("ButtonClick"));
            buttonTestLevel.onClick.AddListener(() => GameManager.Instance?.SetGameState(GameStates.GamePlay));
            buttonTestLevel.onClick.AddListener(() => SceneManager.LoadScene("TestLevel"));
        }
    }

    private void SetupLevelButtons()
    {
        if (levels.Length == 0 || !buttonLevelPrefab || !levelsContainer) return;
        
        
        foreach (Transform child in levelsContainer.transform) // Delete all buttons before creating new ones
        {
            Destroy(child.gameObject);
        }
        
        foreach (SceneField level in levels) // Create a button for each level in the list
        {
            GameObject buttonObject = Instantiate(buttonLevelPrefab.gameObject, levelsContainer.transform);
            Button button = buttonObject.GetComponent<Button>();
            selectables.Add(button);
            SetupSelectable(button);
            StoreOriginalTransforms(button);
            button.interactable = level.BuildIndex <= SaveManager.Instance.LoadInt("HighestLevel");
            button.onClick.AddListener(() => SoundManager.Instance?.PlaySoundFX("ButtonClick"));
            button.onClick.AddListener(() => SaveManager.Instance?.SaveInt("SavedCheckpoint", 0));
            button.onClick.AddListener(() => GameManager.Instance?.SetGameState(GameStates.GamePlay));
            button.onClick.AddListener(() => SceneManager.LoadScene(level.SceneName));
            buttonObject.GetComponentInChildren<TextMeshProUGUI>().text = level.SceneName.Replace("Level", "").Trim();
            

        }
    }


}
