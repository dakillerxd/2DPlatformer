using UnityEngine;
using UnityEngine.Events;
using System.Linq;
using UnityEngine.Serialization;


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
    public bool force16By9AspectRatio;
    
    [Header("Default Settings")]
    [SerializeField] private int defaultTargetFPS = 999;
    [SerializeField] private int defaultVSync = 0;
    [SerializeField] private ScreenModes defaultScreenMode = ScreenModes.Borderless;
    [SerializeField] private int defaultResolutionIndex = 0;
    [SerializeField] private float defaultMasterGameVolume = 1f;
    [SerializeField] private float defaultSoundFXVolume = 0.7f;
    [SerializeField] private float defaultMusicVolume = 0.5f;
    [SerializeField] private bool defaultForce16By9 = true;
    
    [Header("Events")]
    public UnityEvent onSoundFXVolumeChange = new UnityEvent();
    public UnityEvent onMusicVolumeChange = new UnityEvent();
    public UnityEvent onMasterVolumeChange = new UnityEvent();
    public UnityEvent onResolutionChange = new UnityEvent();

    private Resolution[] _cachedResolutions;

    private void Awake()
    {
        if (Instance != null && Instance != this) 
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        UpdateAvailableResolutions();
        defaultResolutionIndex = _cachedResolutions.Length - 1;
        LoadAllSettings();
    }

    private void UpdateAvailableResolutions()
    {
        Resolution[] allResolutions = Screen.resolutions;
        
        if (force16By9AspectRatio)
        {
            // Filter for 16:9 resolutions (allowing for small floating-point differences)
            _cachedResolutions = allResolutions.Where(r => 
            {
                float aspect = (float)r.width / r.height;
                return Mathf.Approximately(aspect, 16f/9f);
            }).ToArray();
        }
        else
        {
            _cachedResolutions = allResolutions;
        }
    }

    public Resolution[] GetAvailableResolutions()
    {
        return _cachedResolutions;
    }

    public void LoadAllSettings()
    {
        SetForce16by9(SaveManager.LoadBool("Force16by9", defaultForce16By9));
        SetResolution(SaveManager.LoadInt("ResolutionIndex", defaultResolutionIndex));
        SetScreenMode((ScreenModes)SaveManager.LoadInt("screenMode", (int)defaultScreenMode));
        SetVSync(SaveManager.LoadInt("VSync", defaultVSync));
        SetFPS(SaveManager.LoadInt("FPS", defaultTargetFPS));
        SetMasterVolume(SaveManager.LoadFloat("MasterVolume", defaultMasterGameVolume));
        SetSoundFXVolume(SaveManager.LoadFloat("GameVolume", defaultSoundFXVolume));
        SetMusicVolume(SaveManager.LoadFloat("MusicVolume", defaultMusicVolume));
    }

    public void SetForce16by9(bool force)
    {
        if (force16By9AspectRatio != force)
        {
            force16By9AspectRatio = force;
            SaveManager.SaveBool("Force16by9", force);
            UpdateAvailableResolutions();
            
            // Reset resolution to the highest available if current resolution is not valid
            if (resolutionIndex >= _cachedResolutions.Length)
            {
                SetResolution(_cachedResolutions.Length - 1);
            }
            
            onResolutionChange.Invoke();
        }
    }

    public void SetScreenMode(ScreenModes modes)
    {
        FullScreenMode unityScreenMode;
        Resolution highestResolution = _cachedResolutions[_cachedResolutions.Length - 1];
    
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
        SaveManager.SaveInt("ScreenModes", (int)modes);
    }

    public void SetResolution(int index)
    {
        if (index >= 0 && index < _cachedResolutions.Length)
        {
            Resolution resolution = _cachedResolutions[index];
            resolutionIndex = index;
            Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreenMode);
            SaveManager.SaveInt("ResolutionIndex", index);
            onResolutionChange.Invoke();
        }
    }

    public void SetVSync(int vSyncCount)
    {
        vSync = vSyncCount;
        QualitySettings.vSyncCount = vSync;
        SaveManager.SaveInt("VSync", vSync);
    }

    public void SetFPS(int fps)
    {
        targetFPS = fps;
        Application.targetFrameRate = targetFPS;
        SaveManager.SaveInt("FPS", fps);
    }

    public void SetMasterVolume(float volume)
    {
        masterGameVolume = volume;
        SaveManager.SaveFloat("MasterVolume", masterGameVolume);
        onMasterVolumeChange.Invoke();
    }
    
    public void SetSoundFXVolume(float volume)
    {
        soundFXVolume = volume;
        SaveManager.SaveFloat("GameVolume", soundFXVolume);
        onSoundFXVolumeChange.Invoke();
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = volume;
        SaveManager.SaveFloat("MusicVolume", musicVolume);
        onMusicVolumeChange.Invoke();
    }
}