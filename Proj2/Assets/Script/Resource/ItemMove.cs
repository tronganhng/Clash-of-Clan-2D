using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DevelopersHub.RealtimeNetworking.Client;

public class ItemMove : MonoBehaviour
{
    Animator ani;
    Transform Des_point;
    public float speed = 2.4f;
    public int item_index, quantity;
    public string DesName;
    MainCamera mainCamera;
    Camera mycam;
    void Start()
    {
        ani = GetComponent<Animator>();
        mainCamera = FindObjectOfType<MainCamera>();
        mycam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        speed = speed * mainCamera.zoom;
        Des_point = GameObject.Find(DesName).transform;
    }

    void Update()
    {
        if(ani.GetBool("idle"))
        {
            Vector3 real_pos = mycam.ScreenToWorldPoint(new Vector3(Des_point.position.x, Des_point.position.y, mycam.nearClipPlane)); // chỉnh pos về camera
            Vector3 des_pos = new Vector3(real_pos.x, real_pos.y, 0);
            Vector2 newPos = Vector2.MoveTowards(transform.position, des_pos, speed * Time.deltaTime); // cho item bay đến UI
            transform.position = newPos;

            if (Vector2.Distance(transform.position, des_pos) <= 0.4f)
            {
                Packet packet = new Packet();
                packet.Write(10);
                if (item_index == 0) packet.Write("gold");
                else if (item_index == 1) packet.Write("wood");
                packet.Write(quantity);
                Sender.TCP_Send(packet);
                Destroy(gameObject);
            }
        }
    }

    public void SetIdle()
    {
        ani.SetBool("idle", true);
    }
}
