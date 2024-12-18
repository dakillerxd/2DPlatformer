using UnityEngine;
using UnityEngine.Events;


public class SettingsManager : MonoBehaviour
{
    
    public static SettingsManager Instance { get; private set; }
    
    public enum ScreenModes
    {
        FullScreen = 0,
        Borderless = 1,
        Windowed = 2
    }

    [Header("Settings")] 
    public int targetFPS;
    public int vSync;
    public ScreenModes screenMode;
    public int resolutionIndex;
    public float masterGameVolume;
    public float soundFXVolume;
    public float musicVolume;
    
    [Header("Default Settings")]
    [SerializeField] private  int defaultTargetFPS = 999;
    [SerializeField] private int defaultVSync = 0;
    [SerializeField] private ScreenModes defaultScreenMode = ScreenModes.Borderless;
    [SerializeField] private int defaultResolutionIndex = 0;
    [SerializeField] private float defaultMasterGameVolume = 1f;
    [SerializeField] private float defaultSoundFXVolume = 0.50f;
    [SerializeField] private float defaultMusicVolume = 0.30f;
    
    [Header("Events")]
    public UnityEvent onSoundFXVolumeChange = new UnityEvent();
    public UnityEvent onMusicVolumeChange = new UnityEvent();
    public UnityEvent onMasterVolumeChange = new UnityEvent();

    private void Awake()
    {
        
        if (Instance != null && Instance != this) 
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;


        defaultResolutionIndex = Screen.resolutions.Length - 1;
        LoadAllSettings();
    }

    public void LoadAllSettings()
    {
        SetResolution(SaveManager.Instance.LoadInt("ResolutionIndex", defaultResolutionIndex));
        SetScreenMode((ScreenModes)SaveManager.Instance.LoadInt("screenMode", (int)defaultScreenMode));
        SetVSync(SaveManager.Instance.LoadInt("VSync", defaultVSync));
        SetFPS(SaveManager.Instance.LoadInt("FPS", defaultTargetFPS));
        SetMasterVolume(SaveManager.Instance.LoadFloat("MasterVolume", defaultMasterGameVolume));
        SetSoundFXVolume(SaveManager.Instance.LoadFloat("GameVolume", defaultSoundFXVolume));
        SetMusicVolume(SaveManager.Instance.LoadFloat("MusicVolume", defaultMusicVolume));
    }
    

    public void SetScreenMode(ScreenModes modes)
    {
        FullScreenMode unityScreenMode;
        Resolution[] resolutions = Screen.resolutions;
        Resolution highestResolution = resolutions[resolutions.Length - 1];
    
        switch (modes)
        {
            case ScreenModes.FullScreen:
                unityScreenMode = FullScreenMode.ExclusiveFullScreen;
                Screen.SetResolution(highestResolution.width, highestResolution.height, unityScreenMode);
                break;
            case ScreenModes.Borderless:
                unityScreenMode = FullScreenMode.FullScreenWindow;
                Screen.SetResolution(highestResolution.width, highestResolution.height, unityScreenMode);
                break;
            case ScreenModes.Windowed:
                unityScreenMode = FullScreenMode.Windowed;
                Resolution currentResolution = Screen.currentResolution;
                Screen.SetResolution(currentResolution.width, currentResolution.height, unityScreenMode);
                break;
        }

        screenMode = modes;
        SaveManager.Instance.SaveInt("ScreenModes", (int)modes);
    }

    public void SetResolution(int index)
    {
        Resolution[] resolutions = Screen.resolutions;
        if (index >= 0 && index < resolutions.Length)
        {
            Resolution resolution = resolutions[index];
            resolutionIndex = index;
            Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreenMode);
            SaveManager.Instance.SaveInt("ResolutionIndex", index);
        }

    }

    public void SetVSync(int vSyncCount)
    {
        vSync = vSyncCount;
        QualitySettings.vSyncCount = vSync;
        SaveManager.Instance.SaveInt("VSync", vSync);
    }

    public void SetFPS(int fps)
    {
        targetFPS = fps;
        Application.targetFrameRate = targetFPS;
        SaveManager.Instance.SaveInt("FPS", fps);
    }

    public void SetMasterVolume(float volume)
    {
        masterGameVolume = volume;
        SaveManager.Instance.SaveFloat("MasterVolume", masterGameVolume);
        onMasterVolumeChange.Invoke();
    }
    
    public void SetSoundFXVolume(float volume)
    {
        soundFXVolume = volume;
        SaveManager.Instance.SaveFloat("GameVolume", soundFXVolume);
        onSoundFXVolumeChange.Invoke();
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = volume;
        SaveManager.Instance.SaveFloat("MusicVolume", musicVolume);
        onMusicVolumeChange.Invoke();
    }
    
}