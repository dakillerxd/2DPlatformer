using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Serialization;
using VInspector;

[System.Serializable]
public class Sound
{
    public string name;
    [Range(0f, 2f)] public float volume = 1f;
    [Range(0.1f, 3f)] public float pitch = 1f;
    [Range(-1f, 1f)] public float stereoPan = 0f;
    [Range(0f, 1f)] public float spatialBlend = 0f;
    [Range(0f, 1.1f)] public float reverbZoneMix = 1f;
    public bool loop = false;
    public AudioClip[] clips;
    public AudioClip[] funnyModeClips;
    [HideInInspector] public AudioSource source;
    
}

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


    
    
#endregion

}