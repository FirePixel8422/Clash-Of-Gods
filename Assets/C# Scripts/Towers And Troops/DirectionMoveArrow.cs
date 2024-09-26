using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectionMoveArrow : DirectionArrow
{
    private Troop troop;

    [HideInInspector]
    public SpriteRenderer spriteRenderer;


    public override void Start()
    {
        base.Start();
        troop = GetComponentInParent<Troop>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }


    public override void OnValidateArrow()
    {
        base.OnValidateArrow();

        GridObjectData troop_GridObjectData = GridManager.Instance.GridObjectFromWorldPoint(troop.transform.position);
        GridObjectData arrow_GridObjectData = GridManager.Instance.GetGridData(troop_GridObjectData.gridPos + dir);


        bool inGrid = GridManager.Instance.IsInGrid(arrow_GridObjectData.gridPos);

        bool validMovement = inGrid && (arrow_GridObjectData.full == false);


        interactable = (troop.movesLeft != 0 && validMovement) || validAttack;
        spriteRenderer.enabled = validMovement;
    }


    protected override void OnClick()
    {
        base.OnClick();

        if (TurnManager.Instance.isMyTurn == false || troop.stunned || troop.movesLeft == 0 || validAttack)
        {
            return;
        }

        GridObjectData gridObjectData = GridManager.Instance.GridObjectFromWorldPoint(troop.transform.position);


        troop.MoveTower(gridObjectData.gridPos, gridObjectData.gridPos + dir);

        troop.DeSelectTower();
    }
}