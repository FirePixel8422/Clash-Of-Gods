using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.VFX;


public class Hades : NetworkBehaviour
{
    public static Hades Instance;
    private void Awake()
    {
        Instance = this;
    }

    public Sprite[] uiSprites;
    public int[] abilityCooldowns;
    public int[] abilityCharges;
    public string[] abilityInfo;


    public Transform fireWallSelectionSprite;

    public GameObject[] fireWallEffectPrefabs;

    public float fwAnimationMoveSpeed;
    public int fireWallLifeTime;

    private List<GameObject> fireWallEffectList;
    private List<Vector2Int> fireWallEffectGridPosList;
    private List<int> fireWallEffectLifeTimeList;



    public Transform meteorSelectionSprite;

    public GameObject[] meteorEffectPrefabs;

    public float meteorImpactDelay;
    public float meteorDamageDelay;

    public int meteorFireLifeTime;

    public int meteorImpactDamageMain;
    public int meteorImpactDamageClose;

    public float meteorAnimationMoveSpeed;



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

    public static GraphicRaycaster gfxRayCaster;





    public void Init()
    {
        gfxRayCaster = FindObjectOfType<GraphicRaycaster>(true);

        fireWallEffectList = new List<GameObject>();
        fireWallEffectGridPosList = new List<Vector2Int>();
        fireWallEffectLifeTimeList = new List<int>();

        fireEffectList = new List<VisualEffect>();
        fireEffectGridPosList = new List<Vector2Int>();
        fireEffectLifeTimeList = new List<int>();


        targetFireWallPos = fireWallSelectionSprite.position;
        targetMeteorPos = meteorSelectionSprite.position;

        if (GodCore.Instance.IsHades == false)
        {
            return;
        }



        mainCam = Camera.main;

        if (PlacementManager.Instance != null)
        {
            PlacementManager.Instance.OnConfirmEvent.AddListener(() => OnConfirm());
            PlacementManager.Instance.OnCancelEvent.AddListener(() => OnCancel());
            PlacementManager.Instance.OnSelectEvent.AddListener(() => OnCancel());

            TurnManager.Instance.OnMyTurnStartedEvent.AddListener(() => OnTurnGranted());

            AbilityManager.Instance.SetupUI(uiSprites[0], abilityCooldowns[0], abilityCharges[0], uiSprites[1], abilityCooldowns[1], abilityCharges[1], uiSprites[2]);

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

        PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
        pointerEventData.position = Input.mousePosition;

        var results = new List<RaycastResult>();
        gfxRayCaster.Raycast(pointerEventData, results);

        if (results.Count > 0)
        {
            return;
        }


        if (usingDefenseAbility)
        {
            Ray ray = mainCam.ScreenPointToRay(mousePos);
            if (Physics.Raycast(ray, 100, PlacementManager.Instance.ownFieldLayers + PlacementManager.Instance.neutralLayers))
            {
                AbilityManager.Instance.ConfirmUseAbility(true);
                SyncSelectionSpriteState_ServerRPC(0, false);

                PlaceFireWall_ServerRPC(selectedGridTileData.worldPos);

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
                SyncSelectionSpriteState_ServerRPC(1, false);

                CallMeteor_ServerRPC(selectedGridTileData.worldPos);

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

        SyncSelectionSpriteState_ServerRPC(0, false);

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
        PlacementManager.Instance.Cancel();

        usingDefenseAbility = true;
        usingOffensiveAbility = false;

        fireWallSelectionSprite.gameObject.SetActive(true);

        SyncSelectionSpriteState_ServerRPC(0, true);

        meteorSelectionSprite.gameObject.SetActive(false);
        meteorSelectionSprite.localPosition = Vector3.zero;
        targetMeteorPos = meteorSelectionSprite.localPosition;
    }

    public bool usingOffensiveAbility;
    public void UseOffensiveAbility()
    {
        PlacementManager.Instance.Cancel();

        usingOffensiveAbility = true;
        usingDefenseAbility = false;

        meteorSelectionSprite.gameObject.SetActive(true);

        SyncSelectionSpriteState_ServerRPC(1, true);

        fireWallSelectionSprite.gameObject.SetActive(false);
        fireWallSelectionSprite.localPosition = Vector3.zero;
        targetFireWallPos = meteorSelectionSprite.localPosition;
    }


    [ServerRpc(RequireOwnership = false)]
    private void SyncSelectionSpriteState_ServerRPC(int abilityId, bool newState, ServerRpcParams rpcParams = default)
    {
        ulong senderClientId = rpcParams.Receive.SenderClientId;

        SyncSelectionState_ClientRPC(senderClientId, abilityId, newState);
    }

    [ClientRpc(RequireOwnership = false)]
    private void SyncSelectionState_ClientRPC(ulong clientId, int abilityId, bool newState)
    {
        if (NetworkManager.LocalClientId == clientId)
        {
            return;
        }

        if (abilityId == 0)
        {
            fireWallSelectionSprite.gameObject.SetActive(newState);
            meteorSelectionSprite.gameObject.SetActive(false);
        }
        else
        {
            meteorSelectionSprite.gameObject.SetActive(newState);
            fireWallSelectionSprite.gameObject.SetActive(false);
        }
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

                int rPrefab = Random.Range(0, fireEffectPrefabs.Length);

                Vector3 pos = gridTilesOwnField[rTile].worldPos;
                pos.y += fireEffectPrefabs[rPrefab].transform.position.y;

                VisualEffect effect = Instantiate(fireEffectPrefabs[rPrefab], pos, Quaternion.Euler(0, Random.Range(180, -180), 0));
                effect.GetComponent<NetworkObject>().Spawn(true);


                fireEffectList.Add(effect);
                fireEffectGridPosList.Add(gridTilesOwnField[rTile].gridPos);
                fireEffectLifeTimeList.Add(fireLifeTime);


                SetFireState_ClientRPC(gridTilesOwnField[rTile].gridPos, 1);

                break;
            }
        }
    }

    #endregion




    #region Update Selection Sprite

    private void Update()
    {
        UpdateSelectionSprite(Input.mousePosition != mousePos);
        mousePos = Input.mousePosition;
    }


    private Vector3 targetFireWallPos;
    private Vector3 savedFireWallpos;

    private Vector3 targetMeteorPos;
    private Vector3 savedMeteorpos;

    private void UpdateSelectionSprite(bool mouseMoved)
    {
        if (usingDefenseAbility && mouseMoved)
        {
            Ray ray = mainCam.ScreenPointToRay(mousePos);

            if (Physics.Raycast(ray, out RaycastHit hitInfo, 100, PlacementManager.Instance.ownFieldLayers + PlacementManager.Instance.neutralLayers))
            {
                GridObjectData newSelectedGridTileData = GridManager.Instance.GridObjectFromWorldPoint(hitInfo.point);

                float posZOffset = 0;

                if (newSelectedGridTileData.gridPos.y == (GridManager.Instance.gridSizeZ - 1))
                {
                    posZOffset = -GridManager.Instance.tileSize;
                }
                if (newSelectedGridTileData.gridPos.y == 0)
                {
                    posZOffset = GridManager.Instance.tileSize;
                }

                //only if selected a proper Tile
                if (posZOffset == 0)
                {
                    selectedGridTileData = newSelectedGridTileData;
                }


                if (fireWallSelectionSprite.localPosition == Vector3.zero)
                {
                    fireWallSelectionSprite.position = newSelectedGridTileData.worldPos + new Vector3(0, 0, posZOffset);
                }
                else if (mouseMoved)
                {
                    targetFireWallPos = newSelectedGridTileData.worldPos + new Vector3(0, 0, posZOffset);
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




        if (usingOffensiveAbility && mouseMoved)
        {
            Ray ray = mainCam.ScreenPointToRay(mousePos);

            if (Physics.Raycast(ray, out RaycastHit hitInfo, 100, PlacementManager.Instance.fullFieldLayers))
            {
                selectedGridTileData = GridManager.Instance.GridObjectFromWorldPoint(hitInfo.point);


                if (meteorSelectionSprite.localPosition == Vector3.zero)
                {
                    meteorSelectionSprite.position = selectedGridTileData.worldPos;
                }
                else if (mouseMoved)
                {
                    targetMeteorPos = selectedGridTileData.worldPos;
                    savedMeteorpos = meteorSelectionSprite.position;
                }
            }
        }


        if (usingOffensiveAbility)
        {
            float _meteorMoveSpeed = fwAnimationMoveSpeed * (Vector3.Distance(savedMeteorpos, targetMeteorPos) / GridManager.Instance.tileSize);

            if (Vector3.Distance(meteorSelectionSprite.position, targetMeteorPos) > 0.0001f)
            {
                meteorSelectionSprite.position = VectorLogic.InstantMoveTowards(meteorSelectionSprite.position, targetMeteorPos, _meteorMoveSpeed * Time.deltaTime);
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

    #endregion




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


    private IEnumerator DestroyDelay(NetworkObject networkObject)
    {
        StopFire_ClientRPC(networkObject.NetworkObjectId);

        yield return new WaitForSeconds(destroyDelay);

        networkObject.Despawn(true);
    }

    [ClientRpc(RequireOwnership = false)]
    public void StopFire_ClientRPC(ulong networkObjectId)
    {
        NetworkObject fireGameObject = NetworkManager.SpawnManager.SpawnedObjects[networkObjectId];

        foreach(VisualEffect fireEffect in fireGameObject.GetComponentsInChildren<VisualEffect>())
        {
            fireEffect.Stop();
        }
    }


    #region Call Down Meteor On Network

    [ServerRpc(RequireOwnership = false)]
    private void CallMeteor_ServerRPC(Vector3 pos, ServerRpcParams rpcParams = default)
    {
        ulong senderClientId = rpcParams.Receive.SenderClientId;

        int rPrefab = Random.Range(0, meteorEffectPrefabs.Length);

        GameObject effect = Instantiate(meteorEffectPrefabs[rPrefab], pos + meteorEffectPrefabs[rPrefab].transform.position, Quaternion.identity);
        NetworkObject effectNetwork = effect.GetComponent<NetworkObject>();
        effectNetwork.Spawn(true);

        StartCoroutine(MeteorDamageDelay(senderClientId, pos));
    }

    private IEnumerator MeteorDamageDelay(ulong senderClientId, Vector3 pos)
    {
        yield return new WaitForSeconds(meteorImpactDelay);

        Vector2Int gridPos = GridManager.Instance.GridObjectFromWorldPoint(pos).gridPos;

        Vector2Int[] gridPositonOffsets = new Vector2Int[8]
        {
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(0, -1),
            new Vector2Int(1, 1),
            new Vector2Int(1, -1),
            new Vector2Int(-1, 1),
            new Vector2Int(-1, -1),
        };




        GridObjectData gridData = GridManager.Instance.GetGridData(gridPos);

        Vector3 spawnFirePos = gridData.worldPos;

        int rPrefab = Random.Range(0, fireEffectPrefabs.Length);

        pos.y += fireEffectPrefabs[rPrefab].transform.position.y;

        VisualEffect effect = Instantiate(fireEffectPrefabs[rPrefab], spawnFirePos, Quaternion.Euler(0, Random.Range(180, -180), 0));
        effect.GetComponent<NetworkObject>().Spawn(true);

        SetFireState_ClientRPC(gridPos, 1);

        fireEffectList.Add(effect);
        fireEffectGridPosList.Add(gridData.gridPos);
        fireEffectLifeTimeList.Add(meteorFireLifeTime);

        for (int i = 0; i < gridPositonOffsets.Length; i++)
        {
            Vector2Int targetGridPos = gridPos + gridPositonOffsets[i];

            if (GridManager.Instance.IsInGrid(targetGridPos) && targetGridPos.x != 0 && targetGridPos.x != (GridManager.Instance.gridSizeX -1))
            {
                gridData = GridManager.Instance.GetGridData(targetGridPos);

                spawnFirePos = gridData.worldPos;

                rPrefab = Random.Range(0, fireEffectPrefabs.Length);

                pos.y += fireEffectPrefabs[rPrefab].transform.position.y;

                effect = Instantiate(fireEffectPrefabs[rPrefab], spawnFirePos, Quaternion.Euler(0, Random.Range(180, -180), 0));
                effect.GetComponent<NetworkObject>().Spawn(true);

                SetFireState_ClientRPC(targetGridPos, 1);

                fireEffectList.Add(effect);
                fireEffectGridPosList.Add(gridData.gridPos);
                fireEffectLifeTimeList.Add(meteorFireLifeTime);
            }
        }




        yield return new WaitForSeconds(meteorDamageDelay);

        gridData = GridManager.Instance.GetGridData(gridPos);

        if (gridData.tower != null && gridData.tower.OwnerClientId != senderClientId)
        {
            gridData.tower.GetAttacked(meteorImpactDamageMain, false);
        }

        for (int i = 0; i < 8; i++)
        {
            if (GridManager.Instance.IsInGrid(gridPos + gridPositonOffsets[i]))
            {
                gridData = GridManager.Instance.GetGridData(gridPos + gridPositonOffsets[i]);

                if (gridData.tower != null && gridData.tower.GetComponent<PlayerBase>() == null && gridData.tower.OwnerClientId != senderClientId)
                {
                    gridData.tower.GetAttacked(meteorImpactDamageClose, false);
                }
            }
        }
    }

    #endregion
}