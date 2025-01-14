using TMPro;
using UnityEngine;
using System.Text;
using UnityEngine.SceneManagement;


public class GameUIManager : MenuController
{   
    public static GameUIManager Instance { get; private set; }
    
    [Header("Categories")]
    [SerializeField] private MenuCategoryGameplay gameplayCategory;
    [SerializeField] private MenuCategoryPause pauseCategory;
    
    [Header("Debug")]
    [SerializeField] private TextMeshProUGUI playerDebugText;
    [SerializeField] private TextMeshProUGUI cameraDebugText;
    public TextMeshProUGUI fpsText;
    
    

    protected override void Awake() {
        base.Awake();
        
        if (Instance != null && Instance != this) 
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            DisableAllCategories();
        }
    }
    
    
    protected override void Update()
    {
        if (InputManager.NavigateUI != Vector2.zero || currentCategory)
        {
            OnNavigate(InputManager.NavigateUI);
        }
        
        UpdateDebugUI();
    }
    
    protected override void OnActiveSceneChanged(Scene currentScene, Scene nextScene)
    {
        if (nextScene.name == "MainMenu")
        {
            DisableAllCategories();
        } else {
            SelectFirstCategory();
            currentCategory.OnActiveSceneChanged(currentScene, nextScene);
        }
        
        ToggleDebugUI(GameManager.Instance.debugMode);
    }
    
    protected override void OnGameStateChange(GameStates state)
    {
        if (state == GameStates.Paused)
        {
            SelectCategory(pauseCategory);

        }
        else if (state == GameStates.GamePlay)
        {
            SelectCategory(gameplayCategory);
        }
        
    }
    
    

#region Debug UI

    public void ToggleDebugUI(bool state) {
        
        playerDebugText.enabled = state;
        cameraDebugText.enabled = state;
        fpsText.enabled = state;
    }
    
    private void UpdateDebugUI()
    {
        if (!GameManager.Instance.debugMode) return;
        
        if (playerDebugText && PlayerController.Instance)
        {
            PlayerController.Instance.UpdateDebugText(playerDebugText);
        }

        if (cameraDebugText && CameraController.Instance)
        {
            CameraController.Instance.UpdateDebugText(cameraDebugText);
        }

        if (fpsText)
        {
            UpdateFpsText();
        }
    }
    
    
    private readonly StringBuilder fpsStringBuilder = new StringBuilder(256);
    private void UpdateFpsText() {

        fpsStringBuilder.Clear();

        float deltaTime = 0.0f;
        deltaTime += Time.unscaledDeltaTime - deltaTime;
        float fps = 1.0f / deltaTime;

        fpsStringBuilder.AppendFormat("FPS: {0}\n", (int)fps);
        fpsStringBuilder.AppendFormat("VSync: {0}\n", QualitySettings.vSyncCount);

        fpsText.text = fpsStringBuilder.ToString();
    }

#endregion


}