using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

public class PawnChop : MonoBehaviour
{
    Animator ani;
    Movement move;
    AIDestinationSetter ai_point;
    public Transform atk_point;
    public LayerMask targetLayer;
    public float chop_range ,chop_rate, chop_dame;
    private float AttackTime;
    void Start()
    {
        ani = GetComponent<Animator>();
        move = GetComponent<Movement>();   
        ai_point = move.aipath.GetComponent<AIDestinationSetter>();
    }


    void Update()
    {
        SetChop();
    }

    void SetChop()
    {
        if(ai_point.target != null){
            if(Vector2.Distance(transform.position, ai_point.target.position) <= 1.4f){
                if(Time.time >= AttackTime){
                    AttackTime = Time.time + Random.Range(chop_rate - .5f, chop_rate + .4f);
                    int random = Random.Range(1,11);
                    if(random <= 7) ani.SetTrigger("atk");
                    else ani.SetTrigger("atk2");
                }
            }
        }
    }

    void Chop()
    {
        Collider2D[] hit_tree = Physics2D.OverlapCircleAll(atk_point.position, chop_range, targetLayer);
        foreach (Collider2D tree in hit_tree)
        {
            tree.GetComponent<Health>().TakeDame(chop_dame);
        }
    }

    // private void OnDrawGizmos() {
    //     Gizmos.DrawWireSphere(atk_point.position, chop_range);
    // }
}
