using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
    }

    private void OnDisable()
    {
        SceneManager.activeSceneChanged -= OnActiveSceneChanged;
    }

    private void OnActiveSceneChanged(Scene currentScene, Scene nextScene)
    {
        if (nextScene.name != "MainMenu") return;
        if (!HasKey("HighestLevel") || LoadInt("HighestLevel") < 1)
        {
            SaveInt("HighestLevel", 1); // or whatever starting value
        }
    }

    // Save methods for different data types
    public void SaveInt(string key, int value)
    {
        PlayerPrefs.SetInt(key, value);
        PlayerPrefs.Save();
    }

    public void SaveFloat(string key, float value)
    {
        PlayerPrefs.SetFloat(key, value);
        PlayerPrefs.Save();
    }

    public void SaveString(string key, string value)
    {
        PlayerPrefs.SetString(key, value);
        PlayerPrefs.Save();
    }

    public void SaveBool(string key, bool value)
    {
        PlayerPrefs.SetInt(key, value ? 1 : 0);
        PlayerPrefs.Save();
    }

    // Load methods for different data types
    public int LoadInt(string key, int defaultValue = 0)
    {
        return PlayerPrefs.GetInt(key, defaultValue);
    }

    public float LoadFloat(string key, float defaultValue = 0f)
    {
        return PlayerPrefs.GetFloat(key, defaultValue);
    }

    public string LoadString(string key, string defaultValue = "")
    {
        return PlayerPrefs.GetString(key, defaultValue);
    }

    public bool LoadBool(string key, bool defaultValue = false)
    {
        return PlayerPrefs.GetInt(key, defaultValue ? 1 : 0) == 1;
    }

    // Check if a key exists
    public bool HasKey(string key)
    {
        return PlayerPrefs.HasKey(key);
    }

    // Delete a specific key
    public void DeleteKey(string key)
    {
        PlayerPrefs.DeleteKey(key);
        PlayerPrefs.Save();
    }

    

    // Save current game session
    public void SaveGame(int checkpoint = 0)
    {
        if (SceneManager.GetActiveScene().name == "ShowcaseLevel" || SceneManager.GetActiveScene().name == "TestLevel") return;

        if (LoadInt("HighestLevel") <= SceneManager.GetActiveScene().buildIndex) // Save level index
        {
            SaveInt("HighestLevel", SceneManager.GetActiveScene().buildIndex);
        }

        SaveString("SavedLevel", SceneManager.GetActiveScene().name); // Save level

        SaveInt("SavedCheckpoint", checkpoint); // Save checkpoint
        
        PlayerPrefs.Save();
    }
    

    // Delete all saved data
    public void DeleteAllKeys()
    {
        PlayerPrefs.DeleteAll();
        
        PlayerPrefs.Save();
    }

}

