using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using VInspector;



public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }
    
    public AudioSource soundFXPrefab;
    public List<Sound> soundEffects = new List<Sound>();
    public List<Sound> music = new List<Sound>();

    private void Awake() {

        if (Instance != null && Instance != this) 
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        SetupSounds();
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
        PlayMusic(nextScene.name == "MainMenu" ? "MainMenu" : "Gameplay");
    }
    

    

#region Setup

    private void SetupSounds() {
        
        foreach (Sound soundFx in soundEffects)
        {
            SetupSound(soundFx);
        }

        foreach (Sound music in music)
        {
            SetupSound(music);
        }
    }

    private void SetupSound(Sound sound) {
        
        foreach (AudioClip clip in sound.clips)
        {
            sound.source = gameObject.AddComponent<AudioSource>();
            sound.source.clip = clip;
            sound.source.pitch = sound.pitch;
            sound.source.panStereo = sound.stereoPan;
            sound.source.spatialBlend = sound.spatialBlend;
            sound.source.reverbZoneMix = sound.reverbZoneMix;
            sound.source.loop = sound.loop;
            sound.source.volume = sound.volume;
        }
    }
    
    
    [Button]
    public void ResetAllSoundsSettings() {
        
        foreach (AudioSource audioSource in GetComponents<AudioSource>())
        {
            Destroy(audioSource);
        }

        SetupSounds();
    }
    
    
#endregion


#region Sounds

    public void PlaySoundFX(string name, float peach = 0 ,AudioSource audioSource = null) {
        
        // Find sound
        Sound soundFx = soundEffects.Find(sound => sound.name == name);
        if (soundFx == null || soundFx.clips.Length == 0) {

            #if UNITY_EDITOR
            // Debug.Log("SoundFX: " + name + " not found!");   
            #endif
            
            return;
        }
        
        // Choose random clip
        int rand; 
        AudioClip[] clipsType;
        if (GameManager.Instance.funnyMode && soundFx.funnyModeClips.Length > 0) {
            rand = Random.Range(0, soundFx.funnyModeClips.Length);
            clipsType = soundFx.funnyModeClips;
        } else {
            rand = Random.Range(0, soundFx.clips.Length);
            clipsType = soundFx.clips;
        }

       
        if (audioSource == null)  // Play sound using sound manager
        {
           
            soundFx.source.clip = clipsType[rand];
            soundFx.source.volume = soundFx.volume * SettingsManager.Instance.soundFXVolume * SettingsManager.Instance.masterGameVolume;
            soundFx.source.pitch = peach <= 0 ? soundFx.pitch : peach;
            soundFx.source.Play();
            
        } else { // Play sound using specific audio source
            
            audioSource.clip = clipsType[rand];
            audioSource.volume = soundFx.volume * SettingsManager.Instance.soundFXVolume * SettingsManager.Instance.masterGameVolume;
            audioSource.pitch = peach <= 0 ? soundFx.pitch : peach;
            audioSource.loop = soundFx.loop;
            audioSource.panStereo = soundFx.stereoPan;
            audioSource.spatialBlend = soundFx.spatialBlend;
            audioSource.reverbZoneMix = soundFx.reverbZoneMix;
            audioSource.Play();
        }

    }

    public void FadeSoundIn(string name, float pitch = 0, AudioSource audioSource = null, float fadeTime = 1f)
    {
        // Find sound and store its original volume
        Sound soundFx = soundEffects.Find(sound => sound.name == name);
        if (soundFx == null) return;
        
        float originalVolume = soundFx.volume;
        soundFx.volume = 0f;
        
        // Use existing PlaySoundFX function
        PlaySoundFX(name, pitch, audioSource);
        
        // Restore the original volume
        soundFx.volume = originalVolume;
        
        // Get the target audio source
        AudioSource targetSource = audioSource ?? soundFx.source;
        targetSource.volume = 0f;
        
        // Start fade coroutine
        StartCoroutine(FadeVolume(
            targetSource,
            0f,
            originalVolume * SettingsManager.Instance.soundFXVolume * SettingsManager.Instance.masterGameVolume,
            fadeTime
        ));
    }

    public void FadeSoundOut(string name, AudioSource audioSource = null, float fadeTime = 1f)
    {
        Sound soundFx = soundEffects.Find(sound => sound.name == name);
        if (soundFx == null)
        {
            return;
        }

        // Get the target audio source
        AudioSource targetSource = audioSource ?? soundFx.source;
        if (!targetSource.isPlaying)
        {
            return;
        }

        StartCoroutine(FadeVolume(
            targetSource,
            targetSource.volume,
            0f,
            fadeTime,
            true  // Stop the sound after fade
        ));
    }

    private IEnumerator FadeVolume(AudioSource audioSource, float startVolume, float targetVolume, float duration, bool stopAfterFade = false)
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float newVolume = Mathf.Lerp(startVolume, targetVolume, elapsedTime / duration);
            audioSource.volume = newVolume;
            yield return null;
        }

        audioSource.volume = targetVolume;
        
        if (stopAfterFade)
        {
            audioSource.Stop();
        }
    }

    
    
    public void StopSoundFx(string name) {
        
        Sound s = soundEffects.Find(sound => sound.name == name);
        if (s == null) {
            
        #if UNITY_EDITOR 
            // Debug.Log("SoundFX: " + name + " not found!"); 
        #endif
            
            return;
        }
        s.source.Stop();
    }
    
    public void PlayMusic(string name) {
        
        Sound newMusic = music.Find(music => music.name == name);
        if (newMusic == null || newMusic.clips.Length == 0) {
            #if UNITY_EDITOR
            // Debug.Log("Music: " + name + " not found!"); 
            #endif
            
            return;
        }
        
        StopAllMusic();

        int rand = Random.Range(0, newMusic.clips.Length);
        
        newMusic.source.clip = newMusic.clips[rand];
      newMusic.source.volume = newMusic.volume * SettingsManager.Instance.musicVolume * SettingsManager.Instance.masterGameVolume;
        newMusic.source.Play();
    }

    
    
    private void StopAllMusic() {
        
        foreach (Sound music in music)
        {
            if (music.source.isPlaying)
            {
                music.source.Stop();
            }
        }
    }


    public void RestartPlayingMusic()
    {
        foreach (Sound music in music)
        {
            if (music.source.isPlaying)
            {
                PlayMusic(music.name);
            }
        }
    }


    
    
#endregion

}