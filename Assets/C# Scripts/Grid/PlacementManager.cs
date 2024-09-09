using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Resources;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlacementManager : NetworkBehaviour
{
    public static PlacementManager Instance;
    private void Awake()
    {
        Instance = this;
        gfxRayCaster = FindObjectOfType<GraphicRaycaster>();
    }

    public ulong localClientId;


    public static GraphicRaycaster gfxRayCaster;
    private Camera mainCam;

    public LayerMask placeableLayer;


    public bool isPlacingTower;
    public bool towerSelected;


    public Transform towerPreviewHolder;
    private TowerPreview[] towerPreviews;
    public TowerPreview selectedPreviewTower;

    public TowerCore selectedTower;

    private GridObjectData selectedGridTileData;
    private Vector3 mousePos;

    public float currency;




    public void Init(LayerMask _placeableLayer, ulong _localClientId)
    {
        towerPreviews = towerPreviewHolder.GetComponentsInChildren<TowerPreview>();

        placeableLayer = _placeableLayer;
        localClientId = _localClientId;
        mainCam = Camera.main;
    }


    public void OnConfirm(InputAction.CallbackContext ctx)
    {
        if (TurnManager.Instance.isMyTurn == false)
        {
            return;
        }
        print("l");
        if (ctx.performed)
        {
            PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
            pointerEventData.position = Input.mousePosition;

            var results = new List<RaycastResult>();
            gfxRayCaster.Raycast(pointerEventData, results);

            if (results.Count > 0)
            {
                return;
            }

            if (isPlacingTower)
            {
                TryPlaceTower();
            }
            else
            {
                TrySelectTower();
            }
        }
    }
    public void OnCancel(InputAction.CallbackContext ctx)
    {
        if (TurnManager.Instance.isMyTurn == false)
        {
            return;
        }

        if (ctx.performed)
        {
            if (towerSelected)
            {
                towerSelected = false;
                selectedTower = null;
            }

            if (isPlacingTower)
            {
                selectedPreviewTower.transform.localPosition = Vector3.zero;
                isPlacingTower = false;
            }
        }
    }



    #region Select tower Preview

    public void SelectTowerPreview(int id)
    {
        if (TurnManager.Instance.isMyTurn == false)
        {
            return;
        }

        if (isPlacingTower)
        {
            selectedPreviewTower.transform.localPosition = Vector3.zero;
            UpdateTowerPreviewServerRPC(Vector3.zero);
        }
        if (towerSelected)
        {
            towerSelected = false;
        }

        selectedPreviewTower = towerPreviews[id];
        SelectTowerPreview_ServerRPC(id);

        selectedTower = null;
        isPlacingTower = true;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SelectTowerPreview_ServerRPC(int towerPreviewId, ServerRpcParams rpcParams = default)
    {
        SelectTowerPreview_ClientRPC(rpcParams.Receive.SenderClientId, towerPreviewId);
    }
    [ClientRpc(RequireOwnership = false)]
    private void SelectTowerPreview_ClientRPC(ulong clientId, int towerPreviewId)
    {
        if (localClientId == clientId)
        {
            return;
        }
        selectedPreviewTower = towerPreviews[towerPreviewId];
    }
    #endregion


    public void TryPlaceTower()
    {
        //place tower system
        if (selectedGridTileData.full == false)
        {
            if (currency >= selectedPreviewTower.cost)
            {
                PlaceTower();
            }
            else
            {
                CancelTowerPlacement();
            }
        }
    }
    private void CancelTowerPlacement()
    {
        selectedPreviewTower.transform.localPosition = Vector3.zero;
        UpdateTowerPreviewServerRPC(Vector3.zero);
        isPlacingTower = false;
    }
    private void PlaceTower()
    {
        selectedPreviewTower.towerPreviewRenderer.color = new Color(0.7619722f, 0.8740168f, 0.9547169f);
        selectedPreviewTower.UpdateTowerPreviewColor(Color.white);

        selectedPreviewTower.transform.localPosition = Vector3.zero;
        UpdateTowerPreviewServerRPC(Vector3.zero);
        isPlacingTower = false;

        selectedTower = Instantiate(selectedPreviewTower.towerPrefab, selectedGridTileData.worldPos, selectedPreviewTower.transform.rotation).GetComponent<TowerCore>();

        selectedTower.CoreInit();

        GridManager.Instance.UpdateGridDataFieldType(selectedGridTileData.gridPos, 3, selectedTower);
        GridManager.Instance.UpdateGridDataFieldType(selectedGridTileData.gridPos, (float)selectedTower.cost);


        TurnManager.Instance.isMyTurn = false;
        TurnManager.Instance.NextTurn_ServerRPC();
    }

    private void TrySelectTower()
    {
        Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hitInfo, 100, GridManager.Instance.p1 + GridManager.Instance.p2))
        {
            GridObjectData gridData = GridManager.Instance.GridObjectFromWorldPoint(hitInfo.point);
            if (gridData.tower != null && gridData.tower.towerCompleted)
            {
                //deselect older selected tower
                if (towerSelected)
                {
                    selectedTower.SelectOrDeselectTower(false);
                }

                //select tower
                selectedTower = gridData.tower;
                towerSelected = true;
                return;
            }
        }
        towerSelected = false;
    }



    private void Update()
    {
        if (isPlacingTower && Input.mousePosition != mousePos)
        {
            mousePos = Input.mousePosition;
            UpdateTowerPlacementPreview();
        }
    }

    private void UpdateTowerPlacementPreview()
    {
        Ray ray = mainCam.ScreenPointToRay(mousePos);

        if (Physics.Raycast(ray, out RaycastHit hitInfo, 100, placeableLayer))
        {
            selectedGridTileData = GridManager.Instance.GridObjectFromWorldPoint(hitInfo.point);

            if (selectedGridTileData.full && currency >= selectedPreviewTower.cost)
            {
                selectedPreviewTower.towerPreviewRenderer.color = new Color(0.7619722f, 0.8740168f, 0.9547169f);
                selectedPreviewTower.UpdateTowerPreviewColor(Color.white);

                UpdateTowerPreviewServerRPC(selectedGridTileData.worldPos);
            }
            else
            {
                selectedPreviewTower.towerPreviewRenderer.color = new Color(0.8943396f, 0.2309691f, 0.09955848f);
                selectedPreviewTower.UpdateTowerPreviewColor(Color.red);
            }
            selectedPreviewTower.transform.position = selectedGridTileData.worldPos;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateTowerPreviewServerRPC(Vector3 pos, ServerRpcParams rpcParams = default)
    {
        UpdateTowerPreviewClientRPC(rpcParams.Receive.SenderClientId, pos);
    }
    [ClientRpc(RequireOwnership = false)]
    private void UpdateTowerPreviewClientRPC(ulong fromClientId, Vector3 pos)
    {
        if (localClientId == fromClientId)
        {
            return;
        }

        selectedPreviewTower.towerPreviewRenderer.color = new Color(0.7619722f, 0.8740168f, 0.9547169f);
        selectedPreviewTower.UpdateTowerPreviewColor(Color.white);

        selectedPreviewTower.transform.position = pos;
    }
}
