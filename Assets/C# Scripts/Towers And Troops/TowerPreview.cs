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
    private Color cColor;


    private void Start()
    {
        towerPreviewRenderer = GetComponentInChildren<SpriteRenderer>();

        xrayControllers = GetComponentsInChildren<XrayController>();
    }

    public void UpdateTowerPreviewColor(Color color)
    {
        if (color == cColor)
        {
            return;
        }
        cColor = color;
        foreach (var d in xrayControllers)
        {
            d.dissolveMaterial.SetColor("_PreviewColor", color);
        }
    }
}
