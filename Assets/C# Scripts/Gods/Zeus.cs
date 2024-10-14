using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.VFX;


public class Zeus : NetworkBehaviour
{
    public Sprite[] uiSprites;
    public int[] abilityCooldowns;

    public GameObject[] lightningLineEffectPrefabs;
    public int lightningLineDamage;
    public float lightningLineDmgDelay;

    public GameObject[] lightningEffectPrefabs;
    public GameObject lightningBallPrefab;
    public int lightningDamage;
    public float lightningDmgDelay;

    public float llAnimationMoveSpeed;
    public float lbAnimationMoveSpeed;

    public Transform lightningLineSelectionSprite;

    public Transform lightningBoltSelectionSprite;

    public float destroyDelay;

    private Vector3 mousePos;
    private Camera mainCam;

    public GridObjectData selectedGridTileData;

    public static GraphicRaycaster gfxRayCaster;



    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;

        gfxRayCaster = FindObjectOfType<GraphicRaycaster>(true);

        targetLightningLinePos = lightningLineSelectionSprite.position;
        targetLightningBoltPos = lightningBoltSelectionSprite.position;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (GodCore.Instance.IsZeus == false)
        {
            return;
        }

        mainCam = Camera.main;

        if (PlacementManager.Instance != null)
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

                CallLightningLine_ServerRPC(selectedGridTileData.worldPos);

                usingDefenseAbility = false;
                lightningLineSelectionSprite.gameObject.SetActive(false);
                lightningLineSelectionSprite.localPosition = Vector3.zero;
            }
        }

        if (usingOffensiveAbility)
        {
            Ray ray = mainCam.ScreenPointToRay(mousePos);
            if (Physics.Raycast(ray, 100, PlacementManager.Instance.fullFieldLayers))
            {
                AbilityManager.Instance.ConfirmUseAbility(false);

                CallLightning_ServerRPC(selectedGridTileData.worldPos);

                usingOffensiveAbility = false;
                lightningBoltSelectionSprite.gameObject.SetActive(false);
                lightningBoltSelectionSprite.localPosition = Vector3.zero;
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
        lightningLineSelectionSprite.gameObject.SetActive(false);
        lightningLineSelectionSprite.localPosition = Vector3.zero;

        usingOffensiveAbility = false;
        lightningBoltSelectionSprite.gameObject.SetActive(false);
        lightningBoltSelectionSprite.localPosition = Vector3.zero;
    }


    public void OnTurnGranted()
    {

    }



    public bool usingDefenseAbility;
    public void UseDefensiveAbility()
    {
        usingDefenseAbility = true;
        usingOffensiveAbility = false;

        lightningBoltSelectionSprite.gameObject.SetActive(false);

        lightningLineSelectionSprite.gameObject.SetActive(true);
        lightningLineSelectionSprite.localPosition = Vector3.zero;
    }

    public bool usingOffensiveAbility;
    public void UseOffensiveAbility()
    {
        usingOffensiveAbility = true;
        usingDefenseAbility = false;

        lightningLineSelectionSprite.gameObject.SetActive(false);

        lightningBoltSelectionSprite.gameObject.SetActive(true);
        lightningBoltSelectionSprite.localPosition = Vector3.zero;
    }




    private void Update()
    {
        UpdateSelectionSprite(Input.mousePosition != mousePos);
        mousePos = Input.mousePosition;
    }


    private Vector3 targetLightningLinePos;
    private Vector3 savedLightningLinepos;

    private Vector3 targetLightningBoltPos;
    private Vector3 savedLightningBoltpos;

    private void UpdateSelectionSprite(bool mouseMoved)
    {
        if (usingDefenseAbility && mouseMoved)
        {
            Ray ray = mainCam.ScreenPointToRay(mousePos);

            if (Physics.Raycast(ray, out RaycastHit hitInfo, 100, PlacementManager.Instance.ownFieldLayers + PlacementManager.Instance.neutralLayers))
            {
                selectedGridTileData = GridManager.Instance.GridObjectFromWorldPoint(hitInfo.point);


                if (lightningLineSelectionSprite.localPosition == Vector3.zero)
                {
                    lightningLineSelectionSprite.position = new Vector3(selectedGridTileData.worldPos.x, 0, 0);
                }
                else if (mouseMoved)
                {
                    targetLightningLinePos =  new Vector3(selectedGridTileData.worldPos.x, 0, 0);
                    savedLightningLinepos = lightningLineSelectionSprite.position;
                }
            }
        }

        if (usingDefenseAbility)
        {
            float _llMoveSpeed = llAnimationMoveSpeed * (Vector3.Distance(savedLightningLinepos, targetLightningLinePos) / GridManager.Instance.tileSize);

            if (Vector3.Distance(lightningLineSelectionSprite.position, targetLightningLinePos) > 0.0001f)
            {
                lightningLineSelectionSprite.position = VectorLogic.InstantMoveTowards(lightningLineSelectionSprite.position, targetLightningLinePos, _llMoveSpeed * Time.deltaTime);
            }

            SyncSelectionSprite_ServerRPC(0, lightningLineSelectionSprite.position);
        }




        if (usingOffensiveAbility && mouseMoved)
        {
            Ray ray = mainCam.ScreenPointToRay(mousePos);

            if (Physics.Raycast(ray, out RaycastHit hitInfo, 100, PlacementManager.Instance.fullFieldLayers))
            {
                selectedGridTileData = GridManager.Instance.GridObjectFromWorldPoint(hitInfo.point);


                if (lightningBoltSelectionSprite.localPosition == Vector3.zero)
                {
                    lightningBoltSelectionSprite.position = new Vector3(selectedGridTileData.worldPos.x, 0, selectedGridTileData.worldPos.z);
                }
                else if (mouseMoved)
                {
                    targetLightningBoltPos = new Vector3(selectedGridTileData.worldPos.x, 0, selectedGridTileData.worldPos.z);
                    savedLightningBoltpos = lightningBoltSelectionSprite.position;
                }
            }
        }

        if (usingOffensiveAbility)
        {
            float _lbMoveSpeed = lbAnimationMoveSpeed * (Vector3.Distance(savedLightningBoltpos, targetLightningBoltPos) / GridManager.Instance.tileSize);

            if (Vector3.Distance(lightningBoltSelectionSprite.position, targetLightningBoltPos) > 0.0001f)
            {
                lightningBoltSelectionSprite.position = VectorLogic.InstantMoveTowards(lightningBoltSelectionSprite.position, targetLightningBoltPos, _lbMoveSpeed * Time.deltaTime);
            }

            SyncSelectionSprite_ServerRPC(1, lightningBoltSelectionSprite.position);
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
            lightningLineSelectionSprite.position = pos;
        }
        else
        {
            lightningBoltSelectionSprite.position = pos;
        }
    }




    [ServerRpc(RequireOwnership = false)]
    private void CallLightningLine_ServerRPC(Vector3 pos, ServerRpcParams rpcParams = default)
    {
        ulong senderClientId = rpcParams.Receive.SenderClientId;

        int rPrefab = Random.Range(0, lightningLineEffectPrefabs.Length);

        GameObject effect = Instantiate(lightningLineEffectPrefabs[rPrefab], new Vector3(pos.x, 0, 0), Quaternion.identity);
        NetworkObject effectNetwork = effect.GetComponent<NetworkObject>();
        effectNetwork.Spawn(true);

        CallLightningLine_ClientRPC(senderClientId, pos);

        StartCoroutine(DestroyDelay(effectNetwork));
    }

    [ClientRpc(RequireOwnership = false)]
    private void CallLightningLine_ClientRPC(ulong senderClientId, Vector3 pos)
    {
        StartCoroutine(LightningLineDamageDelay(senderClientId, pos));
    }

    private IEnumerator LightningLineDamageDelay(ulong senderClientId, Vector3 pos)
    {
        yield return new WaitForSeconds(lightningLineDmgDelay);

        int gridPosX = GridManager.Instance.GridObjectFromWorldPoint(pos).gridPos.x;

        Vector2Int[] gridPositons = new Vector2Int[6]
        {
            new Vector2Int(gridPosX, 0),
            new Vector2Int(gridPosX, 1),
            new Vector2Int(gridPosX, 2),
            new Vector2Int(gridPosX, 3),
            new Vector2Int(gridPosX, 4),
            new Vector2Int(gridPosX, 5),
        };

        for (int i = 0; i < 6; i++)
        {
            if (GridManager.Instance.IsInGrid(gridPositons[i]))
            {
                GridObjectData gridData =  GridManager.Instance.GetGridData(gridPositons[i]);

                if (gridData.tower != null && gridData.tower.OwnerClientId != senderClientId)
                {
                    gridData.tower.GetAttacked(lightningLineDamage, GodCore.Instance.RandomStunChance());
                }
            }
        }
    }

    private IEnumerator DestroyDelay(NetworkObject networkObject)
    {
        yield return new WaitForSeconds(destroyDelay);

        networkObject.Despawn(true);
    }




    [ServerRpc(RequireOwnership = false)]
    private void CallLightning_ServerRPC(Vector3 pos, ServerRpcParams rpcParams = default)
    {
        ulong senderClientId = rpcParams.Receive.SenderClientId;

        int rPrefab = Random.Range(0, lightningEffectPrefabs.Length);

        GameObject effect = Instantiate(lightningEffectPrefabs[rPrefab], pos, Quaternion.identity);
        NetworkObject effectNetwork = effect.GetComponent<NetworkObject>();
        effectNetwork.Spawn(true);

        CallLightning_ClientRPC(senderClientId, pos);

        StartCoroutine(DestroyDelay(effectNetwork));
    }


    [ClientRpc(RequireOwnership = false)]
    private void CallLightning_ClientRPC(ulong senderClientId, Vector3 pos)
    {
        StartCoroutine(LightningDamageDelay(senderClientId, pos));
    }

    private IEnumerator LightningDamageDelay(ulong senderClientId, Vector3 pos)
    {
        yield return new WaitForSeconds(lightningDmgDelay);

        GridObjectData gridData = GridManager.Instance.GridObjectFromWorldPoint(pos);

        if (gridData.tower != null && gridData.tower.OwnerClientId != senderClientId)
        {
            gridData.tower.GetAttacked(lightningDamage, GodCore.Instance.RandomStunChance());
        }

        if (IsServer)
        {
            CallLightningBall_ServerRPC(selectedGridTileData.worldPos);
        }
    }


    [ServerRpc(RequireOwnership = false)]
    private void CallLightningBall_ServerRPC(Vector3 pos, ServerRpcParams rpcParams = default)
    {
        ulong senderClientId = rpcParams.Receive.SenderClientId;

        GameObject effect = Instantiate(lightningBallPrefab, pos, Quaternion.identity);
        NetworkObject effectNetwork = effect.GetComponent<NetworkObject>();
        effectNetwork.SpawnWithOwnership(senderClientId, true);

        effectNetwork.GetComponent<ChainLightning>().Init();

        StartCoroutine(DestroyDelay(effectNetwork));
    }
}
