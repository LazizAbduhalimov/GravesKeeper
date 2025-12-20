using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class SoundManager: MonoBehaviour
{
    public static SoundManager Instance => _instance;
    private static SoundManager _instance;
    
    [HideInInspector] public List<SoundGroup> Musics;
    [HideInInspector] public List<SoundGroup> FXSounds;
    [HideInInspector] public List<SoundGroup> UISounds;

    [HideInInspector] public AudioSource PlayingMusicSource;
    
    public void Init()
    {
        transform.parent = null;
        Musics = new List<SoundGroup>();
        FXSounds = new List<SoundGroup>();
        UISounds = new List<SoundGroup>();
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(this);
            return;
        }
        Destroy(gameObject);
    }
    
    public void PlayRandomMusic()
    {
        var random = Random.Range(0, Musics.Count);
        MarkAndPlayMusicSource(Musics[random].TakeFreeSource());
    }
    
    public void PlayMusic(string musicName)
    {
        if (TryGetAudioSource(musicName, Musics, out var music))
        {
            MarkAndPlayMusicSource(music);
        }
        else
        {
            Debug.LogError($"Music with name {musicName} not found");
        }
    }

    public void RestartMusic()
    {
        StopMusic();
        MarkAndPlayMusicSource(PlayingMusicSource);
    }

    public void StopMusic()
    {
        if (PlayingMusicSource != null && PlayingMusicSource.isPlaying)
            PlayingMusicSource.Stop();
    }

    public void PlayFX(string fxName, Vector3? position = null)
    {
        // if (TryGetAudioSource(fxName, FXSounds, out var fx, position))
        // {
        //     fx.PlayOneShot(fx.clip);
        //     if (position != null)
        //         fx.transform.position = position.Value;
        // }
        // else
        // {
        //     Debug.LogError($"Sound FX with name {fxName} not found");
        // }
    }

    public void PlayUISound(string uiSoundName, Vector3? position = null)
    {
        if (TryGetAudioSource(uiSoundName, UISounds, out var uiSound, position))
        {
            uiSound.PlayOneShot(uiSound.clip);
            if (position != null)
                uiSound.transform.position = position.Value;
        }
        else
        {
            Debug.LogError($"UI Sound with name {uiSoundName} not found");
        }
    }

    private bool TryGetAudioSource(string soundName, IEnumerable<SoundGroup> container, out AudioSource sound,
        Vector3? playPoint = null)
    {
        sound = null;
        var soundGroup = container.FirstOrDefault(sound => sound.Name == soundName);
        var hasGroup = soundGroup != null;
        if (hasGroup)
            sound = soundGroup.TakeFreeSource(playPoint);
    
        return hasGroup;
    }
    
    private void MarkAndPlayMusicSource(AudioSource audioSource)
    {
        StopMusic();
        PlayingMusicSource = audioSource;
        PlayingMusicSource.Play();
    }
    
}
