using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameSaveData
{
    public float volume;

    public int rWidth;
    public int rHeight;
    public bool fullScreen;

    public GameSaveData(GameSaveLoadFunctions p)
    {
        volume = p.saveData.volume;
        rWidth = p.saveData.rWidth;
        rHeight = p.saveData.rHeight;
        fullScreen = p.saveData.fullScreen;
    }
}
