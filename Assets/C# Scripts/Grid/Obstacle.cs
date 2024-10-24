using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Obstacle : TowerCore
{
    public override void OnNetworkSpawn()
    {
        TurnManager.Instance.OnMyTurnStartedEvent.AddListener(() => GrantTurn());

        anim = GetComponent<Animator>();

        dissolves = GetComponentsInChildren<DissolveController>();

        underAttackArrowRenderer = underAttackArrowAnim.GetComponentInChildren<MeshRenderer>();
        underAttackArrowColors.Add(PlacementManager.Instance.playerColors[NetworkObject.OwnerClientId / 10 - 1]);
    }


    public override void OnDeath()
    {
        anim.SetTrigger("Death");
        foreach (var dissolve in dissolves)
        {
            dissolve.Revert(this);
        }
        GridObjectData gridObjectData = GridManager.Instance.GridObjectFromWorldPoint(transform.position);
        GridManager.Instance.UpdateTowerData(gridObjectData.gridPos, null);


        TurnManager.Instance.OnMyTurnStartedEvent.RemoveListener(() => GrantTurn());
        TurnManager.Instance.OnMyTurnEndedEvent.RemoveListener(() => OnTurnEnd());

        if (GodCore.Instance.chosenGods[OwnerClientId / 10 - 1] != (int)GodCore.God.Hades && GetComponent<PlayerBase>() == false)
        {
            TurnManager.Instance.OnTurnChangedEvent.RemoveListener(() => TurnChanged());
        }
    }
}
