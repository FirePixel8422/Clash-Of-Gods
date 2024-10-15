using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Troop : TowerCore
{
    public float animatedMoveSpeed;
    public float animatedRotSpeed;
    public float attackAnimationTime;

    public int movesPerTurn;
    [HideInInspector]
    public int movesLeft;


    private SpriteRenderer[] moveArrowRenderers;
    public Color[] moveArrowColors;




    protected override void OnSetupTower()
    {
        selectStateAnim.transform.rotation = Quaternion.identity;

        GetComponentInChildren<DirectionArrowValidator>().Init(Mathf.RoundToInt(transform.rotation.y) == -90);

        moveArrowRenderers = selectStateAnim.GetComponentsInChildren<SpriteRenderer>();

        if (GodCore.Instance.IsAthena)
        {
            movesLeft = movesPerTurn;
        }
    }



    #region Tower Select/Deselect

    protected override void OnSelectTower()
    {
        foreach (var sprite in moveArrowRenderers)
        {
            if (movesLeft == 0)
            {
                sprite.color = moveArrowColors[0];
            }
            else
            {
                sprite.color = moveArrowColors[Mathf.Min(movesLeft, moveArrowColors.Length - 1)];
            }
        }
    }
    protected override void OnDeSelectTower()
    {
        
    }
    #endregion


    protected override void OnGetAttacked()
    {
        anim.SetTrigger("Hurt");

        SyncHurtAnimation_ServerRPC();
    }

    [ServerRpc(RequireOwnership = false)]
    private void SyncHurtAnimation_ServerRPC(ServerRpcParams rpcParams = default)
    {
        ulong senderClientId = rpcParams.Receive.SenderClientId;

        SyncHurtAnimation_ClientRPC(senderClientId);
    }

    [ClientRpc(RequireOwnership = false)]
    private void SyncHurtAnimation_ClientRPC(ulong clientId)
    {
        if (NetworkManager.LocalClientId == clientId)
        {
            return;
        }

        anim.SetTrigger("Hurt");
    }


    protected override void OnGrantTurn()
    {
        movesLeft = movesPerTurn;
    }


    protected override IEnumerator AttackTargetAnimation(Vector3 targetPos, float combinedSize, TowerCore target = null)
    {
        float maxDist = Vector3.Distance(transform.position, targetPos) - combinedSize;

        targetPos = VectorLogic.InstantMoveTowards(transform.position, targetPos, maxDist);

        Vector3 towerStartpos = transform.position;
        targetPos.y = towerStartpos.y;





        anim.SetTrigger("Attack");

        Vector3 direction = (targetPos - transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(direction);

        while (Quaternion.Angle(transform.rotation, targetRotation) > 0.0001f)
        {
            yield return null;

            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, animatedRotSpeed * Time.deltaTime);
            selectStateAnim.transform.rotation = Quaternion.identity;
        }

        


        anim.SetTrigger("MoveAttack");

        while (Vector3.Distance(transform.position, targetPos) > 0.0001f)
        {
            yield return null;

            transform.position = VectorLogic.InstantMoveTowards(transform.position, targetPos, animatedMoveSpeed * Time.deltaTime);
        }



        anim.SetTrigger("MoveAttack");

        yield return new WaitForSeconds(attackAnimationTime);

        while (Vector3.Distance(transform.position, towerStartpos) > 0.0001f)
        {
            yield return null;

            transform.position = VectorLogic.InstantMoveTowards(transform.position, towerStartpos, animatedMoveSpeed * Time.deltaTime);
        }





        anim.SetTrigger("MoveAttack");

        if (target != null)
        {
            target.GetAttacked(dmg, GodCore.Instance.RandomStunChance());
        }
    }


    #region Move Tower

    public void MoveTower(Vector2Int currentGridPos, Vector2Int newGridPos)
    {
        PlacementManager.Instance.playedAnything = true;

        movesLeft -= 1;

        GridManager.Instance.UpdateTowerData(currentGridPos, null);
        GridManager.Instance.UpdateTowerData(newGridPos, this);


        StartCoroutine(MoveTowerAnimation(currentGridPos, newGridPos));

        MoveTower_ServerRPC(currentGridPos, newGridPos);
    }


    [ServerRpc(RequireOwnership = false)]
    private void MoveTower_ServerRPC(Vector2Int currentGridPos, Vector2Int newGridPos, ServerRpcParams rpcParams = default)
    {
        ulong fromClientId = rpcParams.Receive.SenderClientId;
        MoveTower_ClientRPC(fromClientId, currentGridPos, newGridPos);
    }

    [ClientRpc(RequireOwnership = false)]
    private void MoveTower_ClientRPC(ulong fromClientId, Vector2Int currentGridPos, Vector2Int newGridPos)
    {
        if (TurnManager.Instance.localClientId != fromClientId)
        {
            StartCoroutine(MoveTowerAnimation(currentGridPos, newGridPos));
        }
    }

    private IEnumerator MoveTowerAnimation(Vector2Int currentGridPos, Vector2Int newGridPos)
    {
        anim.SetTrigger("Move");

        GridManager.Instance.UpdateTowerData(currentGridPos, null);
        GridManager.Instance.UpdateTowerData(newGridPos, this);


        Vector3 newPos = GridManager.Instance.GetGridData(newGridPos).worldPos;
        newPos = new Vector3(newPos.x, transform.position.y, newPos.z);


        Vector3 direction = (newPos - transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(direction);

        while (Quaternion.Angle(transform.rotation, targetRotation) > 0.0001f)
        {
            yield return null;

            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, animatedRotSpeed * Time.deltaTime);
            selectStateAnim.transform.rotation = Quaternion.identity;
        }

        while (Vector3.Distance(transform.position, newPos) > 0.001f)
        {
            yield return null;
            transform.position = VectorLogic.InstantMoveTowards(transform.position, newPos, animatedMoveSpeed * Time.deltaTime);
        }

        anim.SetTrigger("MoveEnd");
    }
    #endregion



    [ServerRpc(RequireOwnership = false)]
    public void EnhanceTroop_ServerRPC()
    {
        EnhanceTroop_ClientRPC();
    }

    [ClientRpc(RequireOwnership = false)]
    private void EnhanceTroop_ClientRPC()
    {
        dmg = (int)(dmg * GodCore.Instance.damageMultiplier);
        health = (int)(health * GodCore.Instance.healthMultiplier);

        movesLeft += GodCore.Instance.addedMoves;

        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        foreach (Renderer renderer in renderers)
        {
            renderer.material.SetInt("_GlowPower", 1);
        }
    }
}
