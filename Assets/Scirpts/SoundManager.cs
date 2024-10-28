using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using VInspector;

[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;
    [Range(0f, 2f)] public float volume = 1f;
    [Range(0.1f, 3f)] public float pitch = 1f;
    public bool loop = false;
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
        
        sound.source = gameObject.AddComponent<AudioSource>();
        sound.source.clip = sound.clip;
        sound.source.pitch = sound.pitch;
        sound.source.loop = sound.loop;
        SetupSoundVolume(sound,soundCategory);
    }

    private void SetupSoundVolume(Sound sound, float soundCategory) {
        
        sound.source.volume = sound.volume * soundCategory * masterGameVolume;
    }

    
    private void SetSoundsVolume() {
        
        if (soundEffects.Count >= 1)
        {
            foreach (Sound soundFx in soundEffects)
            {
                soundFx.source.volume = soundFx.volume * soundFXVolume * masterGameVolume;
                Debug.Log(soundFx.source.volume);
            }
        }

        
        if (music.Count >= 1)
        {
            foreach (Sound music in music)
            {
                music.source.volume = music.volume * musicVolume * masterGameVolume;
            }
        }

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
        if (soundFx == null) {
            Debug.LogWarning("SoundFX: " + name + " not found!");
            return;
        }
        
        StartCoroutine(PlayDelayed(soundFx, delay, spawnTransform));
    }
    
    public void PlayMusic(string name) {
        
        Sound newMusic = music.Find(music => music.name == name);
        if (newMusic == null) {
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
        
        yield return new WaitForSeconds(delay);
        if (spawnTransform) {
                
            AudioSource audioSource = Instantiate(soundFXPrefab, spawnTransform.position, Quaternion.identity);
            audioSource.gameObject.name = "Sfx " + soundFx.name;
            audioSource.clip = soundFx.clip;
            audioSource.volume = soundFx.volume * soundFXVolume * masterGameVolume;
            audioSource.pitch = soundFx.pitch;
            audioSource.Play();
            float length = audioSource.clip.length;
            Destroy(audioSource.gameObject, length);
                
        } else {
                
            soundFx.source.Play();
        }
    }
    
#endregion

}