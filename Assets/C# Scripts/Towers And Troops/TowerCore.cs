using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(AudioController))]
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

    public Transform centerPoint;
    public float size;
    public bool drawSizeGizmos;

    public Animator underAttackArrowAnim;

    protected MeshRenderer underAttackArrowRenderer;
    public List<Color> underAttackArrowColors;

    private AudioController audioController;

    public Animator selectStateAnim;

    private HealthBar healthBar;


    [HideInInspector]
    public List<TowerCore> targets;

    [HideInInspector]
    public Animator anim;

    [HideInInspector]
    public bool towerCompleted;

    public bool stunned;

    public int actionsPerTurn;
    [HideInInspector]
    public int actionsLeft;

    public bool useSelectionFlicker;

    public float soundDelay;

    public bool underAttack;

    #region Tower Setup And Initialize

    public virtual void CoreInit()
    {
        TurnManager.Instance.OnMyTurnStartedEvent.AddListener(() => GrantTurn());
        if (GodCore.Instance.IsHades == false)
        {
            TurnManager.Instance.OnTurnChangedEvent.AddListener(() => TurnChanged());
        }

        underAttackArrowRenderer = underAttackArrowAnim.GetComponentInChildren<MeshRenderer>();
        underAttackArrowColors.Add(PlacementManager.Instance.playerColors[NetworkObject.OwnerClientId]);

        audioController = GetComponent<AudioController>();


        SettingsManager.SingleTon.audioControllers.Add(audioController);

        anim = GetComponent<Animator>();

        healthBar = GetComponent<HealthBar>();

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

        if (selectStateAnim != null)
        {
            selectStateAnim.SetBool("Enabled", true);
        }

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
            target.GetTargetted(false, actionsLeft != 0);
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

    public virtual void DissolveCompleted()
    {
        cDissolves += 1;
        if (cDissolves == amountOfDissolves)
        {
            towerCompleted = true;
            OnTowerCompleted();
        }
    }

    public virtual void RevertCompleted()
    {
        cDissolves -= 1;
        if (cDissolves == 0)
        {
            DespawnTower_ServerRPC();
        }
    }
    [ServerRpc(RequireOwnership = false)]
    private void DespawnTower_ServerRPC()
    {
        NetworkObject.Despawn(gameObject);
    }
    #endregion



    #region On Grant/Lose Turn

    public void GrantTurn()
    {
        if (stunned)
        {
            stunned = false;
            return;
        }

        actionsLeft = actionsPerTurn;
        OnGrantTurn();
    }
    protected virtual void OnGrantTurn()
    {
        return;
    }

    public void TurnChanged()
    {
        if (GridManager.Instance.GridObjectFromWorldPoint(transform.position).onFire > 0)
        {
            GetAttacked(GodCore.Instance.fireDamage, false);
        }
    }

    public void LoseAction()
    {
        actionsLeft -= 1;
        OnLoseAction();
    }
    public virtual void OnLoseAction()
    {
        return;
    }
    #endregion


    



    #region Target, Attack and Animation

    public void AttackTarget(TowerCore target)
    {
        target.underAttack = true;

        float combinedSize = (target.size + size) / 2;

        AttackTarget_ServerRPC(target.transform.position, combinedSize);

        StartCoroutine(SoundDelay(soundDelay));

        StartCoroutine(AttackTargetAnimation(target.transform.position, combinedSize, target));

        PlacementManager.Instance.playedAnything = true;
    }

   

    private IEnumerator SoundDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        audioController.Play();
    }

    [ServerRpc(RequireOwnership = false)]
    private void AttackTarget_ServerRPC(Vector3 targetPos, float combinedSize, ServerRpcParams rpcParams = default)
    {
        ulong fromClientId = rpcParams.Receive.SenderClientId;
        AttackTarget_ClientRPC(fromClientId, targetPos, combinedSize);
    }

    [ClientRpc(RequireOwnership = false)]
    private void AttackTarget_ClientRPC(ulong fromClientId, Vector3 targetPos, float combinedSize)
    {
        if (TurnManager.Instance.localClientId == fromClientId)
        {
            return;
        }

        StartCoroutine(AttackTargetAnimation(targetPos, combinedSize));
    }
    protected virtual IEnumerator AttackTargetAnimation(Vector3 targetPos, float combinedSize, TowerCore target = null)
    {
        yield break;
    }


    public void GetTargetted(bool state, bool canAttackerAttack)
    {
        underAttackArrowAnim.SetBool("Enabled", state);

        underAttackArrowRenderer.material.SetColor(Shader.PropertyToID("_Base_Color"), underAttackArrowColors[canAttackerAttack ? 1 : 0]);
    }


    public void GetAttacked(int dmg, bool stun)
    {
        StartCoroutine(GetAttackedAnimations(dmg, stun));

        GetAttacked_ServerRPC(dmg, stun);
    }


    [ServerRpc(RequireOwnership = false)]
    private void GetAttacked_ServerRPC(int dmg, bool stun, ServerRpcParams rpcParams = default)
    {
        ulong fromClientId = rpcParams.Receive.SenderClientId;
        GetAttacked_ClientRPC(fromClientId, dmg, stun);
    }

    [ClientRpc(RequireOwnership = false)]
    private void GetAttacked_ClientRPC(ulong fromClientId, int dmg, bool stun)
    {
        if (TurnManager.Instance.localClientId == fromClientId)
        {
            return;
        }

        StartCoroutine(GetAttackedAnimations(dmg, stun));
    }


    public virtual IEnumerator GetAttackedAnimations(int dmg, bool stun)
    {
        underAttack = false;

        health -= dmg;
        stunned = stun;

        if (stunned)
        {
            print("STUNNED");
        }

        if (healthBar != null)
        {
            healthBar.UpdateHealthBar(Mathf.Clamp(health, 0, int.MaxValue));
        }


        underAttackArrowAnim.SetBool("Enabled", false);

        yield return null;

        if (health <= 0)
        {
            OnDeath();
        }
    }
    public virtual void OnDeath()
    {
        anim.SetTrigger("Death");
        foreach (var dissolve in dissolves)
        {
            dissolve.Revert(this);
        }
        GridObjectData gridObjectData = GridManager.Instance.GridObjectFromWorldPoint(transform.position);
        GridManager.Instance.UpdateTowerData(gridObjectData.gridPos, null);
    }
    #endregion



    private void OnDrawGizmos()
    {
        if (drawSizeGizmos && centerPoint != null && size != 0)
        {
            Gizmos.DrawWireCube(centerPoint.position, Vector3.one * size);
        }
    }


    public override void OnDestroy()
    {
        SettingsManager.SingleTon.audioControllers.Remove(audioController);
    }
}