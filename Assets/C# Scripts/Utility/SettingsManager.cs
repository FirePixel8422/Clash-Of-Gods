using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingsManager : NetworkBehaviour
{
    public GameObject settingsMenu;

    public TMP_Dropdown dropdown;

    public Resolution[] resolutions;
    public List<Resolution> filterdResolutionList;

    private int cresolutionIndex;
    public RefreshRate cRefreshRate;

    public Slider audioSlider;

    public TextMeshProUGUI fullscreenButtonText;

    public bool displayRefreshRate;


    public void ChangeFullScreenState()
    {
        bool newState = !Screen.fullScreen;
        Screen.fullScreen = newState;

        fullscreenButtonText.text = newState ? "Go Windowed" : "Go Fullscreen";
    }

    private void Start()
    {
        StartCoroutine(FrameDelay());
    }
    private IEnumerator FrameDelay()
    {
        yield return new WaitForEndOfFrame();

        if (GameSaveLoadFunctions.Instance.saveData.rWidth != 0)
        {
            Screen.SetResolution(GameSaveLoadFunctions.Instance.saveData.rWidth, GameSaveLoadFunctions.Instance.saveData.rHeight, GameSaveLoadFunctions.Instance.saveData.fullScreen);

            audioSlider.value = GameSaveLoadFunctions.Instance.saveData.volume;
        }
        else
        {
            GameSaveLoadFunctions.Instance.SaveScreenData();
            Debug.LogError("NO DATA FOUND");

            GameSaveLoadFunctions.Instance.SaveVolume(100);
        }


        fullscreenButtonText.text = Screen.fullScreen ? "Go Windowed" : "Go Fullscreen";

        audioSlider.onValueChanged.AddListener((float value) => GameSaveLoadFunctions.Instance.SaveVolume(value));



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
  


    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = filterdResolutionList[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);

        GameSaveLoadFunctions.Instance.SaveScreenData();

        Debug.LogError(resolution.width + " + " + resolution.height);
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
        NetworkManager.Shutdown();

        Destroy(GodCore.Instance.gameObject);
        Destroy(GameSaveLoadFunctions.Instance.gameObject);

        SceneManager.LoadScene(0);
    }
}
