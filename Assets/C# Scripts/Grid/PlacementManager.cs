using System.Collections.Generic;
using System.Runtime.InteropServices;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlacementManager : NetworkBehaviour
{
    public static PlacementManager Instance;
    private void Awake()
    {
        Instance = this;
        gfxRayCaster = FindObjectOfType<GraphicRaycaster>(true);
    }



    [SerializeField] private bool interactable;

    public bool Interactable
    {
        get
        {
            return interactable;
        }
        set
        {
            interactable = value;
            if (interactable == false)
            {
                DeSelectTower();

                if (isPlacingTower)
                {
                    selectedPreviewTower.transform.localPosition = Vector3.zero;
                    UpdateTowerPreviewServerRPC(Vector3.zero, 0, true);
                    isPlacingTower = false;
                }
            }
        }
    }



    public ulong localClientId;
    public int towerForwardRotationY;

    [ColorUsage(true, true)]
    public Color[] playerColors;


    public static GraphicRaycaster gfxRayCaster;
    private Camera mainCam;

    public Transform selectionSprite;


    [HideInInspector]
    public LayerMask ownFieldLayers;
    [HideInInspector]
    public LayerMask neutralLayers;
    [HideInInspector]
    public LayerMask fullFieldLayers;


    public bool isPlacingTower;
    public bool towerSelected;


    public Transform towerPreviewHolder;
    private TowerPreview[] towerPreviews;
    public TowerPreview selectedPreviewTower;

    public int maxTowerPlacements;
    [HideInInspector]
    public int cTowerPlacements;

    public TowerCore selectedTower;

    private GridObjectData selectedGridTileData;
    private Vector3 mousePos;



    public TextMeshProUGUI currencyTextObj;

    public int Currency
    {
        get
        {
            return currency;
        }
        set
        {
            currency = value;
            currencyTextObj.text = useGreekCurrency ? RomanNumeralConverter.IntToRoman(currency) : currency.ToString();
        }
    }
    public int currency;
    public int currencyGeneration;
    public int skipTurncurrencyGeneration;
    public bool playedAnything;
    public bool useGreekCurrency;




    public bool initialized;
    public void Init(LayerMask _ownFieldLayers, LayerMask _neutralLayers, LayerMask _fullFieldLayers, ulong _localClientId)
    {
        TurnManager.Instance.OnMyTurnStartedEvent.AddListener(() => GrantTurn());
        TurnManager.Instance.OnMyTurnEndedEvent.AddListener(() => EndTurn());

        cTowerPlacements = maxTowerPlacements;

        towerPreviews = towerPreviewHolder.GetComponentsInChildren<TowerPreview>();

        ownFieldLayers = _ownFieldLayers;
        neutralLayers = _neutralLayers;
        fullFieldLayers = _fullFieldLayers;

        localClientId = _localClientId;
        towerForwardRotationY = localClientId == 0 ? 90 : -90;

        mainCam = Camera.main;

        selectedGridTileData = new GridObjectData
        {
            full = true,
            type = -1,
            gridPos = new Vector2Int(-1, -1),
            worldPos = new Vector3(0, 2, 0),
        };

        Currency += 0;

        initialized = true;
    }


    public void OnConfirm(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            OnConfirmEvent.Invoke();
        }


        if (TurnManager.Instance.isMyTurn == false || interactable == false || ctx.performed == false)
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

        if (isPlacingTower && TurnManager.Instance.isMyTurn)
        {
            TryPlaceTower();
        }
        else
        {
            TrySelectTower();
        }
    }

    public UnityEvent OnConfirmEvent;


    public void OnCancel(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            OnCancelEvent.Invoke();
        }

        if (TurnManager.Instance.isMyTurn == false || interactable == false || ctx.performed == false)
        {
            return;
        }


        DeSelectTower();

        if (isPlacingTower)
        {
            selectedPreviewTower.transform.localPosition = Vector3.zero;
            UpdateTowerPreviewServerRPC(Vector3.zero, 0, true);
            isPlacingTower = false;
        }
    }

    public UnityEvent OnCancelEvent;



    #region Select tower Preview

    public void SelectTowerPreview(int id)
    {
        if (interactable == false || TurnManager.Instance.isMyTurn == false || cTowerPlacements == 0)
        {
            return;
        }


        if (isPlacingTower && selectedPreviewTower != towerPreviews[id])
        {
            selectedPreviewTower.transform.localPosition = Vector3.zero;
            UpdateTowerPreviewServerRPC(Vector3.zero, 0, true);
        }

        if (towerSelected)
        {
            selectedTower.DeSelectTower();
            
            towerSelected = false;
        }

        selectedPreviewTower = towerPreviews[id];
        SelectTowerPreview_ServerRPC(id);

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
                Currency -= selectedPreviewTower.cost;

                GridManager.Instance.UpdateGridDataFullState(selectedGridTileData.gridPos, true);
                selectedGridTileData.full = true;

                PlaceTower();
                cTowerPlacements -= 1;
            }
            else
            {
                CancelTowerPlacement();
            }
        }
    }

    private void CancelTowerPlacement()
    {
        if (selectedPreviewTower != null)
        {
            selectedPreviewTower.transform.localPosition = Vector3.zero;
        }
        UpdateTowerPreviewServerRPC(Vector3.zero, 0, true);
        isPlacingTower = false;
    }

    private void PlaceTower()
    {
        playedAnything = true;

        selectedPreviewTower.towerPreviewRenderer.color = new Color(0.7619722f, 0.8740168f, 0.9547169f);
        selectedPreviewTower.UpdateTowerPreviewColor(true);

        selectedPreviewTower.transform.localPosition = Vector3.zero;
        UpdateTowerPreviewServerRPC(Vector3.zero, 0, true);


        isPlacingTower = false;
        PlaceTower_ServerRPC(selectedGridTileData.worldPos, localClientId == 0 ? 90 : -90, selectedGridTileData.gridPos);
    }

    [ServerRpc(RequireOwnership = false)]
    private void PlaceTower_ServerRPC(Vector3 pos, int rotY, Vector2Int gridPos, ServerRpcParams rpcParams = default)
    {
        TowerCore spawnedTower = Instantiate(selectedPreviewTower.towerPrefab, pos, Quaternion.Euler(0, rotY, 0)).GetComponent<TowerCore>();

        ulong fromClientId = rpcParams.Receive.SenderClientId;
        spawnedTower.NetworkObject.SpawnWithOwnership(fromClientId, true);

        PlaceTower_ClientRPC(fromClientId, gridPos, spawnedTower.NetworkObjectId);
    }

    [ClientRpc(RequireOwnership = false)]
    private void PlaceTower_ClientRPC(ulong fromClientId, Vector2Int gridPos, ulong spawnedTowerNetworkObjectId)
    {
        selectedTower = NetworkManager.SpawnManager.SpawnedObjects[spawnedTowerNetworkObjectId].GetComponent<TowerCore>();
        selectedTower.CoreInit();

        if(selectedTower.TryGetComponent(out Troop _))
        {
            Renderer[] renderers = selectedTower.transform.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer renderer in renderers)
            {
                if (renderer.gameObject.CompareTag("TeamColor"))
                {
                    renderer.material.SetColor(Shader.PropertyToID("_Base_Color"), playerColors[GodCore.Instance.chosenGods[fromClientId]]);
                }
            }
        }

        GridManager.Instance.UpdateTowerData(gridPos, selectedTower);
        selectedTower = null;
    }
    #endregion


    public void GrantTurn()
    {
        cTowerPlacements = maxTowerPlacements;

        Currency += currencyGeneration;

        if (playedAnything == false)
        {
            Currency += skipTurncurrencyGeneration;
        }
        playedAnything = false;
    }
    public void EndTurn()
    {
        CancelTowerPlacement();
        DeSelectTower();
    }




    private void TrySelectTower()
    {
        Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hitInfo, 100, fullFieldLayers))
        {
            GridObjectData gridData = GridManager.Instance.GridObjectFromWorldPoint(hitInfo.point);
            if (gridData.tower != null && localClientId == gridData.tower.OwnerClientId)
            {
                //deselect older selected tower
                if (towerSelected)
                {
                    selectedTower.DeSelectTower();
                }

                if (towerSelected == false || selectedTower != gridData.tower)
                {
                    //select tower
                    selectedTower = gridData.tower;

                    selectedTower.SelectTower();
                    towerSelected = true;
                }
                else
                {
                    towerSelected = false;
                }

                return;
            }
        }
        DeSelectTower();
    }
    private void DeSelectTower()
    {
        if (selectedTower != null)
        {
            selectedTower.DeSelectTower();
        }
        towerSelected = false;
    }


    #region Tower/Troop Preview Update

    private void Update()
    {
        if (initialized && Input.mousePosition != mousePos)
        {
            mousePos = Input.mousePosition;
            UpdateTowerPlacementPreview();
        }
    }

    public void UpdateTowerPlacementPreview()
    {
        Ray ray = mainCam.ScreenPointToRay(mousePos);

        if (Physics.Raycast(ray, out RaycastHit hitInfo, 100, ownFieldLayers))
        {
            selectedGridTileData = GridManager.Instance.GridObjectFromWorldPoint(hitInfo.point);

            if (isPlacingTower)
            {
                if (selectedPreviewTower.CompareTag("Goldmine") && (selectedGridTileData.gridPos.x < (GridManager.Instance.gridSizeX / 2 - 4) || selectedGridTileData.gridPos.x > (GridManager.Instance.gridSizeX / 2 + 4)))
                {
                    return;
                }

                if (selectedGridTileData.full == false && selectedGridTileData.type == (int)localClientId && currency >= selectedPreviewTower.cost)
                {
                    selectedPreviewTower.towerPreviewRenderer.color = new Color(0.03529412f, 1f, 0f);
                    selectedPreviewTower.UpdateTowerPreviewColor(true);

                    UpdateTowerPreviewServerRPC(selectedGridTileData.worldPos, towerForwardRotationY);
                }
                else
                {
                    selectedPreviewTower.towerPreviewRenderer.color = new Color(0.8943396f, 0.2309691f, 0.09955848f);

                    selectedPreviewTower.UpdateTowerPreviewColor(false);
                }

                selectedPreviewTower.transform.position = selectedGridTileData.worldPos;
                selectedPreviewTower.transform.rotation = Quaternion.Euler(0, towerForwardRotationY, 0);
            }


            selectionSprite.position = selectedGridTileData.worldPos;
        }
        else
        {
            selectionSprite.position = new Vector3(0, 100, 0);
        }
    }


    [ServerRpc(RequireOwnership = false)]
    private void UpdateTowerPreviewServerRPC(Vector3 pos, int rotY, bool resetPos = false, ServerRpcParams rpcParams = default)
    {
        UpdateTowerPreviewClientRPC(rpcParams.Receive.SenderClientId, pos, rotY, resetPos);
    }

    [ClientRpc(RequireOwnership = false)]
    private void UpdateTowerPreviewClientRPC(ulong fromClientId, Vector3 pos, int rotY, bool resetPos)
    {
        if (localClientId == fromClientId)
        {
            return;
        }

        selectedPreviewTower.towerPreviewRenderer.color = new Color(0.7619722f, 0.8740168f, 0.9547169f);
        selectedPreviewTower.UpdateTowerPreviewColor(true);

        if (resetPos)
        {
            selectedPreviewTower.transform.localPosition = Vector3.zero;
        }
        else
        {
            selectedPreviewTower.transform.SetPositionAndRotation(pos, Quaternion.Euler(0, rotY, 0));
        }
    }
    #endregion

}
