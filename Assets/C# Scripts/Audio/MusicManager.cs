using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;
    private void Awake()
    {
        Instance = this;
    }

    public AudioSource musicPlayer;
    public AudioSource musicPlayerAlt;

    public bool altMusicPlayerActive;

    public AudioClip[] mainMenuClips;
    public AudioClip[] battleFieldClips;
    private int clipIndex;

    public AudioClip winMusicClip;
    public AudioClip loseMusicClip;

    private Coroutine queNextTrackCO;


    public void UpdateVolume(float main, float sfx, float music)
    {
        musicPlayer.volume = main;
        musicPlayer.volume *= music;
        musicPlayerAlt.volume = main;
        musicPlayerAlt.volume *= music;
    }


    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        clipIndex = Random.Range(0, mainMenuClips.Length);

        ChangeMusicTrack(true, 0.5f);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode arg1)
    {
        if (scene.name == "MainGame")
        {
            ChangeMusicTrack(false, 0.5f);
        }
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
        queNextTrackCO = StartCoroutine(QueNextTracktimer(queClip, queClip.length, mainMenu, winloseMusic));
    }

    private IEnumerator QueNextTracktimer(AudioClip clip, float delay, bool mainMenu, int winloseMusic = -1)
    {
        yield return new WaitForSeconds(delay - 0.5f);
        StartCoroutine(FadeChangeMusicTrack(clip, 0.5f));


        if (winloseMusic != -1)
        {
            if (winloseMusic == 1)
            {
                clip = winMusicClip;
            }
            else
            {
                clip = loseMusicClip;
            }
        }
        else if (mainMenu)
        {
            clipIndex += 1;
            if (clipIndex >= mainMenuClips.Length)
            {
                clipIndex = 0;
            }

            clip = mainMenuClips[clipIndex];
        }
        else
        {
            clipIndex += 1;
            if (clipIndex >= battleFieldClips.Length)
            {
                clipIndex = 0;
            }

            clip = battleFieldClips[clipIndex];
        }
        queNextTrackCO = StartCoroutine(QueNextTracktimer(clip, clip.length, mainMenu, winloseMusic)); ;
    }

    private IEnumerator FadeChangeMusicTrack(AudioClip audioClip, float fadeSpeed)
    {
        AudioSource currentSource = altMusicPlayerActive ? musicPlayerAlt : musicPlayer;
        AudioSource altSource = altMusicPlayerActive ? musicPlayer : musicPlayerAlt;

        altSource.clip = audioClip;
        altSource.Play();

        while (currentSource.volume > 0)
        {
            currentSource.volume -= fadeSpeed * 0.05f;
            altSource.volume += fadeSpeed * 0.05f;
            yield return new WaitForSeconds(0.05f);
        }

        currentSource.Stop();

        altMusicPlayerActive = !altMusicPlayerActive;
    }
}
