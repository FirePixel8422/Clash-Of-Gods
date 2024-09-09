using System.Collections;
using UnityEngine;


public class TowerCore : MonoBehaviour
{
    [HideInInspector]
    public DissolveController[] dissolves;
    [HideInInspector]
    public int amountOfDissolves;
    [HideInInspector]
    public int cDissolves;

    public int cost;


    [HideInInspector]
    public bool towerCompleted;

    [HideInInspector]
    public SpriteRenderer towerPreviewRenderer;
    [HideInInspector]
    public Animator anim;
    public int onHitEffectIndex = -1;


    public virtual void Start()
    {
        SetupTower();
    }

    private void SetupTower()
    {
        dissolves = GetComponentsInChildren<DissolveController>();
        towerPreviewRenderer = GetComponentInChildren<SpriteRenderer>();
        anim = GetComponent<Animator>();
    }
    public virtual void CoreInit()
    {
        SetupTower();

        amountOfDissolves = dissolves.Length;
        foreach (var dissolve in dissolves)
        {
            dissolve.StartDissolve(this);
        }

        towerPreviewRenderer.enabled = false;
        Init();
    }
    public virtual void Init()
    {

    }

    public virtual void SelectOrDeselectTower(bool select)
    {
        towerPreviewRenderer.enabled = select;

        foreach (var d in dissolves)
        {
            d.dissolveMaterial.SetInt("_Selected", select ? 1 : 0);
        }
    }


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
    public virtual void OnTowerCompleted()
    {

    }
}