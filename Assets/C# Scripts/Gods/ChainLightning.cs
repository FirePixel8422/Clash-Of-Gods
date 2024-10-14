using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.VFX;



public class ChainLightning : NetworkBehaviour
{
    private Vector3 prevPos;

    public int startDmg;
    public int maxChains;
    public bool applyZeusStunPassive;

    public Vector2Int[] directions;
    public PrioritizeMode prioritizeMode;

    public enum PrioritizeMode
    {
        Random,
        Closest,
        ClosestIncludeDiagonals
    }


    public float moveSpeed;
    public float chainDelay;

    public float damageDelay;


    public Transform rotTransform;
    public Vector3 coneOffset;

    public void Init()
    {
        prevPos = transform.position;

        StartCoroutine(ChainLogic(GridManager.Instance.GridObjectFromWorldPoint(transform.position)));
    }

    private void LateUpdate()
    {
        Vector3 movementDirection = transform.position - prevPos;

        if (movementDirection.sqrMagnitude > 0.0001f)
        {
            movementDirection.Normalize();

            Quaternion targetRotation = Quaternion.LookRotation(movementDirection);

            rotTransform.rotation = targetRotation;
            rotTransform.position = targetRotation * coneOffset;
        }
        prevPos = transform.position;
    }



    private IEnumerator ChainLogic(GridObjectData startTile)
    {
        List<GridObjectData> alreadyChainedList = new List<GridObjectData>()
        {
            startTile,
        };

        GridObjectData currentTile = startTile;

        for (int i = 0; i < maxChains; i++)
        {
            int currentDamage = startDmg / maxChains * (maxChains - i);

            List<GridObjectData> chainOptions = new List<GridObjectData>();
            int closestTargetsDist = 1000;

            foreach (Vector2Int direction in directions)
            {

                if (GridManager.Instance.IsInGrid(currentTile.gridPos + direction))
                {

                    GridObjectData newCurrentTile = GridManager.Instance.GetGridData(currentTile.gridPos + direction);

                    if (newCurrentTile.tower != null && newCurrentTile.tower.GetComponent<Obstacle>() == null && newCurrentTile.tower.OwnerClientId != OwnerClientId && alreadyChainedList.Contains(newCurrentTile) == false)
                    {
                        switch (prioritizeMode)
                        {
                            case PrioritizeMode.Random:

                                chainOptions.Add(newCurrentTile);
                                break;

                            case PrioritizeMode.Closest:

                                if (Mathf.Max(direction.x, direction.y) <= closestTargetsDist)
                                {
                                    chainOptions.Add(newCurrentTile);

                                    closestTargetsDist = Mathf.Max(direction.x, direction.y);
                                }
                                break;

                            case PrioritizeMode.ClosestIncludeDiagonals:

                                if ((direction.x + direction.y) <= closestTargetsDist)
                                {
                                    chainOptions.Add(newCurrentTile);

                                    closestTargetsDist = direction.x + direction.y;
                                }
                                break;
                        }
                    }
                }
            }

            if (chainOptions.Count == 0)
            {
                break;
            }

            currentTile = chainOptions[Random.Range(0, chainOptions.Count)];
            Vector3 targetPos = currentTile.tower.centerPoint.position;

            SyncBallPos_ServerRPC(targetPos);

            yield return StartCoroutine(MoveBall(targetPos));


            StartCoroutine(DamageDelay(currentTile, currentDamage));


            yield return new WaitForSeconds(chainDelay);
        }

        VisualEffect[] vfxs = GetComponentsInChildren<VisualEffect>();
        foreach (VisualEffect effect in vfxs)
        {
            effect.Stop();
        }
    }



    private IEnumerator DamageDelay(GridObjectData currentTile, int currentDamage)
    {
        yield return new WaitForSeconds(damageDelay);

        currentTile.tower.GetAttacked(currentDamage, applyZeusStunPassive && GodCore.Instance.RandomStunChance());
    }



    [ServerRpc(RequireOwnership = false)]
    private void SyncBallPos_ServerRPC(Vector3 targetPos, ServerRpcParams rpcParams = default)
    {
        ulong fromClientId = rpcParams.Receive.SenderClientId;
        SyncBallPos_ClientRPC(fromClientId, targetPos);
    }

    [ClientRpc(RequireOwnership = false)]
    private void SyncBallPos_ClientRPC(ulong fromClientId, Vector3 targetPos)
    {
        if (TurnManager.Instance.localClientId == fromClientId)
        {
            return;
        }
        StartCoroutine(MoveBall(targetPos));
    }

    private IEnumerator MoveBall(Vector3 targetPos)
    {
        while (Vector3.Distance(transform.position, targetPos) > 0.0001f)
        {
            yield return null;

            transform.position = VectorLogic.InstantMoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
        }
    }
}
