using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

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

        LoadAllSettings();
    }


    public void LoadAllSettings()
    {
        ScreenMode screenMode = (ScreenMode)SaveManager.Instance.LoadInt("ScreenMode", (int)ScreenMode.FullScreen);
        int resolutionIndex = SaveManager.Instance.LoadInt("ResolutionIndex", Screen.resolutions.Length - 1);
        bool vSync = SaveManager.Instance.LoadBool("VSync", false);
        float gameVolume = SaveManager.Instance.LoadFloat("GameVolume", 0.50f);
        float musicVolume = SaveManager.Instance.LoadFloat("MusicVolume", 0.30f);

        ApplySettings(screenMode, resolutionIndex, vSync, gameVolume, musicVolume);
    }

    public void ApplySettings(ScreenMode screenMode, int resolutionIndex, bool vSync, float gameVolume, float musicVolume)
    {
        
        SetResolution(resolutionIndex);
        SetScreenMode(screenMode);
        SetVSync(vSync);
        SetGameVolume(gameVolume);
        SetMusicVolume(musicVolume);
    }

    public enum ScreenMode
    {
        FullScreen = 0,
        Borderless = 1,
        Windowed = 2
    }

    public void SetScreenMode(ScreenMode mode)
    {
        FullScreenMode unityScreenMode;
        switch (mode)
        {
            case ScreenMode.FullScreen:
                unityScreenMode = FullScreenMode.ExclusiveFullScreen;
                break;
            case ScreenMode.Borderless:
                unityScreenMode = FullScreenMode.FullScreenWindow;
                break;
            case ScreenMode.Windowed:
                unityScreenMode = FullScreenMode.Windowed;
                break;
            default:
                unityScreenMode = FullScreenMode.Windowed;
                break;
        }

        Resolution currentResolution = Screen.currentResolution;
        Screen.SetResolution(currentResolution.width, currentResolution.height, unityScreenMode);

        SaveManager.Instance.SaveInt("ScreenMode", (int)mode);
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution[] resolutions = Screen.resolutions;
        if (resolutionIndex >= 0 && resolutionIndex < resolutions.Length)
        {
            Resolution resolution = resolutions[resolutionIndex];
            Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreenMode);
            SaveManager.Instance.SaveInt("ResolutionIndex", resolutionIndex);
        }
        else
        {
            Debug.LogWarning($"Invalid resolution index: {resolutionIndex}");
        }
    }

    public void SetVSync(bool enableVSync)
    {
        QualitySettings.vSyncCount = enableVSync ? 1 : 0;
        SaveManager.Instance.SaveBool("VSync", enableVSync);
    }

    public void SetGameVolume(float volume)
    {
        
        SaveManager.Instance.SaveFloat("GameVolume", volume);
        SoundManager.Instance?.SetGameVolume(volume);
    }

    public void SetMusicVolume(float volume)
    {
        
        SaveManager.Instance.SaveFloat("MusicVolume", volume);
        SoundManager.Instance?.SetMusicVolume(volume);
    }


    public void ResetToDefaults()
    {
        ApplySettings(ScreenMode.Windowed, Screen.resolutions.Length - 1, false, 0.50f, 0.30f);
    }
}