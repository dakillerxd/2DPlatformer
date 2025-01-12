using System.Text;
using CustomAttributes;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuPageLevelSelection : MenuPage
{
    
    [Header("UI Elements")]
    [SerializeField] private Button buttonBack;
    [SerializeField] private Button buttonShowcaseLevel;
    [SerializeField] private Button buttonTestLevel;
    [SerializeField] private Button buttonLevelPrefab;
    [SerializeField] private GameObject levelsContainer;
    [SerializeField] private TextMeshProUGUI selectedLevelInfo;
    
    
    
    private void Start()
    {
        SetupButtons();
        SetupLevelButtons();
    }
    
    protected override void OnEnable()
    {
        base.OnEnable();
        if (selectedLevelInfo) selectedLevelInfo.alpha = 0f;

    }

    protected override void OnSelect(BaseEventData eventData)
    {
        base.OnSelect(eventData);
    
        if (eventData.selectedObject != null)
        {
            Button selectedButton = eventData.selectedObject.GetComponent<Button>();
            if (selectedButton != null)
            {
                // Get the level index from the button's text
                TextMeshProUGUI buttonText = selectedButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null && int.TryParse(buttonText.text, out int levelNumber))
                {
                    // Find the corresponding level in GameManager
                    // Subtract 1 because array is 0-based but level numbers start at 1
                    int levelIndex = levelNumber - 1;
                    if (levelIndex >= 0 && levelIndex < GameManager.Instance.levels.Length)
                    {
                        Level selectedLevel = GameManager.Instance.levels[levelIndex];
                        ShowSelectedLevelInfo(selectedLevel);
                    }
                }
            }
        }
    }
    
    protected override void OnDeselect(BaseEventData eventData)
    {
        base.OnDeselect(eventData);
        HideSelectedLevelInfo();
    }
    
    

    private void SetupButtons()
    {
        if (buttonBack)
        {
            buttonBack.onClick.AddListener(() => SoundManager.Instance?.PlaySoundFX("ButtonClick"));
            buttonBack.onClick.AddListener(() => _menuCategoryMain.SelectPage(_menuCategoryMain.mainMenuPage));
        }
        if (buttonShowcaseLevel)
        {
            buttonShowcaseLevel.onClick.AddListener(() => SoundManager.Instance?.PlaySoundFX("ButtonClick"));
            buttonShowcaseLevel.onClick.AddListener(() => SaveManager.Instance?.SaveInt("SavedCheckpoint", 0));
            buttonShowcaseLevel.onClick.AddListener(() => GameManager.Instance?.SetGameState(GameStates.GamePlay));
            buttonShowcaseLevel.onClick.AddListener(() => SceneManager.LoadScene("ShowcaseLevel"));
        }
        
        if (buttonTestLevel)
        {
            buttonTestLevel.onClick.AddListener(() => SoundManager.Instance?.PlaySoundFX("ButtonClick"));
            buttonTestLevel.onClick.AddListener(() => GameManager.Instance?.SetGameState(GameStates.GamePlay));
            buttonTestLevel.onClick.AddListener(() => SceneManager.LoadScene("TestLevel"));
        }
    }

    private void SetupLevelButtons()
    {
        if (GameManager.Instance.levels.Length == 0 || !buttonLevelPrefab || !levelsContainer) return;
        
        
        foreach (Transform child in levelsContainer.transform) // Delete all buttons before creating new ones
        {
            Destroy(child.gameObject);
        }
        
        foreach (Level level in GameManager.Instance.levels) // Create a button for each level in the list
        {
            GameObject buttonObject = Instantiate(buttonLevelPrefab.gameObject, levelsContainer.transform);
            Button button = buttonObject.GetComponent<Button>();
            selectables.Add(button);
            SetupSelectable(button);
            StoreOriginalTransforms(button);
            button.interactable = level.connectedScene.BuildIndex <= SaveManager.Instance.LoadInt("HighestLevel");
            button.onClick.AddListener(() => SoundManager.Instance?.PlaySoundFX("ButtonClick"));
            button.onClick.AddListener(() => SaveManager.Instance?.SaveInt("SavedCheckpoint", 0));
            button.onClick.AddListener(() => GameManager.Instance?.SetGameState(GameStates.GamePlay));
            button.onClick.AddListener(() => SceneManager.LoadScene(level.connectedScene.BuildIndex));
            buttonObject.GetComponentInChildren<TextMeshProUGUI>().text = level.name.Replace("Level ", "");
        }
    }
    
    private void HideSelectedLevelInfo()
    {
        if (selectedLevelInfo)
        {
            Tween.Alpha(selectedLevelInfo, startValue:1f, endValue: 0f, duration: 1f);
        }
    }

    private void ShowSelectedLevelInfo(Level level)
    {
        if (selectedLevelInfo)
        {
            Tween.Alpha(selectedLevelInfo, startValue: 0f, endValue: 1f, duration: 1f);

            // Get saved data for the level
            bool collectibleCollected = level.collectibleCollected;
            string formattedTime = FormatTime(level.bestTime);
            string formattedDeaths = FormatDeaths(level.totalDeaths);

            selectedLevelInfo.text = 
                $"<color=#00FF00>{level.name}</color>\n" +
                $"Collectible: {(collectibleCollected ? 1 : 0)}/1\n" +
                $"Total Deaths: {formattedDeaths}\n" +
                $"Best Time: {formattedTime}";
        }
    }
    
    private string FormatTime(float timeInSeconds)
    {
        if (timeInSeconds <= 0) return "---";
    
        int minutes = Mathf.FloorToInt(timeInSeconds / 60);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60);
        return $"{minutes:00}:{seconds:00}";
    }
    
    private string FormatDeaths(int deaths)
    {
        if (deaths <= 0) return "---";
        
        return deaths.ToString();
    }
    


}
