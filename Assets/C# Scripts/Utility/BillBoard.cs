using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Camera mainCam;


    public bool freezeX, freezeY, freezeZ;
    public float offsetX, offsetY, offsetZ;


    private void Start()
    {
        mainCam = Camera.main;
    }

    private void Update()
    {
        Vector3 directionToCamera = mainCam.transform.position - transform.position;

        if (freezeX)
        {
            directionToCamera.x = 0;
        }
        if (freezeY)
        {
            directionToCamera.y = 0;
        }
        if (freezeZ)
        {
            directionToCamera.z = 0;
        }


        Vector3 rotation = Quaternion.LookRotation(directionToCamera).eulerAngles;
        transform.rotation = Quaternion.Euler(rotation.x + offsetX, rotation.y + offsetY, rotation.z + offsetZ);
    }
}