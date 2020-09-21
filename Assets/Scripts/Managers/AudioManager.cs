using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class AudioManager : MonoBehaviour
{
    #region Instance
    public static AudioManager singleton;
    #endregion

    #region Fields
    AudioSource musicSource, sfxSource;
    float musicVolume;
    Coroutine stopCoroutine;
    public float currentSeek;
    #endregion

    #region Audio Clips
    [Header("Sound Effects")]
    public AudioClip popSound;
    public AudioClip missSound;
    public List<AudioClip> songList = new List<AudioClip>();
    #endregion

    #region Audio Files
    public int currentlySelectedSongIndex;
    public static string folderPath = "c://Mstar Unity/Songs/";
    public static DirectoryInfo dir = new DirectoryInfo(folderPath);
    FileInfo[] file = dir.GetFiles("*.*");
    [HideInInspector] public List<string> songName = new List<string>();
    string tempName;
    #endregion

    void Start()
    {
        if (singleton == null)
        {
            singleton = FindObjectOfType<AudioManager>();
            if (singleton == null)
            {
                singleton = new GameObject("Spawned AudioManager", typeof(AudioManager)).GetComponent<AudioManager>();
            }
            DontDestroyOnLoad(gameObject); // let this object keep active throughout scenes
        }
        else
        {
            Destroy(gameObject);
        }

        // Create audio sources, and save them as references
        musicSource = gameObject.AddComponent<AudioSource>();
        sfxSource = gameObject.AddComponent<AudioSource>();

        musicSource.loop = false;
        musicVolume = 1;
        currentlySelectedSongIndex = 0;
        currentSeek = 0;

        ReadSongFiles();
    }

    private void Update()
    {
        currentSeek = GetSeek();
    }

    #region global functions
    public bool IsPlaying()
    {
        return musicSource.isPlaying;
    }
    public void PlayMusicFade(AudioClip musicClip)
    {
        StartCoroutine(FadeInPlayMusic(musicClip, 1f));
    }
    public void PlayMusic(AudioClip musicClip)
    {
        if (stopCoroutine != null)
            StopCoroutine(stopCoroutine);
        musicSource.clip = musicClip;
        musicSource.volume = musicVolume;
        musicSource.Play();
        musicSource.time = 0;
    }
    public void PlayCurrentlySelectedMusic()
    {
        try
        {
            PlayMusic(songList[currentlySelectedSongIndex]);
        }
        catch
        {

        }
    }
    public void PauseMusic()
    {
        musicSource.Pause();
    }
    public void UnpauseMusic()
    {
        musicSource.UnPause();
    }
    public void StopMusic(float fadeOutDuration)
    {
        stopCoroutine = StartCoroutine(FadeOutStopMusic(fadeOutDuration));
    }
    public void StopMusic()
    {
        musicSource.Stop();
    }
    IEnumerator FadeInPlayMusic(AudioClip musicClip, float transitionTime)
    {
        musicSource.clip = musicClip;
        musicSource.Play();
        musicSource.time = 0;

        for (float t = 0; t < transitionTime; t += Time.deltaTime)
        {
            musicSource.volume = (t / transitionTime) * musicVolume;
            yield return null;
        }
    }
    IEnumerator FadeOutStopMusic(float transitionTime)
    {
        for (float t = 0; t < transitionTime; t += Time.deltaTime)
        {
            musicSource.volume = musicVolume - ((t / transitionTime) * musicVolume);
            yield return null;
        }
        musicSource.Stop();
    }
    public void PlayMusicWithFade(AudioClip newClip, float transitionTime = 1)
    {
        StartCoroutine(UpdateMusicWithFade(musicSource, newClip, transitionTime));
    }
    IEnumerator UpdateMusicWithFade(AudioSource activeSource, AudioClip newClip, float transitionTime)
    {
        if (!activeSource.isPlaying)
            activeSource.Play();

        float t = 0;

        // fade out
        for (t = 0; t < transitionTime; t += Time.deltaTime)
        {
            activeSource.volume = musicVolume - ((t / transitionTime) * musicVolume);
            yield return null;
        }

        activeSource.Stop();
        activeSource.clip = newClip;
        activeSource.Play();

        // fade in
        for (t = 0; t < transitionTime; t += Time.deltaTime)
        {
            activeSource.volume = (t / transitionTime) * musicVolume;
            yield return null;
        }
    }

    public void PlaySFX(AudioClip clip)
    {
        sfxSource.PlayOneShot(clip);
    }
    public void PlaySFX(AudioClip clip, float volume)
    {
        sfxSource.PlayOneShot(clip, volume);
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = volume;
        musicSource.volume = volume;
    }
    public void SetSFXVolume(float volume)
    {
        sfxSource.volume = volume;
    }

    public float GetSeek()
    {
        if (musicSource == null)
            return 0;
        return musicSource.time;
    }
    public void SetSeek(float time)
    {
        musicSource.time = time;
    }
    public float GetSongLength()
    {
        return musicSource.clip.length;
    }
    #endregion

    #region custom funtions
    public void PlayPop()
    {
        PlaySFX(popSound);
    }
    public void PlayMiss()
    {
        PlaySFX(missSound);
    }
    public string CurrentlySelectedSongName()
    {
        return songName[currentlySelectedSongIndex];
    }
    public void ReadSongFiles()
    {
        file = dir.GetFiles("*.*");
        songName.Clear();
        songList.Clear();
        int count = 0;
        foreach (FileInfo f in file)
        {
            string[] nameWithoutExtension = f.Name.Split('.');

            tempName = nameWithoutExtension[0];
            songName.Add(tempName);
            StartCoroutine(AssignSongs(folderPath + tempName, count));
            count++;
        }
    }
    IEnumerator AssignSongs(string tempPath, int index)
    {
        string prefix = "file:///" + tempPath + ".wav";
        WWW www = new WWW(prefix);
        yield return www;
        if(index < songList.Count)
        {
            songList.Insert(index, www.GetAudioClip());
        }
        else
        {
            songList.Add(www.GetAudioClip());
        }
    }
    #endregion
}