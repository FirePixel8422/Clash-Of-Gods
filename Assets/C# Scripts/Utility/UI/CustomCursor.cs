using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class CustomCursor : MonoBehaviour
{
    public Texture2D cursorTexture;
    public Vector2 offset;
    public CursorMode mode;

    private void Start()
    {
        Cursor.SetCursor(cursorTexture, offset, mode);
    }



    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {

        }

        if (Input.GetMouseButtonUp(0))
        {

        }
    }
}
