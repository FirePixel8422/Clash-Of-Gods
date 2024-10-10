using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Random90Rotation : MonoBehaviour
{
    private void Start()
    {
        List<Transform> transforms = GetComponentsInChildren<Transform>().ToList();
        transforms.Remove(transform);


        for (int i = 0; i < transforms.Count; i++)
        {
            transform.rotation = Quaternion.Euler(0, 90 * Random.Range(0, 4), 0);
        }
    }
}
