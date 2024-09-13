using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class AttackTower : TowerCore
{
    public Transform rotPoint;

    public Transform lookAtTransform;

    public float rotSpeed;



    public override void Start()
    {
        base.Start();
        StartCoroutine(LookAtTarget_UpdateLoop());
    }


    public override void OnGrantTurn()
    {
        base.OnGrantTurn();
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
