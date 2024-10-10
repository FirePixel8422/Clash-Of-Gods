using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.iOS;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Singleton;
    private void Awake()
    {
        if (Singleton != null)
        {
            Destroy(Singleton);
        }
        Singleton = this;
    }

    public AudioSource musicPlayer;
    public AudioSource musicPlayerAlt;

    public bool altMusicPlayerActive;

    public AudioClip[] mainMenuClips;
    public AudioClip[] battleFieldClips;
    public int clipIndex;

    public AudioClip winMusicClip;
    public AudioClip loseMusicClip;

    private Coroutine queNextTrackCO;

    public float currentVolume;


    public void UpdateVolume(float main, float sfx, float music)
    {
        currentVolume = main * music;

        musicPlayer.volume = currentVolume;
        musicPlayerAlt.volume = currentVolume;
    }


    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void ChangeMusicTrack(bool mainMenu, float fadeSpeed, int winloseMusic = -1)
    {
        if (queNextTrackCO != null)
        {
            StopCoroutine(queNextTrackCO);
        }


        AudioClip clip;
        AudioClip queClip;

        if (winloseMusic != -1)
        {
            clip = battleFieldClips[clipIndex];

            if (winloseMusic == 1)
            {
                queClip = winMusicClip;
            }
            else
            {
                queClip = loseMusicClip;
            }
        }
        else if (mainMenu)
        {
            clip = mainMenuClips[clipIndex];

            clipIndex += 1;
            if (clipIndex >= mainMenuClips.Length)
            {
                clipIndex = 0;
            }

            queClip = mainMenuClips[clipIndex];
        }
        else
        {
            clip = battleFieldClips[clipIndex];

            clipIndex += 1;
            if (clipIndex >= battleFieldClips.Length)
            {
                clipIndex = 0;
            }

            queClip = battleFieldClips[clipIndex];
        }

        StartCoroutine(FadeChangeMusicTrack(clip, fadeSpeed));
        queNextTrackCO = StartCoroutine(QueNextTracktimer(queClip, clip.length, mainMenu, winloseMusic));
    }

    private IEnumerator QueNextTracktimer(AudioClip clip, float delay, bool mainMenu, int winloseMusic = -1)
    {
        yield return new WaitForSeconds(delay - 0.5f);
        StartCoroutine(FadeChangeMusicTrack(clip, 0.5f));

        AudioClip queClip;

        if (winloseMusic != -1)
        {
            if (winloseMusic == 1)
            {
                queClip = winMusicClip;
            }
            else
            {
                queClip = loseMusicClip;
            }
        }
        else if (mainMenu)
        {
            clipIndex += 1;
            if (clipIndex >= mainMenuClips.Length)
            {
                clipIndex = 0;
            }

            queClip = mainMenuClips[clipIndex];
        }
        else
        {
            clipIndex += 1;
            if (clipIndex >= battleFieldClips.Length)
            {
                clipIndex = 0;
            }

            queClip = battleFieldClips[clipIndex];
        }
        queNextTrackCO = StartCoroutine(QueNextTracktimer(queClip, clip.length, mainMenu, winloseMusic)); ;
    }

    private IEnumerator FadeChangeMusicTrack(AudioClip audioClip, float fadeSpeed)
    {
        AudioSource currentSource = altMusicPlayerActive ? musicPlayerAlt : musicPlayer;
        AudioSource altSource = altMusicPlayerActive ? musicPlayer : musicPlayerAlt;

        altSource.clip = audioClip;
        altSource.Play();

        while (currentSource.volume > 0)
        {
            currentSource.volume = Mathf.MoveTowards(currentSource.volume, 0, fadeSpeed * Time.deltaTime);
            altSource.volume = Mathf.MoveTowards(altSource.volume, currentVolume, fadeSpeed * Time.deltaTime);
            yield return null;
        }

        currentSource.Stop();

        altMusicPlayerActive = !altMusicPlayerActive;
    }
}
