using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnMeteor : MonoBehaviour
{
    public GameObject vfx;
    public Transform startPoint;
    public Transform endPoint;


    // Start is called before the first frame update
    void Start()
    {
        Vector3 startPos = startPoint.position;
        GameObject objVFX = Instantiate(vfx, startPos, Quaternion.identity);

        Vector3 endPos = endPoint.position;

        RotateTo(objVFX, endPos);
        
    }

    void RotateTo (GameObject obj, Vector3 destination)
    {
        Vector3 direction = (destination - obj.transform.position).normalized;
        var rotation = Quaternion.LookRotation(direction);
        obj.transform.rotation = rotation;

    }

}
