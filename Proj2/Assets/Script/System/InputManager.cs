using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager : MonoBehaviour
{
    public Camera sceneCam;
    private Vector3 lastPosition;
    public LayerMask placementLayer;
    public event Action OnClicked, OnExit;

    private void Update() {
        if(Input.GetMouseButtonDown(0)) // kích hoạt sự kiện -> gọi các hàm đc gán vào sk
            OnClicked?.Invoke();
        if(Input.GetKeyDown(KeyCode.Escape))
            OnExit?.Invoke();
    }

    public bool IsMouseOverUI()
    {
        return EventSystem.current.IsPointerOverGameObject(); // ktra xem chuột đang trỏ đến đối tượng UI
    }

    public Vector3 GetMapPosition() // lấy ra Position chuột trên grass
    {
        Vector3 mousePos = sceneCam.ScreenToWorldPoint(Input.mousePosition);    
        Collider2D hitCollider = Physics2D.OverlapPoint(mousePos, placementLayer);
        if (hitCollider != null)
        {
            // Đối tượng có Layer "place" được nhấp vào
            lastPosition = mousePos;
        }
        return lastPosition;
    }
}
