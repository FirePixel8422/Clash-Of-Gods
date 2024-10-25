using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Athena : NetworkBehaviour
{
    public static Athena Instance;
    private void Awake()
    {
        Instance = this;
    }


    public Sprite[] uiSprites;
    public int[] abilityCooldowns;
    public int[] abilityCharges;
    public string[] abilityInfo;
    public AudioClip[] abilitySounds;


    public float ringAnimationMoveSpeed;

    public Transform offensiveSelectionSprite;
    public SpriteRenderer enhanceSpriteRenderer;

    public Color troopSelectedColor;
    public Color noTroopSelectedColor;

    public GameObject enhanceParticleEffect;
    public float enhanceTroopDelay;


    public GameObject gladiatorPrefab;
    public int spawnAmount;
    public int lifeTimeTurns;



    private Vector3 mousePos;
    private Camera mainCam;

    public GridObjectData selectedGridTileData;

    public static GraphicRaycaster gfxRayCaster;

    public List<TowerCore> gladiators;
    public List<int> gladiatorsLifeLeft;




    public void Init()
    {
        gfxRayCaster = FindObjectOfType<GraphicRaycaster>(true);

        TurnManager.Instance.OnMyTurnStartedEvent.AddListener(() => OnTurnGranted());

        if (GodCore.Instance.IsAthena == false)
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


        mainCam = Camera.main;

        if (PlacementManager.Instance != null)
        {
            PlacementManager.Instance.OnConfirmEvent.AddListener(() => OnConfirm());
            PlacementManager.Instance.OnCancelEvent.AddListener(() => OnCancel());
            PlacementManager.Instance.OnSelectEvent.AddListener(() => OnCancel());

            AbilityManager.Instance.SetupUI(uiSprites[0], abilityCooldowns[0], abilityCharges[0], uiSprites[1], abilityCooldowns[1], abilityCharges[1], uiSprites[2], abilityInfo, abilitySounds);

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

        if (usingOffensiveAbility)
        {
            Ray ray = mainCam.ScreenPointToRay(mousePos);
            if (Physics.Raycast(ray, out RaycastHit hitInfo, 100, PlacementManager.Instance.fullFieldLayers))
            {
                GridObjectData selectedTile = GridManager.Instance.GridObjectFromWorldPoint(hitInfo.point);

                Troop troop;
                if (selectedTile.tower != null)
                {
                    troop = selectedTile.tower.GetComponent<Troop>();

                    if (troop == null || troop.OwnerClientId != NetworkManager.LocalClientId || troop.isBuffed)
                    {
                        return;
                    }
                }
                else
                {
                    return;
                }

                AbilityManager.Instance.ConfirmUseAbility(false);

                SyncSelectionSpriteState_ServerRPC(false);

                troop.isBuffed = true;
                EnhanceTroop(selectedGridTileData.worldPos, troop);

                usingOffensiveAbility = false;
                offensiveSelectionSprite.gameObject.SetActive(false);
                offensiveSelectionSprite.localPosition = Vector3.zero;
            }
        }
    }

    public void OnCancel()
    {
        if (TurnManager.Instance.isMyTurn == false)
        {
            return;
        }

        SyncSelectionSpriteState_ServerRPC(false);

        usingOffensiveAbility = false;
        offensiveSelectionSprite.gameObject.SetActive(false);
        offensiveSelectionSprite.localPosition = Vector3.zero;
    }



    public void OnTurnGranted()
    {
        for (int i = 0; i < gladiators.Count; i++)
        {
            gladiatorsLifeLeft[i] -= 1;

            if (gladiatorsLifeLeft[i] == 0)
            {
                gladiators[i].GetAttacked(100000000, false);

                gladiators.RemoveAt(i);
                gladiatorsLifeLeft.RemoveAt(i);

                i -= 1;
            }
        }
    }



    public void UseDefensiveAbility()
    {
        PlacementManager.Instance.Cancel();

        usingOffensiveAbility = false;

        offensiveSelectionSprite.gameObject.SetActive(false);

        AbilityManager.Instance.ConfirmUseAbility(true);

        Reinforce();
    }

    public bool usingOffensiveAbility;
    public void UseOffensiveAbility()
    {
        PlacementManager.Instance.Cancel();

        usingOffensiveAbility = true;

        offensiveSelectionSprite.gameObject.SetActive(true);
        offensiveSelectionSprite.localPosition = Vector3.zero;
        targetRingPos = offensiveSelectionSprite.localPosition;

        SyncSelectionSpriteState_ServerRPC(true);
    }




    [ServerRpc(RequireOwnership = false)]
    private void SyncSelectionSpriteState_ServerRPC(bool newState, ServerRpcParams rpcParams = default)
    {
        ulong senderClientId = rpcParams.Receive.SenderClientId;

        SyncSelectionState_ClientRPC(senderClientId, newState);
    }

    [ClientRpc(RequireOwnership = false)]
    private void SyncSelectionState_ClientRPC(ulong clientId, bool newState)
    {
        if (NetworkManager.LocalClientId == clientId)
        {
            return;
        }

        offensiveSelectionSprite.gameObject.SetActive(newState);
    }




    private void Update()
    {
        UpdateSelectionSprite(Input.mousePosition != mousePos);
        mousePos = Input.mousePosition;
    }

    private Vector3 targetRingPos;
    private Vector3 savedRingpos;

    private void UpdateSelectionSprite(bool mouseMoved)
    {
        if (usingOffensiveAbility && mouseMoved)
        {
            Ray ray = mainCam.ScreenPointToRay(mousePos);

            if (Physics.Raycast(ray, out RaycastHit hitInfo, 100, PlacementManager.Instance.fullFieldLayers))
            {
                GridObjectData newGridData = GridManager.Instance.GridObjectFromWorldPoint(hitInfo.point);

                if(selectedGridTileData.gridPos != newGridData.gridPos)
                {
                    selectedGridTileData = newGridData;

                    if (selectedGridTileData.tower != null)
                    {
                        Troop troop = selectedGridTileData.tower.GetComponent<Troop>();

                        if (troop != null && troop.OwnerClientId == NetworkManager.LocalClientId && troop.isBuffed == false)
                        {
                            enhanceSpriteRenderer.color = troopSelectedColor;
                        }
                        else
                        {
                            enhanceSpriteRenderer.color = noTroopSelectedColor;
                        }
                    }
                    else
                    {
                        enhanceSpriteRenderer.color = noTroopSelectedColor;
                    }
                }

                if (offensiveSelectionSprite.localPosition == Vector3.zero)
                {
                    offensiveSelectionSprite.position = selectedGridTileData.worldPos;
                }
                else if (mouseMoved)
                {
                    targetRingPos = selectedGridTileData.worldPos;
                    savedRingpos = offensiveSelectionSprite.position;
                }
            }
        }


        if (usingOffensiveAbility)
        {
            float _meteorMoveSpeed = ringAnimationMoveSpeed * (Vector3.Distance(savedRingpos, targetRingPos) / GridManager.Instance.tileSize);

            if (Vector3.Distance(offensiveSelectionSprite.position, targetRingPos) > 0.0001f)
            {
                offensiveSelectionSprite.position = VectorLogic.InstantMoveTowards(offensiveSelectionSprite.position, targetRingPos, _meteorMoveSpeed * Time.deltaTime);
            }

            SyncSelectionSprite_ServerRPC(offensiveSelectionSprite.position);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SyncSelectionSprite_ServerRPC(Vector3 pos, ServerRpcParams rpcParams = default)
    {
        ulong senderClientId = rpcParams.Receive.SenderClientId;
        SyncSelectionSprite_ClientRPC(senderClientId, pos);
    }

    [ClientRpc(RequireOwnership = false)]
    private void SyncSelectionSprite_ClientRPC(ulong clientId, Vector3 pos)
    {
        if (NetworkManager.LocalClientId == clientId)
        {
            return;
        }

        offensiveSelectionSprite.position = pos;
    }



    public void EnhanceTroop(Vector3 pos, Troop troop)
    {
        EnhanceParticleEffect_ServerRPC(pos);

        StartCoroutine(EnhanceTroopDelay(troop));
    }

    [ServerRpc(RequireOwnership = false)]
    private void EnhanceParticleEffect_ServerRPC(Vector3 pos)
    {
        GameObject enhanceEffect = Instantiate(enhanceParticleEffect, pos + new Vector3(0, 0.025f, 0), Quaternion.Euler(90, 0, 0));
        enhanceEffect.GetComponent<NetworkObject>().Spawn();
    }

    private IEnumerator EnhanceTroopDelay(Troop troop)
    {
        yield return new WaitForSeconds(enhanceTroopDelay);

        troop.EnhanceTroop_ServerRPC();
    }


    private void Reinforce()
    {
        List<GridObjectData> possibleSpawnTiles = new List<GridObjectData>();

        for (int x = 0; x < GridManager.Instance.gridSizeZ; x++)
        {
            for (int z = 0; z < GridManager.Instance.gridSizeZ; z++)
            {
                GridObjectData gridData = GridManager.Instance.GetGridData(new Vector2Int(x + (int)NetworkManager.LocalClientId * 7 + 1, z));

                if (gridData.full == false)
                {
                    possibleSpawnTiles.Add(gridData);
                }
            }
        }

        for (int i = 0; i < Mathf.Min(spawnAmount, possibleSpawnTiles.Count); i++)
        {
            int r = Random.Range(0, possibleSpawnTiles.Count);


            GridManager.Instance.UpdateGridDataFullState(possibleSpawnTiles[r].gridPos, true);
            selectedGridTileData.full = true;

            PlaceGladiator_ServerRPC(possibleSpawnTiles[r].worldPos, NetworkManager.LocalClientId == 0 ? 90 : -90, possibleSpawnTiles[r].gridPos);

            possibleSpawnTiles.RemoveAt(r);
        }
    }


    [ServerRpc(RequireOwnership = false)]
    private void PlaceGladiator_ServerRPC(Vector3 pos, int rotY, Vector2Int gridPos, ServerRpcParams rpcParams = default)
    {
        TowerCore spawnedTower = Instantiate(gladiatorPrefab, pos, Quaternion.Euler(0, rotY, 0)).GetComponent<TowerCore>();

        ulong fromClientId = rpcParams.Receive.SenderClientId;
        spawnedTower.NetworkObject.SpawnWithOwnership(fromClientId, true);

        gladiators.Add(spawnedTower);
        gladiatorsLifeLeft.Add(lifeTimeTurns);

        PlaceGladiator_ClientRPC(fromClientId, gridPos, spawnedTower.NetworkObjectId);
    }

    [ClientRpc(RequireOwnership = false)]
    private void PlaceGladiator_ClientRPC(ulong fromClientId, Vector2Int gridPos, ulong spawnedTowerNetworkObjectId)
    {
        TowerCore gladiator = NetworkManager.SpawnManager.SpawnedObjects[spawnedTowerNetworkObjectId].GetComponent<Troop>();
        gladiator.CoreInit();


        Renderer[] renderers = gladiator.transform.GetComponentsInChildren<Renderer>(true);
        foreach (Renderer renderer in renderers)
        {
            if (renderer.gameObject.CompareTag("TeamColor"))
            {
                renderer.material.SetColor(Shader.PropertyToID("_Base_Color"), PlacementManager.Instance.playerColors[GodCore.Instance.chosenGods[fromClientId]]);
            }
        }

        GridManager.Instance.UpdateTowerData(gridPos, gladiator);
    }
}
