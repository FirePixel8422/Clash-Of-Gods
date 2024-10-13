using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Athena : NetworkBehaviour
{
    public Sprite[] uiSprites;
    public int[] abilityCooldowns;

    public Transform defensiveSelectionSprite;

    public Transform offensiveSelectionSprite;



    private Vector3 mousePos;
    private Camera mainCam;

    public GridObjectData selectedGridTileData;




    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (GodCore.Instance.IsAthena == false)
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

        offensiveSelectionSprite.gameObject.SetActive(false);
        offensiveSelectionSprite.localPosition = Vector3.zero;
    }

    public bool usingOffensiveAbility;
    public void UseOffensiveAbility()
    {
        usingOffensiveAbility = true;
        usingDefenseAbility = false;

        offensiveSelectionSprite.gameObject.SetActive(true);

        defensiveSelectionSprite.gameObject.SetActive(false);
        defensiveSelectionSprite.localPosition = Vector3.zero;
    }
}
