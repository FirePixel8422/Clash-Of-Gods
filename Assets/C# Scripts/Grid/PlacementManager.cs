using System.Collections.Generic;
using System.Runtime.InteropServices;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
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

    public Color[] playerColors;


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



    public TextMeshProUGUI currencyTextObj;

    public float Currency
    {
        get
        {
            return currency;
        }
        set
        {
            currency = value;
            //currencyTextObj.text = Mathf.RoundToInt(currency).ToString();
        }
    }
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

            if (isPlacingTower && TurnManager.Instance.isMyTurn)
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
            UpdateTowerPreviewServerRPC(Vector3.zero, true);
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



    #region Place Tower

    public void TryPlaceTower()
    {
        //place tower system
        if (selectedGridTileData.full == false && selectedGridTileData.type == (int)localClientId)
        {
            if (currency >= selectedPreviewTower.cost)
            {
                GridManager.Instance.UpdateGridDataFullState(selectedGridTileData.gridPos, true);
                selectedGridTileData.full = true;

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
        UpdateTowerPreviewServerRPC(Vector3.zero, true);
        isPlacingTower = false;
    }
    private void PlaceTower()
    {
        selectedPreviewTower.towerPreviewRenderer.color = new Color(0.7619722f, 0.8740168f, 0.9547169f);
        selectedPreviewTower.UpdateTowerPreviewColor(Color.white);

        selectedPreviewTower.transform.localPosition = Vector3.zero;
        UpdateTowerPreviewServerRPC(Vector3.zero, true);
        isPlacingTower = false;

        if (towerSelected)
        {
            selectedTower.DeSelectTower();
            towerSelected = false;
        }

        PlaceTower_ServerRPC(selectedGridTileData.worldPos, selectedGridTileData.gridPos);
    }

    [ServerRpc(RequireOwnership = false)]
    private void PlaceTower_ServerRPC(Vector3 pos, Vector2Int gridPos, ServerRpcParams rpcParams = default)
    {
        TowerCore spawnedTower = Instantiate(selectedPreviewTower.towerPrefab, pos, selectedPreviewTower.transform.rotation).GetComponent<TowerCore>();

        ulong fromClientId = rpcParams.Receive.SenderClientId;
        spawnedTower.NetworkObject.SpawnWithOwnership(fromClientId, true);

        PlaceTower_ClientRPC(fromClientId, gridPos, spawnedTower.NetworkObjectId);
    }

    [ClientRpc(RequireOwnership = false)]
    private void PlaceTower_ClientRPC(ulong fromClientId, Vector2Int gridPos, ulong spawnedTowerNetworkObjectId)
    {
        selectedTower = NetworkManager.SpawnManager.SpawnedObjects[spawnedTowerNetworkObjectId].GetComponent<TowerCore>();
        selectedTower.CoreInit();

        selectedTower.transform.GetComponentInChildren<MeshRenderer>(true).material.SetColor(Shader.PropertyToID("_Base_Color"), playerColors[fromClientId]);

        GridManager.Instance.UpdateGridDataFieldType(gridPos, selectedTower);
    }
    #endregion




    private void TrySelectTower()
    {
        Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hitInfo, 100))
        {
            GridObjectData gridData = GridManager.Instance.GridObjectFromWorldPoint(hitInfo.point);
            if (gridData.tower != null && gridData.tower.towerCompleted && localClientId == gridData.tower.OwnerClientId)
            {
                //deselect older selected tower
                if (towerSelected)
                {
                    selectedTower.DeSelectTower();
                }

                //select tower
                selectedTower = gridData.tower;

                selectedTower.SelectTower();
                towerSelected = true;

                return;
            }
        }

        if (towerSelected)
        {
            selectedTower.DeSelectTower();
            towerSelected = false;
        }
    }



    #region Tower/Troop Placement Preview

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

            if (selectedGridTileData.type == (int)localClientId && currency >= selectedPreviewTower.cost)
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
    private void UpdateTowerPreviewServerRPC(Vector3 pos, bool resetPos = false, ServerRpcParams rpcParams = default)
    {
        UpdateTowerPreviewClientRPC(rpcParams.Receive.SenderClientId, pos, resetPos);
    }
    [ClientRpc(RequireOwnership = false)]
    private void UpdateTowerPreviewClientRPC(ulong fromClientId, Vector3 pos, bool resetPos)
    {
        if (localClientId == fromClientId)
        {
            return;
        }

        selectedPreviewTower.towerPreviewRenderer.color = new Color(0.7619722f, 0.8740168f, 0.9547169f);
        selectedPreviewTower.UpdateTowerPreviewColor(Color.white);

        if (resetPos)
        {
            selectedPreviewTower.transform.localPosition = Vector3.zero;
        }
        else
        {
            selectedPreviewTower.transform.position = pos;
        }
    }
    #endregion

}
