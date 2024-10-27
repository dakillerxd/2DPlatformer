using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OptionsPanelManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_Dropdown screenModeDropdown;
    public TMP_Dropdown resolutionDropdown;
    public Toggle vSyncToggle;
    public Slider gameVolumeSlider;
    public Slider musicVolumeSlider;

    private Resolution[] uniqueResolutions;

    private void Start()
    {
        InitializeResolutions();
        SetupScreenModeDropdown();
        SetupResolutionDropdown();
        SetupVSyncToggle();
        SetupVolumeSliders();
        UpdateUIFromSettings();
    }

    private void InitializeResolutions()
    {
        uniqueResolutions = Screen.resolutions
            .GroupBy(resolution => new { resolution.width, resolution.height })
            .Select(group => group.First())
            .OrderByDescending(resolution => resolution.width * resolution.height)
            .ToArray();
    }

    private void SetupScreenModeDropdown()
    {
        if (screenModeDropdown != null)
        {
            screenModeDropdown.ClearOptions();
            List<string> options = System.Enum.GetNames(typeof(SettingsManager.ScreenMode)).ToList();
            screenModeDropdown.AddOptions(options);
            screenModeDropdown.onValueChanged.AddListener(OnScreenModeChanged);
        }
    }

    private void SetupResolutionDropdown()
    {
        if (resolutionDropdown != null)
        {
            resolutionDropdown.ClearOptions();
            List<string> options = uniqueResolutions.Select(res => $"{res.width} x {res.height}").ToList();
            resolutionDropdown.AddOptions(options);
            resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
        }
    }

    private void SetupVSyncToggle()
    {
        if (vSyncToggle != null)
        {
            vSyncToggle.onValueChanged.AddListener(OnVSyncChanged);
        }
    }

    private void SetupVolumeSliders()
    {
        if (gameVolumeSlider != null)
        {
            gameVolumeSlider.onValueChanged.AddListener(OnGameVolumeChanged);
        }

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }
    }

    private void UpdateUIFromSettings()
    {
        SettingsManager.ScreenMode screenMode = (SettingsManager.ScreenMode)SaveManager.Instance.LoadInt("ScreenMode", (int)SettingsManager.ScreenMode.FullScreen);
        Resolution currentResolution = Screen.currentResolution;
        int resolutionIndex = System.Array.FindIndex(uniqueResolutions, r => r.width == currentResolution.width && r.height == currentResolution.height);
        bool vSync = SaveManager.Instance.LoadBool("VSync", false);
        float gameVolume = SaveManager.Instance.LoadFloat("GameVolume", 0.50f);
        float musicVolume = SaveManager.Instance.LoadFloat("MusicVolume", 0.30f);

        screenModeDropdown.value = (int)screenMode;
        resolutionDropdown.value = resolutionIndex != -1 ? resolutionIndex : uniqueResolutions.Length - 1;
        vSyncToggle.isOn = vSync;
        gameVolumeSlider.value = gameVolume;
        musicVolumeSlider.value = musicVolume;
    }

    private void OnScreenModeChanged(int index)
    {
        SettingsManager.Instance.SetScreenMode((SettingsManager.ScreenMode)index);
        UpdateUIFromSettings(); // Refresh UI to reflect any changes
        
    }

    private void OnResolutionChanged(int index)
    {
        if (index >= 0 && index < uniqueResolutions.Length)
        {
            Resolution selectedResolution = uniqueResolutions[index];
            int fullResolutionIndex = System.Array.FindIndex(Screen.resolutions, r => r.width == selectedResolution.width && r.height == selectedResolution.height);
            SettingsManager.Instance.SetResolution(fullResolutionIndex);
        }
    }

    private void OnVSyncChanged(bool isOn)
    {
        SettingsManager.Instance.SetVSync(isOn);
    }

    private void OnGameVolumeChanged(float volume)
    {
        SettingsManager.Instance.SetGameVolume(volume);
    }

    private void OnMusicVolumeChanged(float volume)
    {
        SettingsManager.Instance.SetMusicVolume(volume);
    }

    public void RefreshUI()
    {
        UpdateUIFromSettings();
    }

    public void ResetToDefaults()
    {
        SettingsManager.Instance.ResetToDefaults();
        RefreshUI();
    }
}