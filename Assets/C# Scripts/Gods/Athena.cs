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
            AbilityManager.Instance.ConfirmUseAbility(true);

            Ray ray = mainCam.ScreenPointToRay(mousePos);
            if (Physics.Raycast(ray, 100, PlacementManager.Instance.ownFieldLayers + PlacementManager.Instance.neutralLayers))
            {
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

        EnableSelectionSprite_ServerRPC(0);

        offensiveSelectionSprite.gameObject.SetActive(false);
        offensiveSelectionSprite.localPosition = Vector3.zero;
    }

    public bool usingOffensiveAbility;
    public void UseOffensiveAbility()
    {
        usingOffensiveAbility = true;
        usingDefenseAbility = false;

        offensiveSelectionSprite.gameObject.SetActive(true);

        EnableSelectionSprite_ServerRPC(1);

        defensiveSelectionSprite.gameObject.SetActive(false);
        defensiveSelectionSprite.localPosition = Vector3.zero;
    }


    [ServerRpc(RequireOwnership = false)]
    private void EnableSelectionSprite_ServerRPC(int abilityId, ServerRpcParams rpcParams = default)
    {
        ulong senderClientId = rpcParams.Receive.SenderClientId;

        EnableSelectionSprite_ClientRPC(senderClientId, abilityId);
    }

    [ClientRpc(RequireOwnership = false)]
    private void EnableSelectionSprite_ClientRPC(ulong clientId, int abilityId)
    {
        if (NetworkManager.LocalClientId == clientId)
        {
            return;
        }

        if (abilityId == 0)
        {
            defensiveSelectionSprite.gameObject.SetActive(true);
            offensiveSelectionSprite.gameObject.SetActive(false);
        }
        else
        {
            offensiveSelectionSprite.gameObject.SetActive(true);
            defensiveSelectionSprite.gameObject.SetActive(false);
        }
    }
}
