using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIDissolveManager : MonoBehaviour
{
    public static UIDissolveManager Instance;
    private void Awake()
    {
        Instance = this;
    }


    private void Start()
    {
        UIDissolveGroup[] dissolves = FindObjectsOfType<UIDissolveGroup>(true);
        foreach (UIDissolveGroup dissolve in dissolves)
        {
            dissolve.Init();
        }
    }


    public UIDissolveGroup queuedDissolveGroup;

    public void QueDissolveGroup(UIDissolveGroup dissolveGroup)
    {
        if (queuedDissolveGroup == null)
        {
            dissolveGroup.CreateUI();
        }
        else
        {
            queuedDissolveGroup.DestroyUI();
        }

        queuedDissolveGroup = dissolveGroup;
    }


    public void LastActiveUIGroup_DissolveCompleted()
    {
        queuedDissolveGroup.CreateUI();

        queuedDissolveGroup = null;
    }
}
