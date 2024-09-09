using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerNameHandler : MonoBehaviour
{
    public static PlayerNameHandler Instance;
    private void Awake()
    {
        Instance = this;
    }



    public TMP_InputField playerNameField;
    public string playerName = "New Player";


    public void LoadPlayerName(string name)
    {
        playerName = name;
        playerNameField.text = name;
    }

    public void OnChangeName(string newName)
    {
        playerName = newName;

        string _playerName = playerName.Length < 8 ? playerName : playerName[..4] + "...";
        LobbyRelay.Instance.lobbyNameField.text = _playerName + "'s Lobby";

        GameSaveLoadFunctions.Instance.SavePlayerName(playerName);
        GameSaveLoadFunctions.Instance.SaveDataToFile();
    }
}
