using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TEMP_ResolutionFixer : MonoBehaviour
{

    private void Start()
    {
        if (Screen.currentResolution.width == 2560 && Screen.currentResolution.height == 1600)
        {
            Screen.SetResolution(2560, 1440, true);
        }
        if (Screen.currentResolution.width == 1920 && Screen.currentResolution.height == 1200)
        {
            Screen.SetResolution(1920, 1080, true);
        }
        if (Screen.currentResolution.width == 1680 && Screen.currentResolution.height == 1050)
        {
            Screen.SetResolution(1680, 945, true);
        }
        if (Screen.currentResolution.width == 1440 && Screen.currentResolution.height == 900)
        {
            Screen.SetResolution(1440, 810, true);
        }
        if (Screen.currentResolution.width == 1280 && Screen.currentResolution.height == 800)
        {
            Screen.SetResolution(1280, 720, true);
        }
    }
}
