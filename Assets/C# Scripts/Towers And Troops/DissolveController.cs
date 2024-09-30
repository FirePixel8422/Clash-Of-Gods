using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class DissolveController : MonoBehaviour
{
    public Material dissolveMaterial;

    private float cDissolveEffectState;
    public float startDelay;
    public float revertDelay;
    public float startDissolveEffectState;
    public float dissolveSpeed;
    public float endDisolveValue;



    private void Awake()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer == null)
        {
            Image image = GetComponent<Image>();
            if (image != null)
            {
                dissolveMaterial = image.material;
            }
        }
        else
        {
            dissolveMaterial = renderer.material;
        }

        dissolveMaterial.SetVector(Shader.PropertyToID("_NoiseOffset"), new Vector2(Random.Range(-100f, 100f), Random.Range(-100f, 100f)));
    }
    public void StartDissolve(TowerCore core = null)
    {
        dissolveMaterial = GetComponent<Renderer>().material;
        StartCoroutine(Dissolve(core));
    }
    public void Revert(TowerCore core)
    {
        StartCoroutine(RevertDissolve(core));
    }


    private IEnumerator Dissolve(TowerCore core)
    {
        dissolveMaterial.SetFloat("_Disolve_Active", startDissolveEffectState);
        yield return new WaitForSeconds(startDelay);

        cDissolveEffectState = startDissolveEffectState;
        while (cDissolveEffectState > endDisolveValue)
        {
            yield return null;
            cDissolveEffectState -= Time.deltaTime * dissolveSpeed;
            dissolveMaterial.SetFloat("_Disolve_Active", cDissolveEffectState);
        }
        if (core != null)
        {
            core.DissolveCompleted();
        }
    }
    private IEnumerator RevertDissolve(TowerCore core)
    {
        yield return new WaitForSeconds(revertDelay);

        while (cDissolveEffectState < startDissolveEffectState)
        {
            yield return null;
            cDissolveEffectState += Time.deltaTime * dissolveSpeed;
            dissolveMaterial.SetFloat("_Disolve_Active", cDissolveEffectState);
        }
        core.RevertCompleted();
    }

    public void RevertPercent(float percent)
    {
        StartCoroutine(RevertDissolvePercent(percent));
    }

    private IEnumerator RevertDissolvePercent(float percent)
    {
        float _endDissolveValue = endDisolveValue / startDissolveEffectState * percent;

        while (cDissolveEffectState < _endDissolveValue)
        {
            yield return null;
            cDissolveEffectState += Time.deltaTime * dissolveSpeed;
            dissolveMaterial.SetFloat("_Disolve_Active", cDissolveEffectState);
        }
    }
}