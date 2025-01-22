using System.Collections.Generic;
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
    public static void SaveInt(string key, int value)
    {
        PlayerPrefs.SetInt(key, value);
        PlayerPrefs.Save();
    }

    public static void SaveFloat(string key, float value)
    {
        PlayerPrefs.SetFloat(key, value);
        PlayerPrefs.Save();
    }

    public static void SaveString(string key, string value)
    {
        PlayerPrefs.SetString(key, value);
        PlayerPrefs.Save();
    }

    public static void SaveBool(string key, bool value)
    {
        PlayerPrefs.SetInt(key, value ? 1 : 0);
        PlayerPrefs.Save();
    }

    // Load methods for different data types
    public static int LoadInt(string key, int defaultValue = 0)
    {
        return PlayerPrefs.GetInt(key, defaultValue);
    }

    public static float LoadFloat(string key, float defaultValue = 0f)
    {
        return PlayerPrefs.GetFloat(key, defaultValue);
    }

    public static string LoadString(string key, string defaultValue = "")
    {
        return PlayerPrefs.GetString(key, defaultValue);
    }

    public static bool LoadBool(string key, bool defaultValue = false)
    {
        return PlayerPrefs.GetInt(key, defaultValue ? 1 : 0) == 1;
    }
    
    

    // Check if a key exists
    public static bool HasKey(string key)
    {
        return PlayerPrefs.HasKey(key);
    }

    // Delete a specific key
    public static void DeleteKey(string key)
    {
        PlayerPrefs.DeleteKey(key);
        PlayerPrefs.Save();
    }

    

    // Save current game session
    public static void SaveGame(int checkpoint = 0)
    {
        if (GameManager.Instance.SceneIsALevel())
        {
            
            if (LoadInt("HighestLevel") <= GameManager.Instance.CurrentLevelNumber()) // Save level index
            {
                SaveInt("HighestLevel", GameManager.Instance.CurrentLevelNumber());
            }

            SaveString("SavedLevel", GameManager.Instance.CurrentLevel()); // Save level

            SaveInt("SavedCheckpoint", checkpoint); // Save checkpoint
        
            PlayerPrefs.Save();
        }
    }

    public static void LoadCheckpoint(ref Checkpoint activeCheckpoint, List<Checkpoint> checkpoints)
    {
        if (GameManager.Instance.SceneIsALevel())
        {
            if (LoadString("SavedLevel") == GameManager.Instance.CurrentLevel())
            {
                if (LoadInt("SavedCheckpoint") == 0)
                    activeCheckpoint = null;
                else
                    activeCheckpoint = checkpoints[LoadInt("SavedCheckpoint") - 1];
            }
        }
    }




    // Delete all saved data
    public static void DeleteAllKeys()
    {
        PlayerPrefs.DeleteAll();
        
        PlayerPrefs.Save();
    }

}

