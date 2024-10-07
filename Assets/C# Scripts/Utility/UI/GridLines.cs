using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridLines : MonoBehaviour
{
    public static GridLines Instance;
    private void Awake()
    {
        Instance = this;
    }


    public Renderer[] gridRenderers;
    public Color[] gridColors;


    public void SetColor(int id, int colorId)
    {
        gridRenderers[id].material.SetColor(Shader.PropertyToID("_Color"), gridColors[colorId]);
    }
}
