using CustomAttribute;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelSelectionMenu : MonoBehaviour
{
    
    [Header("Buttons")]
    [SerializeField] private Button buttonLevelSelectBack;
    [SerializeField] private Button buttonLevelPrefab;
    [SerializeField] private GameObject levelsContainer;
    [SerializeField] private SceneField[] levels;
    
    [Header("References")]
    [SerializeField] private GameObject mainMenuPosition;
    

    void Start()
    {
        if (buttonLevelSelectBack != null)
        {
            buttonLevelSelectBack.onClick.AddListener(() => SoundManager.Instance?.PlaySoundFX("ButtonClick"));
            buttonLevelSelectBack.onClick.AddListener(() => CameraController.Instance?.SetTarget(mainMenuPosition.transform));
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
