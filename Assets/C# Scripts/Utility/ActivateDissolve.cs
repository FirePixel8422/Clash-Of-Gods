using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateDissolve : MonoBehaviour
{
    public bool activateOnStart;

    public DissolveController[] dissolves;



    private void Start()
    {
        dissolves = GetComponentsInChildren<DissolveController>();

        if (activateOnStart)
        {
            foreach (DissolveController dissolve in dissolves)
            {
                dissolve.StartDissolve();
            }
        }
    }
}
