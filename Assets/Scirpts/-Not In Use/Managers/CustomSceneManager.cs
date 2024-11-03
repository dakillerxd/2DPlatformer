using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CustomSceneManager : MonoBehaviour
{
    public static CustomSceneManager Instance { get; private set; }
    
    [Header("Load Screen Settings")]
    [SerializeField] private string loadingSceneName = "LoadingScene";
    [SerializeField] private float minimumLoadingTime = 5f;
    [SerializeField] private float tipChangeInterval = 3f;
    [SerializeField] private List<string> tips = new List<string>
    {
        "Tip 1: ",
        "Tip 2: ",
        "Tip 3: ",
        "Tip 4: ",
        "Tip 5: "
    };

    private Image loadingBarFill;
    private TextMeshProUGUI loadingPercentageText;
    private TextMeshProUGUI tipText;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public string GetActiveSceneName()
    {
        return SceneManager.GetActiveScene().name;
    }

    public int GetActiveSceneIndex()
    {
        return SceneManager.GetActiveScene().buildIndex;
    }

    public void LoadNextScene(bool useLoadingScreen = true)
    {
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;

        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            LoadScene(nextSceneIndex, useLoadingScreen);
        }
        else
        {
            Debug.LogWarning("No next scene available.");
        }
    }

    public void LoadScene(int sceneIndex, bool useLoadingScreen = true)
    {
        if (sceneIndex >= 0 && sceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            if (useLoadingScreen)
            {
                StartCoroutine(LoadSceneRoutine(sceneIndex));
            }
            else
            {
                SceneManager.LoadScene(sceneIndex);
            }
        }
        else
        {
            Debug.LogError($"Scene index {sceneIndex} is out of range.");
        }
    }

    public void LoadScene(string sceneName, bool useLoadingScreen = true)
    {
        if (Application.CanStreamedLevelBeLoaded(sceneName))
        {
            if (useLoadingScreen)
            {
                StartCoroutine(LoadSceneRoutine(sceneName));
            }
            else
            {
                SceneManager.LoadScene(sceneName);
            }
        }
        else
        {
            Debug.LogError($"Scene '{sceneName}' does not exist or is not enabled in the build settings.");
        }
    }

    public void ResetScene(bool useLoadingScreen = true)
    {
        LoadScene(SceneManager.GetActiveScene().buildIndex, useLoadingScreen);
    }

    public void ExitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    private IEnumerator LoadSceneRoutine(int sceneIndex)
    {
        yield return LoadSceneRoutineInternal(SceneManager.LoadSceneAsync(sceneIndex));
    }

    private IEnumerator LoadSceneRoutine(string sceneName)
    {
        yield return LoadSceneRoutineInternal(SceneManager.LoadSceneAsync(sceneName));
    }

    private IEnumerator LoadSceneRoutineInternal(AsyncOperation asyncLoad)
    {
        // Load the loading scene
        yield return SceneManager.LoadSceneAsync(loadingSceneName);

        // Find UI elements in the loading scene
        FindUIElements();

        asyncLoad.allowSceneActivation = false;

        float elapsedTime = 0f;
        float progress = 0f;
        float nextTipChange = 0f;

        while (!asyncLoad.isDone)
        {
            elapsedTime += Time.deltaTime;
            
            // Calculate progress based on both asyncLoad.progress and elapsed time
            float asyncProgress = asyncLoad.progress / 0.9f; // AsyncOperation goes to 0.9 when loading is done
            float timeProgress = elapsedTime / minimumLoadingTime;
            progress = Mathf.Clamp01(Mathf.Max(asyncProgress, timeProgress));

            UpdateLoadingUI(progress);

            // Change tip if it's time
            if (elapsedTime >= nextTipChange)
            {
                DisplayRandomTip();
                nextTipChange = elapsedTime + tipChangeInterval;
            }

            // Check if it's time to activate the scene
            if (asyncLoad.progress >= 0.9f && elapsedTime >= (minimumLoadingTime + 0.5f))
            {
                asyncLoad.allowSceneActivation = true;
            }

            yield return null;
        }

        Debug.Log("Scene loading completed");
    }



    private void FindUIElements()
    {
        loadingBarFill = GameObject.Find("LoadingBar")?.GetComponent<Image>();
        loadingPercentageText = GameObject.Find("PercentageText")?.GetComponent<TextMeshProUGUI>();
        tipText = GameObject.Find("TipsText")?.GetComponent<TextMeshProUGUI>();

        if (loadingBarFill == null)
            Debug.LogWarning("LoadingBar not found in the loading scene.");
        if (loadingPercentageText == null)
            Debug.LogWarning("PercentageText not found in the loading scene.");
        if (tipText == null)
            Debug.LogWarning("TipsText not found in the loading scene.");
    }

    private void UpdateLoadingUI(float progress)
    {
        if (loadingBarFill != null)
        {
            loadingBarFill.fillAmount = progress;
        }
        if (loadingPercentageText != null)
        {
            loadingPercentageText.text = $"{Mathf.Round(progress * 100)}%";
        }
    }

    private void DisplayRandomTip()
    {
        if (tipText != null && tips.Count > 0)
        {
            int randomIndex = Random.Range(0, tips.Count);
            tipText.text = tips[randomIndex];
        }
    }
}