using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIDissolveGroup : MonoBehaviour
{
    private UIDissolveController[] dissolvesControllers;
    public int dissolvesCount;

    public bool active;


    public void Init()
    {
        dissolvesControllers = GetComponentsInChildren<UIDissolveController>(true);
        dissolvesCount = dissolvesControllers.Length;

        foreach (var controller in dissolvesControllers)
        {
            controller.Init(this);
        }
    }


    public void CreateUI()
    {
        if (active == false)
        {
            revertDissolvesCompleted = 0;

            foreach (var dissolve in dissolvesControllers)
            {
                dissolve.StartUIDissolve();
            }
        }

        active = true;
    }
    public void DestroyUI()
    {
        if (active == true)
        {
            print("destroying");
            dissolvesCompleted = 0;

            foreach (var dissolve in dissolvesControllers)
            {
                dissolve.RevertUIDissolve();
            }
        }
        
        active = false;
    }


    public int dissolvesCompleted;

    public void ChildDestroyedUI()
    {
        dissolvesCompleted += 1;
        print(dissolvesCompleted);

        if (dissolvesCompleted == dissolvesCount)
        {
            UIDissolveManager.Instance.LastActiveUIGroup_DissolveCompleted();
            gameObject.SetActive(false);
        }
    }


    public int revertDissolvesCompleted;

    public void ChildCreatedUI()
    {
        revertDissolvesCompleted += 1;

        if (dissolvesCompleted == dissolvesCount)
        {
            //no use yet
            return;
        }
    }
}
