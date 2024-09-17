using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.VFX;


public class Hades : GodCore
{
    public VisualEffect[] fireEffectPrefabs;

    public int moltenFloorAmount;

    public int fireLifeTime;

    public float destroyDelay;
    public bool canSpawnOnFullTile;

    public List<VisualEffect> fireEffectList;
    public List<Vector2Int> fireEffectGridPosList;
    public List<int> fireEffectLifeTimeList;



    private void Start()
    {
        god = God.Hades;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.OnMyTurnStartedEvent.AddListener(() => UseMoltenFloor_ServerRPC());
        }
    }



    [ServerRpc(RequireOwnership = false)]
    private void UseMoltenFloor_ServerRPC(ServerRpcParams rpcParams = default)
    {
        for (int i = 0; i < fireEffectList.Count; i++)
        {
            fireEffectLifeTimeList[i] -= 1;

            if (fireEffectLifeTimeList[i] <= 0)
            {
                fireEffectList[i].Stop();

                GridManager.Instance.UpdateGridDataOnFireState(fireEffectGridPosList[i], false);
                UseMoltenFloor_ClientRPC(fireEffectGridPosList[i], false);

                StartCoroutine(DestroyDelay(fireEffectList[i].GetComponent<NetworkObject>()));

                fireEffectList.RemoveAt(i);
                fireEffectGridPosList.RemoveAt(i);
                fireEffectLifeTimeList.RemoveAt(i);

                i--;
            }
        }



        List<GridObjectData> gridTilesOwnField = new List<GridObjectData>(GridManager.Instance.p1GridTiles.Count);

        if (rpcParams.Receive.SenderClientId == 0)
        {
            for (int i = 0; i < GridManager.Instance.p1GridTiles.Count; i++)
            {
                gridTilesOwnField.Add(GridManager.Instance.p1GridTiles[i]);
            }
        }
        else
        {
            for (int i = 0; i < GridManager.Instance.p2GridTiles.Count; i++)
            {
                gridTilesOwnField.Add(GridManager.Instance.p2GridTiles[i]);
            }
        }

        for (int i = 0; i < moltenFloorAmount; i++)
        {
            while (gridTilesOwnField.Count > 0)
            {
                int rTile = Random.Range(0, gridTilesOwnField.Count);

                if ((GridManager.Instance.GetGridData(gridTilesOwnField[rTile].gridPos).onFire > 0) 
                    || (GridManager.Instance.GetGridData(gridTilesOwnField[rTile].gridPos).full == true && canSpawnOnFullTile == false))
                {
                    gridTilesOwnField.RemoveAt(rTile);
                    continue;
                }

                int rPrefab = Random.Range(0, 2);

                Vector3 pos = gridTilesOwnField[rTile].worldPos;
                pos.y += fireEffectPrefabs[rPrefab].transform.position.y;

                VisualEffect effect = Instantiate(fireEffectPrefabs[rPrefab], pos, Quaternion.Euler(0, Random.Range(180, -180), 0));
                effect.GetComponent<NetworkObject>().Spawn(true);


                fireEffectList.Add(effect);
                fireEffectGridPosList.Add(gridTilesOwnField[rTile].gridPos);
                fireEffectLifeTimeList.Add(fireLifeTime);


                GridManager.Instance.UpdateGridDataOnFireState(gridTilesOwnField[rTile].gridPos, true);
                UseMoltenFloor_ClientRPC(gridTilesOwnField[rTile].gridPos, true);

                break;
            }
        }
    }

    private IEnumerator DestroyDelay(NetworkObject networkObject)
    {
        yield return new WaitForSeconds(destroyDelay);

        networkObject.Despawn(true);
    }

    [ClientRpc(RequireOwnership = false)]
    private void UseMoltenFloor_ClientRPC(Vector2Int gridPos, bool newState)
    {
        GridManager.Instance.UpdateGridDataOnFireState(gridPos, newState);
    }
}