using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.VFX;


public class Hades : GodCore
{
    public Transform fireWallSelectionSprite;

    public GameObject[] fireWallEffectPrefabs;

    public float fwAnimationMoveSpeed;
    public int fireWallCharges;
    public int fireWallLifeTime;

    public List<GameObject> fireWallEffectList;
    public List<Vector2Int> fireWallEffectGridPosList;
    public List<int> fireWallEffectLifeTimeList;



    public Transform offensiveSelectionSprite;


    public VisualEffect[] fireEffectPrefabs;

    public int moltenFloorAmount;

    public int fireLifeTime;

    public float destroyDelay;
    public bool canSpawnOnFullTile;

    public List<VisualEffect> fireEffectList;
    public List<Vector2Int> fireEffectGridPosList;
    public List<int> fireEffectLifeTimeList;


    private Vector3 mousePos;
    private Camera mainCam;

    public GridObjectData selectedGridTileData;



    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;

        targetFireWallPos = fireWallSelectionSprite.position;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        mainCam = Camera.main;

        if(PlacementManager.Instance != null)
        {
            PlacementManager.Instance.OnConfirmEvent.AddListener(() => OnConfirm());
            PlacementManager.Instance.OnCancelEvent.AddListener(() => OnCancel());
        }

        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.OnMyTurnStartedEvent.AddListener(() => OnTurnChanged());
        }
    }




    public void OnConfirm()
    {
        if (TurnManager.Instance.isMyTurn == false)
        {
            return;
        }

        if (usingDefenseAbility)
        {
            Ray ray = mainCam.ScreenPointToRay(mousePos);
            if (Physics.Raycast(ray, out RaycastHit hitInfo, 100, PlacementManager.Instance.ownFieldLayers + PlacementManager.Instance.neutralLayers))
            {
                PlaceFireWall_ServerRPC(fireWallSelectionSprite.position);

                usingDefenseAbility = false;
                fireWallSelectionSprite.localPosition = Vector3.zero;
            }
        }
    }

    public void OnCancel()
    {
        if (TurnManager.Instance.isMyTurn == false)
        {
            return;
        }

        usingDefenseAbility = false;
        fireWallSelectionSprite.localPosition = Vector3.zero;
        usingOffensiveAbility = false;
        offensiveSelectionSprite.localPosition = Vector3.zero;
    }


    public void OnTurnChanged()
    {
        CheckForDiscardFireWall_ServerRPC();
        UseMoltenFloor_ServerRPC();
    }


    public bool usingDefenseAbility;
    public override void UseDefensiveAbility()
    {
        usingDefenseAbility = true;
        usingOffensiveAbility = false;

        offensiveSelectionSprite.localPosition = Vector3.zero;
    }

    public bool usingOffensiveAbility;
    public override void UseOffensiveAbility()
    {
        usingOffensiveAbility = true;
        usingDefenseAbility = false;

        fireWallSelectionSprite.localPosition = Vector3.zero;
    }




    #region MoltenFloor

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
                SetFireState_ClientRPC(fireEffectGridPosList[i], false);

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
                SetFireState_ClientRPC(gridTilesOwnField[rTile].gridPos, true);

                break;
            }
        }
    }

    private IEnumerator DestroyDelay(NetworkObject networkObject)
    {
        yield return new WaitForSeconds(destroyDelay);

        networkObject.Despawn(true);
    }
    #endregion


    private void Update()
    {
        UpdateSelectionSprite(Input.mousePosition != mousePos);
        mousePos = Input.mousePosition;
    }


    private Vector3 targetFireWallPos;
    private Vector3 savedFireWallpos;

    private void UpdateSelectionSprite(bool mouseMoved)
    {
        if (usingDefenseAbility && mouseMoved)
        {
            Ray ray = mainCam.ScreenPointToRay(mousePos);

            if (Physics.Raycast(ray, out RaycastHit hitInfo, 100, PlacementManager.Instance.ownFieldLayers + PlacementManager.Instance.neutralLayers))
            {
                selectedGridTileData = GridManager.Instance.GridObjectFromWorldPoint(hitInfo.point);

                float posZOffset = 0;

                if (selectedGridTileData.gridPos.y == (GridManager.Instance.gridSizeZ - 1))
                {
                    posZOffset = -GridManager.Instance.tileSize;
                }
                if (selectedGridTileData.gridPos.y == 0)
                {
                    posZOffset = GridManager.Instance.tileSize;
                }


                if (fireWallSelectionSprite.localPosition == Vector3.zero)
                {
                    fireWallSelectionSprite.position = selectedGridTileData.worldPos + new Vector3(0, 0, posZOffset);
                }
                else if (mouseMoved)
                {
                    targetFireWallPos = selectedGridTileData.worldPos + new Vector3(0, 0, posZOffset);
                    savedFireWallpos = fireWallSelectionSprite.position;
                }
            }
        }


        if (usingDefenseAbility)
        {
            float _fireWallMoveSpeed = fwAnimationMoveSpeed * (Vector3.Distance(savedFireWallpos, targetFireWallPos) / GridManager.Instance.tileSize);

            if (Vector3.Distance(fireWallSelectionSprite.position, targetFireWallPos) > 0.0001f)
            {
                fireWallSelectionSprite.position = VectorLogic.InstantMoveTowards(fireWallSelectionSprite.position, targetFireWallPos, _fireWallMoveSpeed * Time.deltaTime);
            }
        }


        if (usingOffensiveAbility)
        {
            Ray ray = mainCam.ScreenPointToRay(mousePos);

            if (Physics.Raycast(ray, out RaycastHit hitInfo, 100, PlacementManager.Instance.fullFieldLayers))
            {
                selectedGridTileData = GridManager.Instance.GridObjectFromWorldPoint(hitInfo.point);

                if (selectedGridTileData.type == (int)TurnManager.Instance.localClientId)
                {
                    offensiveSelectionSprite.position = selectedGridTileData.worldPos;
                }
            }
        }
    }
    


    #region Place/Discard FireWall On Network

    [ServerRpc(RequireOwnership = false)]
    private void CheckForDiscardFireWall_ServerRPC()
    {
        for (int i = 0; i < fireWallEffectList.Count; i++)
        {
            fireWallEffectLifeTimeList[i] -= 1;

            if (fireWallEffectLifeTimeList[i] <= 0)
            {
                foreach (VisualEffect vfx in fireWallEffectList[i].GetComponentsInChildren<VisualEffect>())
                {
                    vfx.Stop();
                }
                

                GridManager.Instance.UpdateGridDataOnFireState(fireWallEffectGridPosList[i], false);
                GridManager.Instance.UpdateGridDataOnFireState(fireWallEffectGridPosList[i] + Vector2Int.up, false);
                GridManager.Instance.UpdateGridDataOnFireState(fireWallEffectGridPosList[i] + Vector2Int.down, false);

                SetFireState_ClientRPC(fireWallEffectGridPosList[i], false);
                SetFireState_ClientRPC(fireWallEffectGridPosList[i] + Vector2Int.up, false);
                SetFireState_ClientRPC(fireWallEffectGridPosList[i] + Vector2Int.down, false);

                StartCoroutine(DestroyDelay(fireWallEffectList[i].GetComponent<NetworkObject>()));

                fireWallEffectList.RemoveAt(i);
                fireWallEffectGridPosList.RemoveAt(i);
                fireWallEffectLifeTimeList.RemoveAt(i);

                i--;
            }
        }
    }


    [ServerRpc(RequireOwnership = false)]
    private void PlaceFireWall_ServerRPC(Vector3 pos)
    {
        int rPrefab = Random.Range(0, fireWallEffectPrefabs.Length);

        GameObject effect = Instantiate(fireWallEffectPrefabs[rPrefab], pos, Quaternion.identity);
        effect.GetComponent<NetworkObject>().Spawn(true);

        Vector2Int gridPos = GridManager.Instance.GridObjectFromWorldPoint(pos).gridPos;

        fireWallEffectList.Add(effect);
        fireWallEffectGridPosList.Add(gridPos);
        fireWallEffectLifeTimeList.Add(fireWallLifeTime);


        SetFireState_ClientRPC(gridPos, true);
        SetFireState_ClientRPC(gridPos + Vector2Int.up, true);
        SetFireState_ClientRPC(gridPos + Vector2Int.down, true);
    }
    #endregion



    [ClientRpc(RequireOwnership = false)]
    private void SetFireState_ClientRPC(Vector2Int gridPos, bool newState)
    {
        GridManager.Instance.UpdateGridDataOnFireState(gridPos, newState);
    }
}