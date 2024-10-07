using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


public class DissolveController : MonoBehaviour
{
    public Material dissolveMaterial;

    private float cDissolveEffectState;

    public float startDelay;
    public float revertDelay;

    public float startDissolveEffectState;

    public float dissolveSpeed;
    public float revertDissolveSpeed;

    public float endDisolveValue;

    protected UnityEvent onDissolveComplete;
    protected UnityEvent onRevertDissolveComplete;



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

        onDissolveComplete = new UnityEvent();
        onRevertDissolveComplete = new UnityEvent();
    }

    public void StartDissolve(TowerCore core = null)
    {
        dissolveMaterial = GetComponent<Renderer>().material;
        StartCoroutine(Dissolve(core));
    }
    public void Revert(TowerCore core = null)
    {
        StartCoroutine(RevertDissolve(core));
    }


    protected IEnumerator Dissolve(TowerCore core = null)
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
        cDissolveEffectState = Mathf.Clamp(cDissolveEffectState, endDisolveValue, startDissolveEffectState);
        dissolveMaterial.SetFloat("_Disolve_Active", cDissolveEffectState);

        onDissolveComplete.Invoke();

        if (core != null)
        {
            core.DissolveCompleted();
        }
    }
    protected IEnumerator RevertDissolve(TowerCore core = null)
    {
        yield return new WaitForSeconds(revertDelay);

        while (cDissolveEffectState < startDissolveEffectState)
        {
            yield return null;
            cDissolveEffectState += Time.deltaTime * revertDissolveSpeed;
            dissolveMaterial.SetFloat("_Disolve_Active", cDissolveEffectState);
        }
        cDissolveEffectState = Mathf.Clamp(cDissolveEffectState, endDisolveValue, startDissolveEffectState);
        dissolveMaterial.SetFloat("_Disolve_Active", cDissolveEffectState);

        onRevertDissolveComplete.Invoke();

        if (core != null)
        {
            core.RevertCompleted();
        }
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