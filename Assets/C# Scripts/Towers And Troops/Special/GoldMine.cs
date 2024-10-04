using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoldMine : TowerCore
{
    public float mineSpeed;

    public int coinsOnDeath;


    protected override void OnSetupTower()
    {
        if (NetworkManager.LocalClientId == NetworkObject.OwnerClientId)
        {
            TurnManager.Instance.OnMyTurnStartedEvent.AddListener(() => GenerateCoins());
        }
    }



    public float generatedCoins = 0;

    public void GenerateCoins()
    {
        generatedCoins += mineSpeed;

        if (generatedCoins >= 1)
        {
            PlacementManager.Instance.Currency += (int)generatedCoins;
            generatedCoins -= (int)generatedCoins;
        }
    }

    public override void OnDeath()
    {
        if (NetworkManager.LocalClientId != NetworkObject.OwnerClientId)
        {
            PlacementManager.Instance.Currency += coinsOnDeath;
        }
    }
}
