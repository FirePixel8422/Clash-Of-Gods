using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridTile : MonoBehaviour
{
    public Material mat;


    [ColorUsage(true, true)]
    public Color[] onFireColors;

    public float colorSwapSpeed;

    public int fireAmount;


    private void Start()
    {
        mat = GetComponent<Renderer>().material;

        GridManager.Instance.UpdateGridDataTile(GridManager.Instance.GridObjectFromWorldPoint(transform.position).gridPos, this);
    }

    public void SetOnFire(int amount)
    {
        fireAmount += amount;

        


    }
    [ColorUsage(true, true)]
    private Color color;

    private IEnumerator ChangeColor()
    {
        while (true)
        {
            yield return null;

            color = Color.Lerp(color, onFireColors[Mathf.Clamp(fireAmount, 0, onFireColors.Length)], colorSwapSpeed * Time.deltaTime);

            mat.SetColor("_Emission_Color", color);
        }
    }
}
 