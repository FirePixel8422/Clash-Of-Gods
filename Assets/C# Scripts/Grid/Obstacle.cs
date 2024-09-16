using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Obstacle : TowerCore
{
    private void Start()
    {
        underAttackArrowRenderer = underAttackArrowAnim.GetComponentInChildren<MeshRenderer>();
        underAttackArrowColors.Add(PlacementManager.Instance.playerColors[NetworkObject.OwnerClientId / 10 - 1]);

        dissolves = GetComponentsInChildren<DissolveController>();

        amountOfDissolves = dissolves.Length;
    }
}
