using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerPreview : MonoBehaviour
{
    public TowerCore towerPrefab;

    public int cost;

    [HideInInspector]
    public XrayController[] xrayControllers;

    
    private SpriteRenderer[] ranges;


    private int cPlaceable = 2;


    private void Start()
    {
        ranges = GetComponentsInChildren<SpriteRenderer>();

        xrayControllers = GetComponentsInChildren<XrayController>();
    }

    public void UpdateTowerPreviewColor(bool placeable)
    {
        if ((placeable ? 1 : 0) == cPlaceable)
        {
            return;
        }
        cPlaceable = placeable ? 1 : 0;
        foreach (var d in xrayControllers)
        {
            d.dissolveMaterial.SetInt(Shader.PropertyToID("_Placable"), placeable ? 1 : 0);
        }

        foreach (var range in ranges)
        {
            range.color = placeable ? new Color(0.03529412f, 1f, 0f) : new Color(0.8943396f, 0.2309691f, 0.09955848f);
        }
    }
}
