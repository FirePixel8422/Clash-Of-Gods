using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Zeus : NetworkBehaviour
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



    public bool usingDefenseAbility;
    public void UseDefensiveAbility()
    {
        usingDefenseAbility = true;
        usingOffensiveAbility = false;

        defensiveSelectionSprite.gameObject.SetActive(false);

        offensiveSelectionSprite.gameObject.SetActive(true);
        offensiveSelectionSprite.localPosition = Vector3.zero;
    }

    public bool usingOffensiveAbility;
    public void UseOffensiveAbility()
    {
        usingOffensiveAbility = true;
        usingDefenseAbility = false;

        offensiveSelectionSprite.gameObject.SetActive(false);

        defensiveSelectionSprite.gameObject.SetActive(true);
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
