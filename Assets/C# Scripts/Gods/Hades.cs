using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.VFX;


public class Hades : NetworkBehaviour
{
    public Sprite[] uiSprites;
    public int[] abilityCooldowns;


    public Transform fireWallSelectionSprite;

    public GameObject[] fireWallEffectPrefabs;

    public float fwAnimationMoveSpeed;
    public int fireWallLifeTime;

    private List<GameObject> fireWallEffectList;
    private List<Vector2Int> fireWallEffectGridPosList;
    private List<int> fireWallEffectLifeTimeList;



    public Transform meteorSelectionSprite;


    public VisualEffect[] fireEffectPrefabs;

    public int moltenFloorAmount;

    public int fireLifeTime;

    public float destroyDelay;
    public bool canSpawnOnFullTile;

    private List<VisualEffect> fireEffectList;
    private List<Vector2Int> fireEffectGridPosList;
    private List<int> fireEffectLifeTimeList;


    private Vector3 mousePos;
    private Camera mainCam;

    public GridObjectData selectedGridTileData;



    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;

        fireWallEffectList = new List<GameObject>();
        fireWallEffectGridPosList = new List<Vector2Int>();
        fireWallEffectLifeTimeList = new List<int>();

        fireEffectList = new List<VisualEffect>();
        fireEffectGridPosList = new List<Vector2Int>();
        fireEffectLifeTimeList = new List<int>();


        targetFireWallPos = fireWallSelectionSprite.position;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (GodCore.Instance.IsHades == false)
        {
            return;
        }

        mainCam = Camera.main;

        if(PlacementManager.Instance != null)
        {
            PlacementManager.Instance.OnConfirmEvent.AddListener(() => OnConfirm());
            PlacementManager.Instance.OnCancelEvent.AddListener(() => OnCancel());

            TurnManager.Instance.OnMyTurnStartedEvent.AddListener(() => OnTurnGranted());

            AbilityManager.Instance.SetupUI(uiSprites[0], abilityCooldowns[0], uiSprites[1], abilityCooldowns[1]);

            AbilityManager.Instance.ability1Activate.AddListener(() => UseDefensiveAbility());
            AbilityManager.Instance.ability2Activate.AddListener(() => UseOffensiveAbility());
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
            if (Physics.Raycast(ray, 100, PlacementManager.Instance.ownFieldLayers + PlacementManager.Instance.neutralLayers))
            {
                AbilityManager.Instance.ConfirmUseAbility(true);

                PlaceFireWall_ServerRPC(fireWallSelectionSprite.position);

                usingDefenseAbility = false;
                fireWallSelectionSprite.gameObject.SetActive(false);
                fireWallSelectionSprite.localPosition = Vector3.zero;
            }
        }
        if (usingOffensiveAbility)
        {
            Ray ray = mainCam.ScreenPointToRay(mousePos);
            if (Physics.Raycast(ray, 100, PlacementManager.Instance.fullFieldLayers))
            {
                AbilityManager.Instance.ConfirmUseAbility(false);
                //meteor

                usingOffensiveAbility = false;
                meteorSelectionSprite.gameObject.SetActive(false);
                meteorSelectionSprite.localPosition = Vector3.zero;
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
        fireWallSelectionSprite.gameObject.SetActive(false);
        fireWallSelectionSprite.localPosition = Vector3.zero;

        usingOffensiveAbility = false;
        meteorSelectionSprite.gameObject.SetActive(false);
        meteorSelectionSprite.localPosition = Vector3.zero;
    }


    public void OnTurnGranted()
    {
        CheckForDiscardFireWall_ServerRPC();
        UseMoltenFloor_ServerRPC();
    }


    public bool usingDefenseAbility;
    public void UseDefensiveAbility()
    {
        usingDefenseAbility = true;
        usingOffensiveAbility = false;

        fireWallSelectionSprite.gameObject.SetActive(true);

        meteorSelectionSprite.gameObject.SetActive(false);
        meteorSelectionSprite.localPosition = Vector3.zero;
    }

    public bool usingOffensiveAbility;
    public void UseOffensiveAbility()
    {
        usingOffensiveAbility = true;
        usingDefenseAbility = false;

        meteorSelectionSprite.gameObject.SetActive(true);

        fireWallSelectionSprite.gameObject.SetActive(false);
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

                GridManager.Instance.UpdateGridDataOnFireState(fireEffectGridPosList[i], -1);
                SetFireState_ClientRPC(fireEffectGridPosList[i], -1);

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


                GridManager.Instance.UpdateGridDataOnFireState(gridTilesOwnField[rTile].gridPos, 1);
                SetFireState_ClientRPC(gridTilesOwnField[rTile].gridPos, 1);

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

            SyncSelectionSprite_ServerRPC(0, fireWallSelectionSprite.position);
        }


        if (usingOffensiveAbility)
        {
            Ray ray = mainCam.ScreenPointToRay(mousePos);

            if (Physics.Raycast(ray, out RaycastHit hitInfo, 100, PlacementManager.Instance.fullFieldLayers))
            {
                selectedGridTileData = GridManager.Instance.GridObjectFromWorldPoint(hitInfo.point);

                if (selectedGridTileData.type == (int)TurnManager.Instance.localClientId)
                {
                    meteorSelectionSprite.position = selectedGridTileData.worldPos;
                }
            }

            SyncSelectionSprite_ServerRPC(1, meteorSelectionSprite.position);
        }
    }


    [ServerRpc(RequireOwnership = false)]
    private void SyncSelectionSprite_ServerRPC(int abilityId, Vector3 pos, ServerRpcParams rpcParams = default)
    {
        ulong senderClientId = rpcParams.Receive.SenderClientId;
        SyncSelectionSprite_ClientRPC(senderClientId, abilityId, pos);
    }

    [ClientRpc(RequireOwnership = false)]
    private void SyncSelectionSprite_ClientRPC(ulong clientId, int abilityId, Vector3 pos)
    {
        if (NetworkManager.LocalClientId == clientId)
        {
            return;
        }

        if (abilityId == 0)
        {
            fireWallSelectionSprite.position = pos;
        }
        else
        {
            meteorSelectionSprite.position = pos;
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
                

                GridManager.Instance.UpdateGridDataOnFireState(fireWallEffectGridPosList[i], -1);
                GridManager.Instance.UpdateGridDataOnFireState(fireWallEffectGridPosList[i] + Vector2Int.up, -1);
                GridManager.Instance.UpdateGridDataOnFireState(fireWallEffectGridPosList[i] + Vector2Int.down, -1);

                SetFireState_ClientRPC(fireWallEffectGridPosList[i], -1);
                SetFireState_ClientRPC(fireWallEffectGridPosList[i] + Vector2Int.up, -1);
                SetFireState_ClientRPC(fireWallEffectGridPosList[i] + Vector2Int.down, -1);

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


        SetFireState_ClientRPC(gridPos, 1);
        SetFireState_ClientRPC(gridPos + Vector2Int.up, 1);
        SetFireState_ClientRPC(gridPos + Vector2Int.down, 1);
    }
    #endregion



    [ClientRpc(RequireOwnership = false)]
    private void SetFireState_ClientRPC(Vector2Int gridPos, int addedState)
    {
        Vector2Int[] gridOffsets = new Vector2Int[4]
        {
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(0, -1),
        };

        Vector2Int targetGridPos;

        for (int i = 0; i < gridOffsets.Length; i++)
        {
            targetGridPos = gridPos + gridOffsets[i];

            if (GridManager.Instance.IsInGrid(targetGridPos) == false)
            {
                continue;
            }

            GridTile directionTile = GridManager.Instance.GetGridData(targetGridPos).tile;
            if (directionTile != null)
            {
                directionTile.SetOnFire(addedState);
            }
        }

        GridTile tile = GridManager.Instance.GetGridData(gridPos).tile;
        if (tile != null)
        {
            tile.SetOnFire(addedState * 3);
        }

        GridManager.Instance.UpdateGridDataOnFireState(gridPos, addedState);
    }
}