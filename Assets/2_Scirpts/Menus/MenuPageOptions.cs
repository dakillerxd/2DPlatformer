using TMPro;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class MenuPageOptions : MenuPage
{
    [Header("Settings")]
    [SerializeField] private TMP_Dropdown vSyncDropdown;
    [SerializeField] private TMP_Dropdown screenModeDropdown;
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private Scrollbar fpsSlider;
    [SerializeField] private TextMeshProUGUI fpsSliderAmount;
    [SerializeField] private Scrollbar masterVolumeSlider;
    [SerializeField] private TextMeshProUGUI masterVolumeSliderAmount;
    [SerializeField] private Scrollbar sfxVolumeSlider;
    [SerializeField] private TextMeshProUGUI sfxVolumeSliderAmount;
    [SerializeField] private Scrollbar musicVolumeSlider;
    [SerializeField] private TextMeshProUGUI musicVolumeSliderAmount;
    [SerializeField] private Toggle force16By9AspectRatioToggle;
    
    [Header("UI Elements")]
    [SerializeField] private Button buttonOptionsBack;
    [SerializeField] private Button buttonDeleteSave;
    
    protected override void Start()
    {
        base.Start();
        SetupButtons();
        SetupResolutionDropdown();
        SetupScreenModeDropdown();
        SetupVSyncDropDown();
        SetupFPSSlider();
        SetupForce16By9Toggle();
        SetupVolumeSliders();

        // Subscribe to resolution change events
        SettingsManager.Instance.onResolutionChange.AddListener(UpdateResolutionDropdown);
    }

    private void OnDestroy()
    {
        // Unsubscribe from events when the object is destroyed
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.onResolutionChange.RemoveListener(UpdateResolutionDropdown);
        }
    }
    
    private void Update()
    {
        if (fpsSlider) fpsSliderAmount.text = SettingsManager.Instance.targetFPS.ToString();
        if (masterVolumeSlider) masterVolumeSliderAmount.text = (masterVolumeSlider.value * 100).ToString("F0") + "%";
        if (sfxVolumeSlider) sfxVolumeSliderAmount.text = (sfxVolumeSlider.value * 100).ToString("F0") + "%";
        if (musicVolumeSlider) musicVolumeSliderAmount.text = (musicVolumeSlider.value * 100).ToString("F0") + "%";
    }

    public override void OnActiveSceneChanged(Scene currentScene, Scene nextScene)
    {
        base.OnActiveSceneChanged(currentScene, nextScene);
        SetupButtons();
    }

    private void SetupButtons()
    {
        if (buttonOptionsBack)
        {
            if (SceneManager.GetActiveScene().name == "MainMenu") {
                buttonOptionsBack.onClick.RemoveAllListeners();
                buttonOptionsBack.onClick.AddListener(() => SoundManager.Instance?.PlaySoundFX("ButtonClick"));
                buttonOptionsBack.onClick.AddListener(() => menuCategoryMain.SelectPage(menuCategoryMain.mainMenuPage));
            } else {
                buttonOptionsBack.onClick.RemoveAllListeners();
                buttonOptionsBack.onClick.AddListener(() => SoundManager.Instance?.PlaySoundFX("ButtonClick"));
                buttonOptionsBack.onClick.AddListener(() => menuCategoryPause.SelectPage(menuCategoryPause.pauseMenuPage));
            }
        }
        
        if (buttonDeleteSave)
        {
            buttonDeleteSave.onClick.RemoveAllListeners();
            buttonDeleteSave.onClick.AddListener(() => SoundManager.Instance?.PlaySoundFX("ButtonClick"));
            buttonDeleteSave.onClick.AddListener(() => GameManager.Instance?.DeleteSave());
        }
    }

    private void SetupVSyncDropDown()
    {
        if (!vSyncDropdown) return;
    
        vSyncDropdown.ClearOptions();
        vSyncDropdown.AddOptions(new List<string> { 
            "Disabled", 
            "Every V-Blank", 
            "Every Second V-Blank"
        });
    
        vSyncDropdown.value = SettingsManager.Instance.vSync;
        vSyncDropdown.onValueChanged.RemoveAllListeners();
        vSyncDropdown.onValueChanged.AddListener((value) => SettingsManager.Instance.SetVSync(value));
    }
    
    private void SetupScreenModeDropdown()
    {
        if (!screenModeDropdown) return;
        screenModeDropdown.ClearOptions();
        List<string> options = System.Enum.GetNames(typeof(SettingsManager.ScreenModes)).ToList();
        screenModeDropdown.AddOptions(options);
        screenModeDropdown.value = (int)SettingsManager.Instance.screenMode;
        screenModeDropdown.onValueChanged.RemoveAllListeners();
        screenModeDropdown.onValueChanged.AddListener((value) => 
        {
            SettingsManager.Instance.SetScreenMode((SettingsManager.ScreenModes)value);
        });
    }

    private void SetupResolutionDropdown()
    {
        if (!resolutionDropdown) return;
        UpdateResolutionDropdown();
    }

    private void UpdateResolutionDropdown()
    {
        if (!resolutionDropdown) return;

        // Get the filtered resolutions from SettingsManager
        Resolution[] availableResolutions = SettingsManager.Instance.GetAvailableResolutions();

        resolutionDropdown.ClearOptions();
        List<string> options = availableResolutions
            .Select(res => $"{res.width} x {res.height}")
            .ToList();
        
        resolutionDropdown.AddOptions(options);

        // Set the current resolution index
        resolutionDropdown.value = SettingsManager.Instance.resolutionIndex;

        // Update the dropdown listener
        resolutionDropdown.onValueChanged.RemoveAllListeners();
        resolutionDropdown.onValueChanged.AddListener((value) => SettingsManager.Instance.SetResolution(value));
    }

    private void SetupFPSSlider()
    {
        if (!fpsSlider) return;
        
        fpsSlider.value = (SettingsManager.Instance.targetFPS - 30f) / (999f - 30f); // Convert FPS to 0-1 range
        fpsSlider.onValueChanged.RemoveAllListeners();
        fpsSlider.onValueChanged.AddListener((value) =>
        {
            float fps = 30f + (value * (999f - 30f));
            SettingsManager.Instance.SetFPS((int)fps);
        });
    }
    
    private void SetupForce16By9Toggle()
    {
        if (!force16By9AspectRatioToggle) return;
        
        force16By9AspectRatioToggle.isOn = SettingsManager.Instance.force16By9AspectRatio;
        force16By9AspectRatioToggle.onValueChanged.RemoveAllListeners();
        force16By9AspectRatioToggle.onValueChanged.AddListener((value) => SettingsManager.Instance?.SetForce16by9(value));
        force16By9AspectRatioToggle.onValueChanged.AddListener((value) => SoundManager.Instance?.PlaySoundFX("Toggle"));
    }

    private void SetupVolumeSliders()
    {
        if (masterVolumeSlider)
        {
            masterVolumeSlider.value = SettingsManager.Instance.masterGameVolume;
            masterVolumeSlider.onValueChanged.RemoveAllListeners();
            masterVolumeSlider.onValueChanged.AddListener((value) => SettingsManager.Instance?.SetMasterVolume(value));
            masterVolumeSlider.onValueChanged.AddListener((value) => SoundManager.Instance?.PlaySoundFX("Player Jump"));
            masterVolumeSlider.onValueChanged.AddListener((value) => SoundManager.Instance?.RestartPlayingMusic());
        }
        
        if (sfxVolumeSlider)
        {
            sfxVolumeSlider.value = SettingsManager.Instance.soundFXVolume;
            sfxVolumeSlider.onValueChanged.RemoveAllListeners();
            sfxVolumeSlider.onValueChanged.AddListener((value) => SettingsManager.Instance?.SetSoundFXVolume(value));
            sfxVolumeSlider.onValueChanged.AddListener((value) => SoundManager.Instance?.PlaySoundFX("Player Jump"));
        }

        if (musicVolumeSlider)
        {
            musicVolumeSlider.value = SettingsManager.Instance.musicVolume;
            musicVolumeSlider.onValueChanged.RemoveAllListeners();
            musicVolumeSlider.onValueChanged.AddListener((value) => SettingsManager.Instance?.SetMusicVolume(value));
            musicVolumeSlider.onValueChanged.AddListener((value) => SoundManager.Instance?.RestartPlayingMusic());
        }
    }
}