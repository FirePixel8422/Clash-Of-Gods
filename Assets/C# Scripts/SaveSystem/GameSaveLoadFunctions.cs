using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class GameSaveLoadFunctions : MonoBehaviour
{
    public static GameSaveLoadFunctions Instance;
    private void Awake()
    {
        Instance = this;

        GameSaveData data = SaveAndLoadGame.LoadInfo();
        if (data != null)
        {
            LoadDataFromFile(data);
        }
    }

    public GameSaveData saveData;
    public AudioMixer audioMixer;


    public void LoadDataFromFile(GameSaveData data)
    {
        saveData.volume = data.volume;
        saveData.rWidth = data.rWidth;
        saveData.rHeight = data.rHeight;
        saveData.fullScreen = data.fullScreen;
    }

    public void SaveVolume(float volume)
    {
        saveData.volume = volume;
        audioMixer.SetFloat("Volume", Mathf.Log10(volume / 100) * 20);
    }

    public void SaveScreenData()
    {
        saveData.rWidth = Screen.currentResolution.width;
        saveData.rHeight = Screen.currentResolution.height;
        saveData.fullScreen = Screen.fullScreen;
    }

    private void OnDestroy()
    {
        SaveDataToFile();
    }

    public void SaveDataToFile()
    {
        SaveAndLoadGame.SaveInfo(this);
    }
}
