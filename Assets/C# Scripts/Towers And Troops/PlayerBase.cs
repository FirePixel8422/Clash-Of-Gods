using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerBase : TowerCore
{
    public Transform[] tileTransforms;

    private int maxHealth;
    public float addedDissolve;

    private void Start()
    {
        CoreInit();
        maxHealth = health;
    }

    public override IEnumerator GetAttackedAnimations(int dmg, bool stun)
    {
        StartCoroutine(base.GetAttackedAnimations(dmg, stun));

        foreach (var dissolve in dissolves)
        {
            dissolve.RevertPercent((float)health / (float)maxHealth + addedDissolve);
        }

        yield break;
    }


    public override void RevertCompleted()
    {
        base.RevertCompleted();

        LoseWinGame.Instance.WinLoseGame_ServerRPC((ulong)(OwnerClientId == 1 ? 0 : 1));
    }
}
