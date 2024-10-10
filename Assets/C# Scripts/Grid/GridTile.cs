using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridTile : MonoBehaviour
{
    public Material mat;


    [ColorUsage(true, true)]
    public Color[] onFireColors;

    public float colorSwapTime;

    public int fireAmount;

    private Coroutine changeColorCO;


    private void Start()
    {
        mat = GetComponent<Renderer>().material;

        GridManager.Instance.UpdateGridDataTile(GridManager.Instance.GridObjectFromWorldPoint(transform.position).gridPos, this);
    }

    public void SetOnFire(int amount)
    {
        fireAmount += amount;


        if (changeColorCO != null)
        {
            StopCoroutine(changeColorCO);
        }

        changeColorCO = StartCoroutine(ChangeColor(onFireColors[Mathf.Clamp(fireAmount, 0, onFireColors.Length - 1)]));
    }



    [ColorUsage(true, true)]
    private Color color;

    private IEnumerator ChangeColor(Color targetColor)
    {
        float elapsedTime = 0;

        while (color != targetColor)
        {
            yield return null;

            elapsedTime += Time.deltaTime;

            // Calculate the interpolation factor (clamped to the range [0, 1])
            float t = Mathf.Clamp01(elapsedTime / colorSwapTime);


            color = Color.Lerp(color, targetColor, t * Time.deltaTime);

            mat.SetColor("_Emission_Color", color);
        }
    }
}
 