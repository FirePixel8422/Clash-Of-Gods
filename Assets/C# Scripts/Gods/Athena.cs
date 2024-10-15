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

    public Transform defensiveSelectionSprite;

    public float fwAnimationMoveSpeed;

    public Transform offensiveSelectionSprite;



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
                SyncSelectionSpriteState_ServerRPC(0, false);

                usingDefenseAbility = false;
                defensiveSelectionSprite.gameObject.SetActive(false);
                defensiveSelectionSprite.localPosition = Vector3.zero;
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
        defensiveSelectionSprite.gameObject.SetActive(false);
        defensiveSelectionSprite.localPosition = Vector3.zero;

        SyncSelectionSpriteState_ServerRPC(0, false);

        usingOffensiveAbility = false;
        offensiveSelectionSprite.gameObject.SetActive(false);
        offensiveSelectionSprite.localPosition = Vector3.zero;
    }


    public void OnTurnGranted()
    {

    }


    public bool usingDefenseAbility;
    public void UseDefensiveAbility()
    {
        usingDefenseAbility = true;
        usingOffensiveAbility = false;

        defensiveSelectionSprite.gameObject.SetActive(true);

        SyncSelectionSpriteState_ServerRPC(0, true);

        offensiveSelectionSprite.gameObject.SetActive(false);
        offensiveSelectionSprite.localPosition = Vector3.zero;
    }

    public bool usingOffensiveAbility;
    public void UseOffensiveAbility()
    {
        usingOffensiveAbility = true;
        usingDefenseAbility = false;

        offensiveSelectionSprite.gameObject.SetActive(true);

        SyncSelectionSpriteState_ServerRPC(1, true);

        defensiveSelectionSprite.gameObject.SetActive(false);
        defensiveSelectionSprite.localPosition = Vector3.zero;
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
            defensiveSelectionSprite.gameObject.SetActive(newState);
            offensiveSelectionSprite.gameObject.SetActive(false);
        }
        else
        {
            offensiveSelectionSprite.gameObject.SetActive(newState);
            defensiveSelectionSprite.gameObject.SetActive(false);
        }
    }




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


                if (defensiveSelectionSprite.localPosition == Vector3.zero)
                {
                    defensiveSelectionSprite.position = selectedGridTileData.worldPos + new Vector3(0, 0, posZOffset);
                }
                else if (mouseMoved)
                {
                    targetFireWallPos = selectedGridTileData.worldPos + new Vector3(0, 0, posZOffset);
                    savedFireWallpos = defensiveSelectionSprite.position;
                }
            }
        }


        if (usingDefenseAbility)
        {
            float _fireWallMoveSpeed = fwAnimationMoveSpeed * (Vector3.Distance(savedFireWallpos, targetFireWallPos) / GridManager.Instance.tileSize);

            if (Vector3.Distance(defensiveSelectionSprite.position, targetFireWallPos) > 0.0001f)
            {
                defensiveSelectionSprite.position = VectorLogic.InstantMoveTowards(defensiveSelectionSprite.position, targetFireWallPos, _fireWallMoveSpeed * Time.deltaTime);
            }

            SyncSelectionSprite_ServerRPC(0, defensiveSelectionSprite.position);
        }




        if (usingOffensiveAbility && mouseMoved)
        {
            Ray ray = mainCam.ScreenPointToRay(mousePos);

            if (Physics.Raycast(ray, out RaycastHit hitInfo, 100, PlacementManager.Instance.fullFieldLayers))
            {
                selectedGridTileData = GridManager.Instance.GridObjectFromWorldPoint(hitInfo.point);


                if (offensiveSelectionSprite.localPosition == Vector3.zero)
                {
                    offensiveSelectionSprite.position = selectedGridTileData.worldPos;
                }
                else if (mouseMoved)
                {
                    targetMeteorPos = selectedGridTileData.worldPos;
                    savedMeteorpos = offensiveSelectionSprite.position;
                }
            }
        }


        if (usingOffensiveAbility)
        {
            float _meteorMoveSpeed = fwAnimationMoveSpeed * (Vector3.Distance(savedMeteorpos, targetMeteorPos) / GridManager.Instance.tileSize);

            if (Vector3.Distance(offensiveSelectionSprite.position, targetMeteorPos) > 0.0001f)
            {
                offensiveSelectionSprite.position = VectorLogic.InstantMoveTowards(offensiveSelectionSprite.position, targetMeteorPos, _meteorMoveSpeed * Time.deltaTime);
            }

            SyncSelectionSprite_ServerRPC(1, offensiveSelectionSprite.position);
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
            defensiveSelectionSprite.position = pos;
        }
        else
        {
            offensiveSelectionSprite.position = pos;
        }
    }


    public void Reinforce()
    {

    }
}
