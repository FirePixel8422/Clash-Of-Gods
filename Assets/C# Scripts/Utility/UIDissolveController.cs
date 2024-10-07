using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UIDissolveController : DissolveController
{
    public UIDissolveGroup uiDissolveGroup;

    public bool randomizeOnUse;

    private Coroutine dissolveCO;
    private Coroutine revertDissolveCO;


    public void Init(UIDissolveGroup group)
    {
        uiDissolveGroup = group;

        onDissolveComplete = new UnityEvent();
        onRevertDissolveComplete = new UnityEvent();

        onDissolveComplete.AddListener(uiDissolveGroup.ChildCreatedUI);
        onRevertDissolveComplete.AddListener(uiDissolveGroup.ChildDestroyedUI);
    }



    public void StartUIDissolve()
    {
        if (dissolveCO != null)
        {
            if (randomizeOnUse)
            {
                dissolveMaterial.SetVector(Shader.PropertyToID("_NoiseOffset"), new Vector2(Random.Range(-100f, 100f), Random.Range(-100f, 100f)));
            }

            StopCoroutine(dissolveCO);
        }

        if (revertDissolveCO != null)
        {
            StopCoroutine(revertDissolveCO);
        }

        dissolveCO = StartCoroutine(Dissolve());
    }

    public void RevertUIDissolve()
    {
        if (revertDissolveCO != null)
        {
            if (randomizeOnUse)
            {
                dissolveMaterial.SetVector(Shader.PropertyToID("_NoiseOffset"), new Vector2(Random.Range(-100f, 100f), Random.Range(-100f, 100f)));
            }

            StopCoroutine(revertDissolveCO);
        }

        if (dissolveCO != null)
        {
            StopCoroutine(dissolveCO);
        }

        revertDissolveCO = StartCoroutine(RevertDissolve());
    }
}
