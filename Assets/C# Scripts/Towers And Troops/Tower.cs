using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class Tower : TowerCore
{
    public Transform rotPoint;

    public Transform lookAtTransform;

    [HideInInspector]
    public SpriteRenderer towerPreviewRenderer;

    public float rotSpeed;



    protected override void OnSetupTower()
    {
        towerPreviewRenderer = GetComponentInChildren<SpriteRenderer>();

        StartCoroutine(LookAtTarget_UpdateLoop());
    }


    #region Tower Select/Deselect

    protected override void OnSelectTower()
    {

    }

    protected override void OnDeSelectTower()
    {

    }
    #endregion


    protected override IEnumerator AttackTargetAnimation(Vector3 targetPos, float combinedSize, TowerCore target = null)
    {
        yield break;
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
