using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerPreview : MonoBehaviour
{
    public TowerCore towerPrefab;

    public int cost;

    [HideInInspector]
    public XrayController[] xrayControllers;

    [HideInInspector]
    public SpriteRenderer towerPreviewRenderer;
    private bool cPlaceable;


    private void Start()
    {
        towerPreviewRenderer = GetComponentInChildren<SpriteRenderer>();

        xrayControllers = GetComponentsInChildren<XrayController>();
    }

    public void UpdateTowerPreviewColor(bool placeable)
    {
        if (placeable == cPlaceable)
        {
            return;
        }
        cPlaceable = placeable;
        foreach (var d in xrayControllers)
        {
            d.dissolveMaterial.SetInt("_Placable", placeable ? 1 : 0);
        }
    }
}
