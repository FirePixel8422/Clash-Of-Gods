using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using Unity.Services.Lobbies;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingsManager : NetworkBehaviour
{
    public static SettingsManager SingleTon;
    private void Awake()
    {
        if (SingleTon != null)
        {
            Destroy(SingleTon);
        }
        SingleTon = this;
    }



    public GameObject settingsMenu;

    public TMP_Dropdown dropdown;

    public Resolution[] resolutions;
    public List<Resolution> filterdResolutionList;

    private int cresolutionIndex;
    public RefreshRate cRefreshRate;

    public Slider mainAudioSlider;
    public Slider sfxAudioSlider;
    public Slider musicAudioSlider;

    public TextMeshProUGUI fullscreenButtonText;

    public bool displayRefreshRate;

    private List<AudioController> audioControllers;


    public void AddAudioController(AudioController toAdd)
    {
        audioControllers.Add(toAdd);

        toAdd.UpdateVolume(mainAudioSlider.value, sfxAudioSlider.value, musicAudioSlider.value);
    }
    public void RemoveAudioController(AudioController toRemove)
    {
        audioControllers.Remove(toRemove);
    }


    public void ChangeFullScreenState()
    {
        bool newState = !Screen.fullScreen;

        Screen.fullScreen = newState;
        GameSaveLoadFunctions.Instance.SaveScreenData(GameSaveLoadFunctions.Instance.saveData.rWidth, GameSaveLoadFunctions.Instance.saveData.rHeight, newState);

        fullscreenButtonText.text = newState ? "Go Windowed" : "Go Fullscreen";
    }

    private void Start()
    {
        StartCoroutine(FrameDelay());
    }

    private IEnumerator FrameDelay()
    {
        yield return new WaitForEndOfFrame();
        yield return null;

        if (GameSaveLoadFunctions.Instance.saveData.rWidth != 0)
        {
            Screen.SetResolution(GameSaveLoadFunctions.Instance.saveData.rWidth, GameSaveLoadFunctions.Instance.saveData.rHeight, GameSaveLoadFunctions.Instance.saveData.fullScreen);

            mainAudioSlider.value = GameSaveLoadFunctions.Instance.saveData.mainVolume;
            sfxAudioSlider.value = GameSaveLoadFunctions.Instance.saveData.sfxVolume;
            musicAudioSlider.value = GameSaveLoadFunctions.Instance.saveData.musicVolume;

            audioControllers = FindObjectsOfType<AudioController>().ToList();

            UpdateVolume(true);
        }
        else
        {
            GameSaveLoadFunctions.Instance.SaveScreenData(1920, 1080, true);

            GameSaveLoadFunctions.Instance.SaveVolume(1, 1, 1);
        }

        if (SceneManager.GetActiveScene().name == "MainGame")
        {
            MusicManager.Singleton.clipIndex = Random.Range(0, MusicManager.Singleton.battleFieldClips.Length);
            MusicManager.Singleton.ChangeMusicTrack(false, 0.5f);
        }
        if (SceneManager.GetActiveScene().name == "Marijn")
        {
            MusicManager.Singleton.clipIndex = Random.Range(0, MusicManager.Singleton.mainMenuClips.Length);
            MusicManager.Singleton.ChangeMusicTrack(true, 0.5f);
        }


        fullscreenButtonText.text = Screen.fullScreen ? "Go Windowed" : "Go Fullscreen";

        mainAudioSlider.onValueChanged.AddListener((_) => UpdateVolume());
        sfxAudioSlider.onValueChanged.AddListener((_) => UpdateVolume());
        musicAudioSlider.onValueChanged.AddListener((_) => UpdateVolume());



        resolutions = Screen.resolutions;
        filterdResolutionList = new List<Resolution>();

        dropdown.ClearOptions();
        cRefreshRate = Screen.currentResolution.refreshRateRatio;

        for (int i = 0; i < resolutions.Length; i++)
        {
            bool isSixteenNineRatio = Mathf.Approximately((float)resolutions[i].width / (float)resolutions[i].height, 1.7777777777777777777777777777778f);
            if (resolutions[i].refreshRateRatio.Equals(cRefreshRate))
            {
                filterdResolutionList.Add(resolutions[i]);
            }
        }


        List<string> options = new List<string>();
        for (int i = 0; i < filterdResolutionList.Count; i++)
        {
            float refreshRate = (float)filterdResolutionList[i].refreshRateRatio.numerator / filterdResolutionList[i].refreshRateRatio.denominator;

            string resolutionOption = filterdResolutionList[i].width + " x " + filterdResolutionList[i].height + (displayRefreshRate ? (" " + refreshRate + "Hz") : "");

            options.Add(resolutionOption);
        }
        filterdResolutionList.Reverse();
        options.Reverse();

        dropdown.AddOptions(options);

        for (int i = 0; i < filterdResolutionList.Count; i++)
        {
            if (filterdResolutionList[i].width == Screen.width && filterdResolutionList[i].height == Screen.height)
            {
                cresolutionIndex = i;
                break;
            }
        }

        dropdown.value = cresolutionIndex;
        dropdown.captionText.text = Screen.width + "x" + Screen.height + " " + Screen.currentResolution.refreshRateRatio + "Hz";

        dropdown.RefreshShownValue();
    }



    public void UpdateVolume(bool updateSourceOnly = false)
    {
        foreach (AudioController audioController in audioControllers)
        {
            audioController.UpdateVolume(mainAudioSlider.value, sfxAudioSlider.value, musicAudioSlider.value);
        }
        MusicManager.Singleton.UpdateVolume(mainAudioSlider.value, sfxAudioSlider.value, musicAudioSlider.value);

        if (updateSourceOnly)
        {
            return;
        }

        GameSaveLoadFunctions.Instance.SaveVolume(mainAudioSlider.value, sfxAudioSlider.value, musicAudioSlider.value);
    }
  


    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = filterdResolutionList[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);

        GameSaveLoadFunctions.Instance.SaveScreenData(resolution.width, resolution.height, Screen.fullScreen);
    }


    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        EditorApplication.ExitPlaymode();
#endif
    }


    public void QuitToMainMenu()
    {
        KillMatch_ServerRPC();
    }

    [ServerRpc(RequireOwnership = false)]
    private void KillMatch_ServerRPC()
    {
        KillMatch_ClientRPC();
    }

    [ClientRpc(RequireOwnership = false)]
    private void KillMatch_ClientRPC()
    {
        Destroy(GodCore.Instance.gameObject);
        Destroy(LobbyRelay.Instance.gameObject);
        Destroy(NetworkManager.gameObject);
        Destroy(MusicManager.Singleton.gameObject);

        NetworkManager.Shutdown();
        Lobbies.Instance.DeleteLobbyAsync(LobbyRelay.Instance._lobbyId);

        SceneManager.LoadScene(0);
    }
}
