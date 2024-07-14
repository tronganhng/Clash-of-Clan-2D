using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Assertions.Must;

public class MainCamera : MonoBehaviour
{
    float zoomMultiple = 4f, smoothTime, velocity;
    public float zoom, minZoom=2f, maxZoom=8f;
    public float Senvisity = 0.1f;
    public Camera cam;
    void Start()
    {
        zoom = cam.orthographicSize;
    }

    void Update()
    {
        if(!EventSystem.current.IsPointerOverGameObject())
        {
            Zoom();
            Move();
        }
    }

    void Zoom()
    {
        float scrollInput = Input.GetAxis("Mouse ScrollWheel"); // (-1; 1)
        zoom -= scrollInput * zoomMultiple;
        zoom = Mathf.Clamp(zoom, minZoom, maxZoom); // giới hạn zoom từ (minZoom; maxZoom)
        cam.orthographicSize = Mathf.SmoothDamp(cam.orthographicSize, zoom, ref velocity, smoothTime); // scale cam từ gtri hiện tại -> zoom
    }

    void Move()
    {
        if(Input.GetMouseButton(1))
        {
            float mouseX = Input.GetAxis("Mouse X") * Senvisity;
            float mouseY = Input.GetAxis("Mouse Y") * Senvisity;

            transform.position -= mouseX * transform.right;
            transform.position -= mouseY * transform.up;
        }
    }
}
