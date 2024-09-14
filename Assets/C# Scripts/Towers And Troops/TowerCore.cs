using System.Collections;
using System.Collections.Generic;
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

    public int health;
    public int dmg;

    public Animator underAttackArrowAnim;

    public Animator selectStateAnim;


    [HideInInspector]
    public List<TowerCore> targets;

    [HideInInspector]
    public Animator anim;

    [HideInInspector]
    public bool towerCompleted;

    public bool useSelectionFlicker;



    #region Tower Setup And Initialize

    public virtual void CoreInit()
    {
        TurnManager.Instance.OnMyTurnStartedEvent.AddListener(() => OnGrantTurn());

        anim = GetComponent<Animator>();


        dissolves = GetComponentsInChildren<DissolveController>();

        amountOfDissolves = dissolves.Length;
        foreach (var dissolve in dissolves)
        {
            dissolve.StartDissolve(this);
        }

        OnSetupTower();
    }
    protected virtual void OnSetupTower()
    {
        return;
    }
    protected virtual void OnTowerCompleted()
    {
        return; 
    }
    #endregion


    #region Tower Select/Deselect

    public void SelectTower()
    {
        if (useSelectionFlicker)
        {
            foreach (var d in dissolves)
            {
                d.dissolveMaterial.SetInt("_Selected", 1);
            }
        }

        selectStateAnim.SetBool("Enabled", true);

        //expirimental
        if (anim != null)
        {
            anim.SetTrigger("Select");
        }

        OnSelectTower();
    }
    protected virtual void OnSelectTower()
    {
        return;
    }


    public void DeSelectTower()
    {
        if (useSelectionFlicker)
        {
            foreach (var d in dissolves)
            {
                d.dissolveMaterial.SetInt("_Selected", 0);
            }
        }

        selectStateAnim.SetBool("Enabled", false);

        foreach (TowerCore target in targets)
        {
            target.GetTargetted(false);
        }
        targets.Clear();

        OnDeSelectTower();
    }
    protected virtual void OnDeSelectTower()
    {

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


    public virtual void OnGrantTurn()
    {
        return;
    }
    public virtual void OnLoseTurn()
    {
        return;
    }


    public void GetTargetted(bool state)
    {
        underAttackArrowAnim.SetBool("Enabled", state);
    }
    public void GetAttacked()
    {
        StartCoroutine(GetAttackedAnimation());
    }

    private IEnumerator GetAttackedAnimation()
    {
        underAttackArrowAnim.SetBool("Enabled", false);

        yield return null;
    }
}