using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Zeus : GodCore
{
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
        mainCam = Camera.main;
    }



    public void OnCancel(InputAction.CallbackContext ctx)
    {
        if (TurnManager.Instance.isMyTurn == false)
        {
            return;
        }

        if (ctx.performed)
        {
            usingDefenseAbility = false;
            defensiveSelectionSprite.localPosition = Vector3.zero;
            usingOffensiveAbility = false;
            offensiveSelectionSprite.localPosition = Vector3.zero;
        }
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

        defensiveSelectionSprite.localPosition = Vector3.zero;
    }




    private void Update()
    {
        if (Input.mousePosition != mousePos)
        {
            mousePos = Input.mousePosition;
            UpdateSelectionSprite();
        }
    }

    private void UpdateSelectionSprite()
    {
        if (usingDefenseAbility)
        {
            Ray ray = mainCam.ScreenPointToRay(mousePos);

            if (Physics.Raycast(ray, out RaycastHit hitInfo, 100, PlacementManager.Instance.ownFieldLayers))
            {
                selectedGridTileData = GridManager.Instance.GridObjectFromWorldPoint(hitInfo.point);

                defensiveSelectionSprite.position = new Vector3(selectedGridTileData.worldPos.x, 0, 0);
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
                    defensiveSelectionSprite.position = selectedGridTileData.worldPos;
                }
            }
        }
    }
}
