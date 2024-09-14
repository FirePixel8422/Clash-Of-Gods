using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectionArrow : ClickableCollider
{
    private Troop troop;
    private SpriteRenderer spriteRenderer;

    public Vector2Int dir;

    private bool validAttack;


    public override void Start()
    {
        base.Start();
        troop = GetComponentInParent<Troop>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }


    public void VaidateMovementAndAttacks()
    {
        GridObjectData troop_GridObjectData = GridManager.Instance.GridObjectFromWorldPoint(troop.transform.position);
        GridObjectData arrow_GridObjectData = GridManager.Instance.GetGridData(troop_GridObjectData.gridPos + dir);


        bool inGrid = GridManager.Instance.IsInGrid(arrow_GridObjectData.gridPos);

        bool validMovement = inGrid && (arrow_GridObjectData.full == false);

        validAttack = inGrid && arrow_GridObjectData.full && arrow_GridObjectData.tower.OwnerClientId != troop.OwnerClientId;


        if (validAttack)
        {
            gameObject.SetActive(false);

            troop.targets.Add(arrow_GridObjectData.tower);
            arrow_GridObjectData.tower.GetTargetted(true);
        }
        else
        {
            gameObject.SetActive(validMovement);
        }
    }


    public override void OnClick()
    {
        base.OnClick();

        if (TurnManager.Instance.isMyTurn == false)
        {
            return;
        }

        GridObjectData gridObjectData = GridManager.Instance.GridObjectFromWorldPoint(troop.transform.position);
        GridObjectData arrow_GridObjectData = GridManager.Instance.GetGridData(gridObjectData.gridPos + dir);


        if (validAttack)
        {
            arrow_GridObjectData.tower.GetAttacked();
        }
        else
        {
            troop.MoveTower(gridObjectData.gridPos, gridObjectData.gridPos + dir);
        }

        troop.DeSelectTower();

        TurnManager.Instance.isMyTurn = false;
        TurnManager.Instance.NextTurn_ServerRPC();
    }
}