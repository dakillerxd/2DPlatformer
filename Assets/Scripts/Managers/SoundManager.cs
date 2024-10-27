using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;
    [Range(0f, 2f)] public float volume = 1f;
    [Range(0.1f, 3f)] public float pitch = 1f;
    public bool loop = false;
    [HideInInspector] public AudioSource source;

    public void UpdateVolume(float categoryVolume)
    {
        if (source != null)
        {
            source.volume = volume * categoryVolume;
        }
    }
}

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    public float currentGameVolume;
    public float currentMusicVolume;

    public List<Sound> sounds = new List<Sound>();
    public List<Sound> musics = new List<Sound>();

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
            return;
        }

        
        SetupSounds();
        LoadVolumes();
    }



#region Setup
    private void SetupSounds()
    {
        foreach (Sound s in sounds)
        {
            SetupSound(s, currentGameVolume);
        }

        foreach (Sound s in musics)
        {
            SetupSound(s, currentMusicVolume);
        }
    }

    private void SetupSound(Sound s, float categoryVolume)
    {
        s.source = gameObject.AddComponent<AudioSource>();
        s.source.clip = s.clip;
        s.source.pitch = s.pitch;
        s.source.loop = s.loop;
        s.UpdateVolume(categoryVolume);
    }

    private void LoadVolumes()
    {
        currentGameVolume = SaveManager.Instance.LoadFloat("GameVolume", 0.50f);
        currentMusicVolume = SaveManager.Instance.LoadFloat("MusicVolume", 0.30f);
        SetGameVolume(currentGameVolume);
        SetMusicVolume(currentMusicVolume);
    }

    public void SetGameVolume(float volume)
    {
        currentGameVolume = volume;
        foreach (Sound s in sounds)
        {
            s.UpdateVolume(volume);
        }
    }

    public void SetMusicVolume(float volume)
    {
        currentMusicVolume = volume;
        foreach (Sound s in musics)
        {
            s.UpdateVolume(volume);
        }
    }

    public void SetIndividualSoundVolume(string name, float volume)
    {
        Sound s = sounds.Find(sound => sound.name == name);
        if (s != null)
        {
            s.volume = volume;
            s.UpdateVolume(currentGameVolume);
        }
        else
        {
            s = musics.Find(music => music.name == name);
            if (s != null)
            {
                s.volume = volume;
                s.UpdateVolume(currentMusicVolume);
            }
            else
            {
                Debug.LogWarning("Sound or Music: " + name + " not found!");
            }
        }
    }

    public float GetIndividualSoundVolume(string name)
    {
        Sound s = sounds.Find(sound => sound.name == name) ?? musics.Find(music => music.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound or Music: " + name + " not found!");
            return 0f;
        }
        return s.volume;
    }
#endregion


#region Sounds
    public void PlaySound(string name, float delay = 0f)
    {
        Sound s = sounds.Find(sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }

        if (delay > 0)
        {
            StartCoroutine(PlayDelayed(s, delay));
        }
        else
        {
            s.source.Play();
        }
    }


    private void PlaySpecificSound(string soundName)
    {
        Sound s = sounds.Find(sound => sound.name == soundName);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + soundName + " not found!");
            return;
        }
        s.source.Play();
        // Debug.Log(soundName + " Sound Played");
    }

    private IEnumerator PlayDelayed(Sound sound, float delay)
    {
        yield return new WaitForSeconds(delay);
        sound.source.Play();
    }

    public void StopSound(string name)
    {
        Sound s = sounds.Find(sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }
        s.source.Stop();
    }



#endregion

#region External AudioSource
    public void PlayFromExternalSource(string name, AudioSource externalSource, float delay = 0f)
    {
        Sound s = sounds.Find(sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }

        if (delay > 0)
        {
            StartCoroutine(PlayDelayedFromExternalSource(s, externalSource, delay));
        }
        else
        {
            PlaySoundFromExternalSource(s, externalSource);
        }
    }

    private void PlaySoundFromExternalSource(Sound sound, AudioSource externalSource)
    {
        externalSource.clip = sound.clip;
        externalSource.volume = sound.volume * currentGameVolume;
        externalSource.pitch = sound.pitch;
        externalSource.loop = sound.loop;
        externalSource.Play();
    }

    private IEnumerator PlayDelayedFromExternalSource(Sound sound, AudioSource externalSource, float delay)
    {
        yield return new WaitForSeconds(delay);
        PlaySoundFromExternalSource(sound, externalSource);
    }

    public void StopFromExternalSource(string name, AudioSource externalSource)
    {
        Sound s = sounds.Find(sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }

        if (externalSource.clip == s.clip)
        {
            externalSource.Stop();
        }
    }

    public void UpdateExternalSourceVolume(AudioSource externalSource)
    {
        Sound s = sounds.Find(sound => sound.clip == externalSource.clip);
        if (s != null)
        {
            externalSource.volume = s.volume * currentGameVolume;
        }
    }
    #endregion


#region  Music
    public void PlayMusic(string name)
    {
        Sound newMusic = musics.Find(music => music.name == name);
        if (newMusic == null)
        {
            Debug.LogWarning("Music: " + name + " not found!");
            return;
        }

        StopAllMusic();
        newMusic.source.Play();
        // Debug.Log("Music: " + name + " played");
    }

    private void StopAllMusic()
    {
        foreach (Sound music in musics)
        {
            if (music.source.isPlaying)
            {
                music.source.Stop();
            }
        }
    }
#endregion

}