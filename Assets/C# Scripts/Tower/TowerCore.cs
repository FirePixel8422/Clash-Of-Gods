using System.Collections;
using Unity.Netcode;
using UnityEngine;


public class TowerCore : NetworkBehaviour
{
    #region Dissolve Variables

    [HideInInspector]
    public DissolveController[] dissolves;
    [HideInInspector]
    public int amountOfDissolves;
    [HideInInspector]
    public int cDissolves;
    #endregion


    public int cost;

    public int movement;
    public float moveSpeed;


    [HideInInspector]
    public SpriteRenderer towerPreviewRenderer;

    public Animator towerMoveArrowsAnim;
    private SpriteRenderer[] towerMoveArrowRenderers;
    public Color[] moveArrowColors;

    [HideInInspector]
    public Animator anim;

    [HideInInspector]
    public bool towerCompleted;


    #region Tower Setup

    public virtual void Start()
    {
        SetupTower();
    }

    private void SetupTower()
    {
        dissolves = GetComponentsInChildren<DissolveController>();
        anim = GetComponent<Animator>();

        towerPreviewRenderer = GetComponentInChildren<SpriteRenderer>();

        towerMoveArrowsAnim = GetComponentInChildren<Animator>();
        if (towerMoveArrowsAnim != null)
        {
            towerMoveArrowRenderers = towerMoveArrowsAnim.GetComponentsInChildren<SpriteRenderer>();
        }
    }
    public virtual void CoreInit()
    {
        SetupTower();

        amountOfDissolves = dissolves.Length;
        foreach (var dissolve in dissolves)
        {
            dissolve.StartDissolve(this);
        }
        Init();
    }
    public virtual void Init()
    {

    }
    public virtual void OnTowerCompleted()
    {

    }
    #endregion


    #region Tower Select/Deselect

    public virtual void SelectTower()
    {
        if (movement != 0)
        {
            towerMoveArrowsAnim.SetBool("Enabled", true);

            foreach (var sprite in towerMoveArrowRenderers)
            {
                sprite.color = moveArrowColors[TurnManager.Instance.isMyTurn ? 0 : 1];
            }
        }
        else
        {
            towerPreviewRenderer.enabled = true;
        }

        foreach (var d in dissolves)
        {
            d.dissolveMaterial.SetInt("_Selected", 1);
        }
    }
    public virtual void DeSelectTower()
    {
        if (movement != 0)
        {
            towerMoveArrowsAnim.SetBool("Enabled", false);
        }
        else
        {
            towerPreviewRenderer.enabled = false;
        }

        foreach (var d in dissolves)
        {
            d.dissolveMaterial.SetInt("_Selected", 0);
        }
    }
    #endregion


    #region Dissolve And Preview

    public void UpdateTowerPreviewColor(Color color)
    {
        foreach (var d in dissolves)
        {
            d.dissolveMaterial.SetColor("_PreviewColor", color);
        }
    }

    public void DissolveCompleted()
    {
        cDissolves += 1;
        if (cDissolves == amountOfDissolves)
        {
            towerCompleted = true;
            OnTowerCompleted();
        }
    }

    public void RevertCompleted()
    {
        cDissolves -= 1;
        if (cDissolves == 0)
        {
            Destroy(gameObject);
        }
    }
    #endregion


    #region Move Tower

    public virtual void MoveTower(Vector2Int currentGridPos, Vector2Int newGridPos)
    {
        if (TurnManager.Instance.isMyTurn == false)
        {
            return;
        }
        TurnManager.Instance.isMyTurn = false;


        GridManager.Instance.UpdateTowerData(currentGridPos, null);
        GridManager.Instance.UpdateTowerData(newGridPos, this);


        StartCoroutine(MoveTowerAnimation(currentGridPos, newGridPos));

        MoveTower_ServerRPC(currentGridPos, newGridPos);
    }


    [ServerRpc(RequireOwnership = false)]
    private void MoveTower_ServerRPC(Vector2Int currentGridPos, Vector2Int newGridPos, ServerRpcParams rpcParams = default)
    {
        ulong fromClientId = rpcParams.Receive.SenderClientId;
        MoveTower_ClientRPC(fromClientId, currentGridPos, newGridPos);
    }

    [ClientRpc(RequireOwnership = false)]
    private void MoveTower_ClientRPC(ulong fromClientId, Vector2Int currentGridPos, Vector2Int newGridPos)
    {
        if (TurnManager.Instance.localClientId != fromClientId)
        {
            StartCoroutine(MoveTowerAnimation(currentGridPos, newGridPos));
        }
    }

    private IEnumerator MoveTowerAnimation(Vector2Int currentGridPos, Vector2Int newGridPos)
    {
        GridManager.Instance.UpdateTowerData(currentGridPos, null);
        GridManager.Instance.UpdateTowerData(newGridPos, this);


        Vector3 newPos = GridManager.Instance.GetGridData(newGridPos).worldPos;
        newPos = new Vector3(newPos.x, transform.position.y, newPos.z);

        while (Vector3.Distance(transform.position, newPos) > 0.001f)
        {
            yield return null;
            transform.position = VectorLogic.InstantMoveTowards(transform.position, newPos, moveSpeed * Time.deltaTime);
        }
    }
    #endregion
}