using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XrayController : MonoBehaviour
{
    public Material dissolveMaterial;


    private void Awake()
    {
        dissolveMaterial = GetComponent<Renderer>().material;
    }
}
