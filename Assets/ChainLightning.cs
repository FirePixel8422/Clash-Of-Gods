using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;



public class ChainLightning : MonoBehaviour
{
    private Vector3 prevPos;

    public Transform rotTransform;
    public Vector3 coneOffset;


    private void Start()
    {
        prevPos = transform.position;
    }

    private void LateUpdate()
    {
        Vector3 movementDirection = transform.position - prevPos;

        if (movementDirection.sqrMagnitude > 0.0001f)
        {
            movementDirection.Normalize();

            Quaternion targetRotation = Quaternion.LookRotation(movementDirection);

            rotTransform.rotation = targetRotation;
            rotTransform.position = targetRotation * coneOffset;
        }
        prevPos = transform.position;
    }
}
