using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Obstacle : TowerCore
{
    private void Start()
    {
        CoreInit();

        underAttackArrowRenderer = underAttackArrowAnim.GetComponentInChildren<MeshRenderer>();
        underAttackArrowColors.Add(PlacementManager.Instance.playerColors[NetworkObject.OwnerClientId / 10 - 1]);
    }
}
