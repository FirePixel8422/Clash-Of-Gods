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
        health -= dmg;
        stunned = stun;

        underAttackArrowAnim.SetBool("Enabled", false);

        foreach (var dissolve in dissolves)
        {
            dissolve.RevertPercent((float)health / (float)maxHealth + addedDissolve);
        }


        yield return null;

        if (health <= 0)
        {
            OnDeath();
        }
    }


    public override void RevertCompleted()
    {
        base.RevertCompleted();

        LoseWinGame.Instance.WinLoseGame_ServerRPC((ulong)(OwnerClientId == 1 ? 0 : 1));
    }
}
