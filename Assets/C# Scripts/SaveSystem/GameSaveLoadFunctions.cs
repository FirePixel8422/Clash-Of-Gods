using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSaveLoadFunctions : MonoBehaviour
{
    public static GameSaveLoadFunctions Instance;
    private void Awake()
    {
        Instance = this;
    }

    public GameSaveData saveData;


    public void Start()
    {
        GameSaveData data = SaveAndLoadGame.LoadInfo();
        if (data != null)
        {
            LoadDataFromFile(data);
        }
    }


    public void LoadDataFromFile(GameSaveData data)
    {
        saveData.playerName = data.playerName;

        PlayerNameHandler.Instance.LoadPlayerName(saveData.playerName);
    }

    public void SavePlayerName(string name)
    {
        saveData.playerName = name;
    }

    public void SaveDataToFile()
    {
        SaveAndLoadGame.SaveInfo(this);
    }
}
