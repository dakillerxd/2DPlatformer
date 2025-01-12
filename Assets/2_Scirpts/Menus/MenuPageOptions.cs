using TMPro;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;


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
    
    [Header("UI Elements")]
    [SerializeField] private Button buttonOptionsBack;
    [SerializeField] private Button buttonDeleteSave;
    private Resolution[] _uniqueResolutions;
    

    
    private void Start()
    {
        SetupButtons();
        SetupResolutionDropdown();
        SetupScreenModeDropdown();
        SetupVSyncDropDown();
        SetupFPSSlider();
        SetupVolumeSliders();
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
            
                buttonOptionsBack.onClick.AddListener(() => SoundManager.Instance?.PlaySoundFX("ButtonClick"));
                buttonOptionsBack.onClick.AddListener(() => _menuCategoryMain.SelectPage(_menuCategoryMain.mainMenuPage));
            
            } else {
                buttonOptionsBack.onClick.AddListener(() => SoundManager.Instance?.PlaySoundFX("ButtonClick"));
                buttonOptionsBack.onClick.AddListener(() => _menuCategoryPause.SelectPage(_menuCategoryPause.pauseMenuPage));
            }
        }
        
        
        if (buttonDeleteSave)
        {
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
        vSyncDropdown.onValueChanged.AddListener((value) => SettingsManager.Instance.SetVSync(value));
    }
    
    private void SetupScreenModeDropdown()
    {
        if (!screenModeDropdown) return;
        screenModeDropdown.ClearOptions();
        List<string> options = System.Enum.GetNames(typeof(SettingsManager.ScreenModes)).ToList();
        screenModeDropdown.AddOptions(options);
        screenModeDropdown.value = (int)SettingsManager.Instance.screenMode;
        screenModeDropdown.onValueChanged.AddListener((value) => 
        {
            SettingsManager.Instance.SetScreenMode((SettingsManager.ScreenModes)value);
        });
    }

    private void SetupResolutionDropdown()
    {
        _uniqueResolutions = Screen.resolutions
            .GroupBy(resolution => new { resolution.width, resolution.height })
            .Select(group => group.First())
            .OrderByDescending(resolution => resolution.width * resolution.height)
            .ToArray();
    
        if (!resolutionDropdown) return;
        resolutionDropdown.ClearOptions();
        List<string> options = _uniqueResolutions.Select(res => $"{res.width} x {res.height}").ToList();
        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = SettingsManager.Instance.resolutionIndex;
        resolutionDropdown.onValueChanged.AddListener((value) => SettingsManager.Instance.SetResolution(value));
    }

    private void SetupFPSSlider()
    {
        fpsSlider.value = (SettingsManager.Instance.targetFPS - 30f) / (999f - 30f); // Convert FPS to 0-1 range

        fpsSlider.onValueChanged.AddListener((value) =>
        {
            float fps = 30f + (value * (999f - 30f));
            SettingsManager.Instance.SetFPS((int)fps);
        });
        
    }

    private void SetupVolumeSliders()
    {
        if (masterVolumeSlider)
        {
            masterVolumeSlider.value = SettingsManager.Instance.masterGameVolume;
            masterVolumeSlider.onValueChanged.AddListener((value) => SettingsManager.Instance.SetMasterVolume(value));
            masterVolumeSlider.onValueChanged.AddListener((value) => SoundManager.Instance.PlaySoundFX("Player Jump"));
            masterVolumeSlider.onValueChanged.AddListener((value) => SoundManager.Instance.RestartPlayingMusic());
            
        }
        
        if (sfxVolumeSlider)
        {
            sfxVolumeSlider.value = SettingsManager.Instance.soundFXVolume;
            sfxVolumeSlider.onValueChanged.AddListener((value) => SettingsManager.Instance.SetSoundFXVolume(value));
            sfxVolumeSlider.onValueChanged.AddListener((value) => SoundManager.Instance.PlaySoundFX("Player Jump"));
        }

        if (musicVolumeSlider)
        {
            musicVolumeSlider.value = SettingsManager.Instance.musicVolume;
            musicVolumeSlider.onValueChanged.AddListener((value) => SettingsManager.Instance.SetMusicVolume(value));
            musicVolumeSlider.onValueChanged.AddListener((value) => SoundManager.Instance.RestartPlayingMusic());
        }
    }
    
}
