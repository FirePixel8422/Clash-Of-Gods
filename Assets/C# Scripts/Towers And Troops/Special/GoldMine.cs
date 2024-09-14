using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoldMine : TowerCore
{
    public float mineSpeed;


    protected override void OnSetupTower()
    {
        TurnManager.Instance.OnTurnChangedEvent.AddListener(() => GenerateCoins());
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
}
