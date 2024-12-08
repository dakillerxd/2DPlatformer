using CustomAttribute;
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
            buttonShowcaseLevel.onClick.AddListener(() => GameManager.Instance.SetGameState(GameStates.GamePlay));
            buttonShowcaseLevel.onClick.AddListener(() => SceneManager.LoadScene("Showcase Level"));
        }
        
        if (buttonTestLevel != null)
        {
            buttonTestLevel.onClick.AddListener(() => SoundManager.Instance?.PlaySoundFX("ButtonClick"));
            buttonTestLevel.onClick.AddListener(() => GameManager.Instance.SetGameState(GameStates.GamePlay));
            buttonTestLevel.onClick.AddListener(() => SceneManager.LoadScene("Test Level"));
        }

        if (levels.Length == 0 || !buttonLevelPrefab || !levelsContainer) return;
        
        
        foreach (SceneField level in levels)
        {
            GameObject button = Instantiate(buttonLevelPrefab.gameObject, levelsContainer.transform);
            button.GetComponent<Button>().onClick.AddListener(() => SoundManager.Instance?.PlaySoundFX("ButtonClick"));
            button.GetComponent<Button>().onClick.AddListener(() => GameManager.Instance.SetGameState(GameStates.GamePlay));
            button.GetComponent<Button>().onClick.AddListener(() => SceneManager.LoadScene(level));
            button.GetComponentInChildren<TextMeshProUGUI>().text = level.SceneName.Replace("Level", "").Trim();
            
        }
    }


}
