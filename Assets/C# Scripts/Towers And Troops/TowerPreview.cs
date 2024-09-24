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
    private int cPlaceable = 2;


    private void Start()
    {
        towerPreviewRenderer = GetComponentInChildren<SpriteRenderer>();

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
    }
}
