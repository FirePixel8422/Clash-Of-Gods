using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tower : TowerCore
{
    public Animator attackAnimator;

    public Transform rotPoint;

    public Transform lookAtTransform;

    [HideInInspector]
    public SpriteRenderer towerPreviewRenderer;

    public float rotSpeed;

    public Transform projectileTransform;
    public float animShootTime;



    protected override void OnSetupTower()
    {   
        towerPreviewRenderer = GetComponentInChildren<SpriteRenderer>();

        attackAnimator.transform.rotation = Quaternion.identity;

        GetComponentInChildren<DirectionArrowValidator>().Init();

        if (rotPoint != null)
        {
            StartCoroutine(LookAtTarget_UpdateLoop());
        }
    }


    #region Tower Select/Deselect

    protected override void OnSelectTower()
    {
        attackAnimator.SetBool("Enabled", true);
    }

    protected override void OnDeSelectTower()
    {
        attackAnimator.SetBool("Enabled", false);
    }
    #endregion


    protected override IEnumerator AttackTargetAnimation(Vector3 targetPos, float combinedSize, TowerCore target = null)
    {
        lookAtTransform = target.centerPoint;

        anim.SetTrigger("Attack");

        yield return new WaitForSeconds(animShootTime);

        if (target != null)
        {
            target.GetAttacked(dmg, GodCore.Instance.RandomStunChance());
        }
    }




    private IEnumerator LookAtTarget_UpdateLoop()
    {
        while (true)
        {
            yield return null;
            yield return new WaitUntil(() => lookAtTransform != null);

            Vector3 direction = lookAtTransform.position - rotPoint.position;

            Quaternion targetRotation = Quaternion.LookRotation(direction);

            rotPoint.rotation = Quaternion.RotateTowards(rotPoint.rotation, targetRotation, rotSpeed * Time.deltaTime);
        }
    }
}
