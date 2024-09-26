using System.Collections;
using System.Collections.Generic;
using UnityEngine;


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
        dissolveMaterial = GetComponent<Renderer>().material;
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

    public void RevertPercent(TowerCore core, float percent)
    {
        StartCoroutine(RevertDissolvePercent(core, percent));
    }

    private IEnumerator RevertDissolvePercent(TowerCore core, float percent)
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