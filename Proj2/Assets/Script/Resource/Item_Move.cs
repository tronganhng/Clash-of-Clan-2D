using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Proj2.clashofclan_2d;

public class Item_Move : StateMachineBehaviour
{
    Transform Des_point;
    public float speed = 2.4f;
    public int item_index, quantity;
    public string DesName;
    MainCamera mainCamera;
    Camera mycam;
    
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        mainCamera = FindObjectOfType<MainCamera>();
        mycam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        speed = speed * mainCamera.zoom;
        Des_point = GameObject.Find(DesName).transform;
    }

    
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Vector3 real_pos = mycam.ScreenToWorldPoint(new Vector3(Des_point.position.x, Des_point.position.y, mycam.nearClipPlane)); // chỉnh pos về camera
        Vector3 des_pos = new Vector3(real_pos.x, real_pos.y, 0);
        Vector2 newPos = Vector2.MoveTowards(animator.transform.position, des_pos, speed * Time.deltaTime); // cho item bay đến UI
        animator.transform.position = newPos;

        if(Vector2.Distance(animator.transform.position, des_pos) <= 0.4f) 
        {
            ResourceControll.instance.UpdateItemCnt(item_index, quantity);
            ResourceControll.instance.SetItemCount(item_index);
            Destroy(animator.gameObject);
        }
    }

}
