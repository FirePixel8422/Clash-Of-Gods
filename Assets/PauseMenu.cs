using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenu;


    public void OnPause(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            pauseMenu.SetActive(!pauseMenu.activeInHierarchy);
        }
    }
}
