using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameSaveData
{
    public string playerName;

    public GameSaveData(GameSaveLoadFunctions p)
    {
        playerName = p.saveData.playerName;
    }
}
