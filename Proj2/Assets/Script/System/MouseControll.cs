using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseControll : MonoBehaviour
{
    public Texture2D cursorTexture;
    public CursorMode cursormode = CursorMode.Auto;
    Vector2 hotSpot = Vector2.zero;
    void Start()
    {
        Cursor.SetCursor(cursorTexture, hotSpot, cursormode);
    }
}
