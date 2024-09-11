using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementArrow : ClickableCollider
{
    private TowerCore troop;

    public Vector2Int dir;


    public override void Start()
    {
        base.Start();
        troop = GetComponentInParent<TowerCore>();
    }


    public void VaidateForVaildTile()
    {
        GridObjectData troop_GridObjectData = GridManager.Instance.GridObjectFromWorldPoint(troop.transform.position);
        GridObjectData arrow_GridObjectData = GridManager.Instance.GetGridData(troop_GridObjectData.gridPos + dir);

        gameObject.SetActive(GridManager.Instance.IsInGrid(arrow_GridObjectData.gridPos) && (arrow_GridObjectData.full == false));
    }


    public override void OnClick()
    {
        base.OnClick();

        if (TurnManager.Instance.isMyTurn == false)
        {
            return;
        }

        GridObjectData gridObjectData = GridManager.Instance.GridObjectFromWorldPoint(troop.transform.position);


        troop.MoveTower(gridObjectData.gridPos, gridObjectData.gridPos + dir);

        troop.towerMoveArrowsAnim.SetBool("Enabled", false);

        TurnManager.Instance.isMyTurn = false;
        TurnManager.Instance.NextTurn_ServerRPC();
    }
}