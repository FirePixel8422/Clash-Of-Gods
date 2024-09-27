using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectionArrow : ClickableCollider
{
    private TowerCore tower;

    public Vector2Int dir;

    [HideInInspector]
    public bool validAttack;


    public void Setup(Vector2Int _dir, Sprite sprite)
    {
        if (sprite != null)
        {
            GetComponent<SpriteRenderer>().sprite = sprite;
        }

        dir = _dir;

        tower = GetComponentInParent<TowerCore>();
    }


    public virtual void OnValidateArrow()
    {
        GridObjectData troop_GridObjectData = GridManager.Instance.GridObjectFromWorldPoint(tower.transform.position);
        GridObjectData arrow_GridObjectData = GridManager.Instance.GetGridData(troop_GridObjectData.gridPos + dir);


        bool inGrid = GridManager.Instance.IsInGrid(arrow_GridObjectData.gridPos);

        validAttack = inGrid && arrow_GridObjectData.full && arrow_GridObjectData.tower != null && arrow_GridObjectData.tower.OwnerClientId != tower.OwnerClientId;


        if (validAttack)
        {
            tower.targets.Add(arrow_GridObjectData.tower);
            arrow_GridObjectData.tower.GetTargetted(true, tower.actionsLeft != 0);
        }
    }


    protected override void OnClick()
    {
        base.OnClick();

        if (TurnManager.Instance.isMyTurn == false || tower.actionsLeft == 0 || tower.stunned || validAttack == false)
        {
            return;
        }

        GridObjectData gridObjectData = GridManager.Instance.GridObjectFromWorldPoint(tower.transform.position);
        GridObjectData arrow_GridObjectData = GridManager.Instance.GetGridData(gridObjectData.gridPos + dir);


        tower.AttackTarget(arrow_GridObjectData.tower);

        tower.DeSelectTower();

        tower.LoseAction();
    }
}