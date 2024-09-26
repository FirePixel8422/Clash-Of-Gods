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
}
