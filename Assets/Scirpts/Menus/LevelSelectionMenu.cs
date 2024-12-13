using CustomAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelSelectionMenu : MonoBehaviour
{
    
    [Header("References")]
    [SerializeField] private GameObject mainMenuPosition;
    [SerializeField] private Button buttonLevelSelectBack;
    [SerializeField] private Button buttonShowcaseLevel;
    [SerializeField] private Button buttonTestLevel;
    [SerializeField] private Button buttonLevelPrefab;
    [SerializeField] private GameObject levelsContainer;
    [SerializeField] private SceneField[] levels;
    

    
    
    private void Start()
    {
        if (buttonLevelSelectBack != null)
        {
            buttonLevelSelectBack.onClick.AddListener(() => SoundManager.Instance?.PlaySoundFX("ButtonClick"));
            buttonLevelSelectBack.onClick.AddListener(() => CameraController.Instance?.SetTarget(mainMenuPosition.transform));
        }
        if (buttonShowcaseLevel != null)
        {
            buttonShowcaseLevel.onClick.AddListener(() => SoundManager.Instance?.PlaySoundFX("ButtonClick"));
            buttonShowcaseLevel.onClick.AddListener(() => SaveManager.Instance.SaveInt("SavedCheckpoint", 0));
            buttonShowcaseLevel.onClick.AddListener(() => GameManager.Instance.SetGameState(GameStates.GamePlay));
            buttonShowcaseLevel.onClick.AddListener(() => SceneManager.LoadScene("Showcase Level"));
        }
        
        if (buttonTestLevel != null)
        {
            buttonTestLevel.onClick.AddListener(() => SoundManager.Instance?.PlaySoundFX("ButtonClick"));
            buttonTestLevel.onClick.AddListener(() => GameManager.Instance.SetGameState(GameStates.GamePlay));
            buttonTestLevel.onClick.AddListener(() => SceneManager.LoadScene("Test Level"));
        }
        

        UpdateLevelButton();

    }

    private void UpdateLevelButton()
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
            button.interactable = level.BuildIndex <= SaveManager.Instance.LoadInt("HighestLevel");
            button.onClick.AddListener(() => SoundManager.Instance?.PlaySoundFX("ButtonClick"));
            button.onClick.AddListener(() => SaveManager.Instance.SaveInt("SavedCheckpoint", 0));
            button.onClick.AddListener(() => GameManager.Instance.SetGameState(GameStates.GamePlay));
            button.onClick.AddListener(() => SceneManager.LoadScene(level.SceneName));
            buttonObject.GetComponentInChildren<TextMeshProUGUI>().text = level.SceneName.Replace("Level", "").Trim();
            
        }
    }


}
