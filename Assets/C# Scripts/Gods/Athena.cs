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


    public float ringAnimationMoveSpeed;

    public Transform offensiveSelectionSprite;
    public SpriteRenderer enhanceSpriteRenderer;

    public Color troopSelectedColor;
    public Color noTroopSelectedColor;

    public GameObject enhanceParticleEffect;
    public float enhanceTroopDelay;



    private Vector3 mousePos;
    private Camera mainCam;

    public GridObjectData selectedGridTileData;

    public static GraphicRaycaster gfxRayCaster;




    public void Init()
    {
        gfxRayCaster = FindObjectOfType<GraphicRaycaster>(true);

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

            TurnManager.Instance.OnMyTurnStartedEvent.AddListener(() => OnTurnGranted());

            AbilityManager.Instance.SetupUI(uiSprites[0], abilityCooldowns[0], abilityCharges[0], uiSprites[1], abilityCooldowns[1], abilityCharges[1]);

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
            Troop troop = selectedGridTileData.tower.GetComponent<Troop>();
            if (troop == null)
            {
                return;
            }

            Ray ray = mainCam.ScreenPointToRay(mousePos);
            if (Physics.Raycast(ray, 100, PlacementManager.Instance.ownFieldLayers + PlacementManager.Instance.neutralLayers))
            {
                AbilityManager.Instance.ConfirmUseAbility(false);

                SyncSelectionSpriteState_ServerRPC(false);

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

    }


    public void UseDefensiveAbility()
    {
        usingOffensiveAbility = false;

        offensiveSelectionSprite.gameObject.SetActive(false);
        offensiveSelectionSprite.localPosition = Vector3.zero;
    }

    public bool usingOffensiveAbility;
    public void UseOffensiveAbility()
    {
        usingOffensiveAbility = true;

        offensiveSelectionSprite.gameObject.SetActive(true);
        offensiveSelectionSprite.localPosition = Vector3.zero;

        SyncSelectionSpriteState_ServerRPC(false);
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

                    Troop troop = selectedGridTileData.tower.GetComponent<Troop>();
                    if (troop != null)
                    {
                        enhanceSpriteRenderer.color = troopSelectedColor;
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
        GameObject enhanceEffect = Instantiate(enhanceParticleEffect, pos, Quaternion.identity);
        enhanceEffect.GetComponent<NetworkObject>().Spawn();
    }

    private IEnumerator EnhanceTroopDelay(Troop troop)
    {
        yield return new WaitForSeconds(enhanceTroopDelay);

        troop.EnhanceTroop_ServerRPC();
    }
}
