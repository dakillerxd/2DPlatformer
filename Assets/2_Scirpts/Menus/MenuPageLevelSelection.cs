using CustomAttribute;
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
    
    
    
    protected override void Start()
    {
        
        SetupButtons();
        SetupLevelButtons();
        if (selectedLevelInfo) selectedLevelInfo.alpha = 0f;
        
        base.Start();
    }
    

    protected override void OnSelect(BaseEventData eventData)
    {
        base.OnSelect(eventData);
        
        if (eventData.selectedObject)
        {
            Button selectedButton = eventData.selectedObject.GetComponent<Button>();
            if (selectedButton && selectedButton.interactable)
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
        
        if (eventData.selectedObject)
        {
            Button selectedButton = eventData.selectedObject.GetComponent<Button>();
            if (selectedButton&& selectedButton.interactable)
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
                        HideSelectedLevelInfo();
                    }
                }
            }
        }
        
    }
    
    
    private void SetupButtons()
    {
        if (buttonBack)
        {
            buttonBack.onClick.RemoveAllListeners();
            buttonBack.onClick.AddListener(() => SoundManager.Instance?.PlaySoundFX("ButtonClick"));
            buttonBack.onClick.AddListener(() => menuCategoryMain.SelectPage(menuCategoryMain.mainMenuPage));
        }
        if (buttonShowcaseLevel)
        {
            buttonShowcaseLevel.onClick.RemoveAllListeners();
            buttonShowcaseLevel.onClick.AddListener(() => SoundManager.Instance?.PlaySoundFX("ButtonClick"));
            buttonShowcaseLevel.onClick.AddListener(() => SaveManager.SaveInt("SavedCheckpoint", 0));
            buttonShowcaseLevel.onClick.AddListener(() => GameManager.Instance?.SetGameState(GameStates.GamePlay));
            buttonShowcaseLevel.onClick.AddListener(() => SceneManager.LoadScene("ShowcaseLevel"));
        }
        
        if (buttonTestLevel)
        {
            buttonTestLevel.onClick.RemoveAllListeners();
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
    
        for (int i = 0; i < GameManager.Instance.levels.Length; i++) // Using index loop instead of foreach
        {
            Level level = GameManager.Instance.levels[i];
            GameObject buttonObject = Instantiate(buttonLevelPrefab.gameObject, levelsContainer.transform);
            Button button = buttonObject.GetComponent<Button>();
            
            buttonObject.GetComponentInChildren<TextMeshProUGUI>().text = level.name.Replace("Level ", "");
            buttonObject.name = "ButtonLevel" + level.name;
            button.interactable = (i +1) <= SaveManager.LoadInt("HighestLevel");
            button.onClick.AddListener(() => SoundManager.Instance?.PlaySoundFX("ButtonClick"));
            button.onClick.AddListener(() => SaveManager.SaveInt("SavedCheckpoint", 0));
            button.onClick.AddListener(() => GameManager.Instance?.SetGameState(GameStates.GamePlay));
            button.onClick.AddListener(() => SceneManager.LoadScene(level.connectedScene.BuildIndex));
            
            selectables.Add(button);
            SetupSelectable(button);
        }
    }
    
    private void HideSelectedLevelInfo()
    {
        if (selectedLevelInfo && selectedLevelInfo.alpha > 0)
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
            bool noDeathRun = level.noDeathRunRun;
            string formattedTime = FormatTime(level.bestTime);
            string formattedDeaths = FormatDeaths(level.totalDeaths);

            selectedLevelInfo.text = 
                $"<!show><jump><b><color=#00FF00>{level.name}</color><b></!show></>\n" +
                $"Collectible: {(collectibleCollected ? 1 : 0)}/1\n" +
                $"No Death Run: {(noDeathRun ? "Yes" : "No")}\n" +
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
