using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class LoseWinGame : MonoBehaviour
{
    public static LoseWinGame Instance;
    private void Awake() 
    {
        Instance = this;
    }




    public GameObject winLoseScreen;

    public GameObject winGameTextObj;
    public GameObject loseGameTextObj;



    [ServerRpc(RequireOwnership = false)]
    public void WinLoseGame_ServerRPC(ulong playerIdToWin)
    {
        WinLoseGame_ClientRPC(playerIdToWin);
    }


    [ClientRpc(RequireOwnership = false)]
    public void WinLoseGame_ClientRPC(ulong playerIdToWin)
    {
        winLoseScreen.SetActive(true);

        if (TurnManager.Instance.localClientId == playerIdToWin)
        {
            winGameTextObj.SetActive(true);
            MusicManager.Singleton.ChangeMusicTrack(false, 0.5f, 1);
        }
        else
        {
            loseGameTextObj.SetActive(true);
            MusicManager.Singleton.ChangeMusicTrack(false, 0.5f, 2);
        }
    }
}
