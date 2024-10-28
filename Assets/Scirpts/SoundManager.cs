using UnityEngine;
using System.Collections;
using System.Collections.Generic;
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
    [HideInInspector] public AudioSource source;
    
}

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Range(0f, 1f)] public float masterGameVolume;
    [Range(0f, 1f)] public float soundFXVolume;
    [Range(0f, 1f)] public float musicVolume;
    public AudioSource soundFXPrefab;
    public List<Sound> soundEffects = new List<Sound>();
    public List<Sound> music = new List<Sound>();

    private void Awake() {

        if (Instance != null && Instance != this) {

            Destroy(gameObject);

        } else {

            Instance = this;
        }
        
        SetupSounds();
    }


#region Setup

    private void SetupSounds() {
        
        foreach (Sound soundFx in soundEffects)
        {
            SetupSound(soundFx, soundFXVolume);
        }

        foreach (Sound music in music)
        {
            SetupSound(music, musicVolume);
        }
    }

    private void SetupSound(Sound sound, float soundCategory) {
        
        foreach (AudioClip clip in sound.clips)
        {
            sound.source = gameObject.AddComponent<AudioSource>();
            sound.source.clip = clip;
            sound.source.pitch = sound.pitch;
            sound.source.panStereo = sound.stereoPan;
            sound.source.spatialBlend = sound.spatialBlend;
            sound.source.reverbZoneMix = sound.reverbZoneMix;
            sound.source.loop = sound.loop;
            sound.source.volume = sound.volume * soundCategory * masterGameVolume;
        }

    }
    
    
    [Button]
    private void ResetAllSoundsSettings() {
        
        foreach (AudioSource audioSource in GetComponents<AudioSource>())
        {
            Destroy(audioSource);
        }

        SetupSounds();
    }

    private void LoadVolumes() {
        masterGameVolume = SaveManager.Instance.LoadFloat("MasterGameVolume", 1f);
        soundFXVolume = SaveManager.Instance.LoadFloat("SoundFXVolume", 0.5f);
        musicVolume = SaveManager.Instance.LoadFloat("MusicVolume", 0.3f);
    }
    
    
#endregion


#region Sounds

    public void PlaySoundFX(string name, float delay = 0f, Transform spawnTransform = null) {
        
        Sound soundFx = soundEffects.Find(sound => sound.name == name);
        if (soundFx == null || soundFx.clips.Length == 0) {
            Debug.LogWarning("SoundFX: " + name + " not found!");
            return;
        }
        
        StartCoroutine(PlayDelayed(soundFx, delay, spawnTransform));
    }
    
    public void PlayMusic(string name) {
        
        Sound newMusic = music.Find(music => music.name == name);
        if (newMusic == null || newMusic.clips.Length == 0) {
            Debug.LogWarning("Music: " + name + " not found!");
            return;
        }
        
        StopAllMusic();
        newMusic.source.Play();
    }


    public void StopSoundFx(string name) {
        
        Sound s = soundEffects.Find(sound => sound.name == name);
        if (s == null) {
            Debug.LogWarning("SoundFX: " + name + " not found!");
            return;
        }
        s.source.Stop();
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
    
    private IEnumerator PlayDelayed(Sound soundFx, float delay, Transform spawnTransform) {
        
        int rand = Random.Range(0, soundFx.clips.Length);
        yield return new WaitForSeconds(delay);
        if (spawnTransform) {
                
            AudioSource audioSource = Instantiate(soundFXPrefab, spawnTransform.position, Quaternion.identity);
            audioSource.gameObject.name = "Sfx " + soundFx.name;
            audioSource.clip = soundFx.clips[rand];
            audioSource.volume = soundFx.volume * soundFXVolume * masterGameVolume;
            audioSource.panStereo = soundFx.stereoPan;
            audioSource.spatialBlend = soundFx.spatialBlend;
            audioSource.reverbZoneMix = soundFx.reverbZoneMix;
            audioSource.pitch = soundFx.pitch;
            audioSource.loop = soundFx.loop;
            audioSource.Play();

            if (!audioSource.loop)
            {
                float length = audioSource.clip.length;
                Destroy(audioSource.gameObject, length);
            }
            
        } else {
                
            soundFx.source.clip = soundFx.clips[rand];
            soundFx.source.Play();
        }
    }
    
#endregion

}