using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using Unity.VisualScripting;

public class SwitchAstarMap : MonoBehaviour
{
    AIPath aiPath;
    AIDestinationSetter ai_point;
    SpriteRenderer sprite;
    Seeker seeker;
    GameObject target;
    Transform stare_point;
    Stat stat;
    public Canvas canvas;
    //public int floor = -1;
    float endAIdis, slowAIdis;
    bool a = true;
    void Start()
    {
        stat = GetComponent<Stat>();
        aiPath = GetComponent<AIPath>();
        ai_point = GetComponent<AIDestinationSetter>();
        seeker = GetComponent<Seeker>();
        sprite = GetComponent<SpriteRenderer>();
        stare_point = GameObject.Find("stare point").transform;
        endAIdis = aiPath.endReachedDistance;
        slowAIdis = aiPath.slowdownDistance;
        SetLayer(stat.floor);
    }

    void Update() 
    {
        FindStare();
        OffStare();
    }

    void SetLayer(int floor)
    {
        if(floor == -1)
        {
            sprite.sortingLayerName = "Player0";
            canvas.sortingLayerName = "Effect0";
            seeker.graphMask = 2;
        }
        else
        {
            sprite.sortingLayerName = "Player";
            canvas.sortingLayerName = "Effect";
            seeker.graphMask = 1;
        }
    }

    void FindStare()
    {
        if(ai_point.target != null && ai_point.target != stare_point) // nếu tồn tại target khác stare point
        {
            if(ai_point.target.parent.GetComponent<Stat>().floor != stat.floor) // nếu target ở tầng khác
            {
                target = ai_point.target.gameObject; // lưu lại target
                aiPath.endReachedDistance = 1;
                aiPath.slowdownDistance = 3;
                ai_point.target = stare_point; // tìm đến bậc thang
            }
        }
    }

    void OffStare()
    {
        if(ai_point.target == stare_point)
        {
            if(target == null) 
            {
                ReturnAIStat();
                ai_point.target = null; // dừng khi target cũ chết
            }
            if(Vector2.Distance(transform.position, stare_point.position) < 2f && a) // đã đến bậc thang
            {
                a = false;
                stat.floor *= -1;
                SetLayer(stat.floor);
                ReturnAIStat();
                if(target != null) // target cũ còn sống
                {
                    a = true;
                    ai_point.target = target.transform;
                }
            }
        }
    }

    void ReturnAIStat()
    {
        aiPath.endReachedDistance = endAIdis;
        aiPath.slowdownDistance = slowAIdis;
    }
}
