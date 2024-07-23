using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class GoblinBarrelState : MonoBehaviour
{
    Animator ani;
    AIDestinationSetter aipoint;
    public Transform atk_point;
    public LayerMask enemyLayer;
    public float atk_range, atk_dame;
    
    void Start()
    {
        ani = GetComponent<Animator>();
        aipoint = GetComponent<AIDestinationSetter>();
        atk_dame = GetComponentInParent<BuildingDefineData>().def_build.damage;
    }

    void Update()
    {
        if(aipoint.target == null) ani.SetBool("Detect", false);
        else ani.SetBool("Detect", true);

        if(ani.GetBool("Detect") && ani.GetInteger("State") == 0) ani.SetTrigger("explode");
    }

    void Explode()
    {
        Collider2D[] hit_enemy = Physics2D.OverlapCircleAll(atk_point.position, atk_range, enemyLayer);
        foreach (Collider2D enemy in hit_enemy)
        {
            enemy.GetComponentInChildren<Health>().TakeDame(atk_dame);
        }
    }

    // private void OnDrawGizmos() {
    //     Gizmos.DrawWireSphere(atk_point.position, atk_range);
    // }

    void DestroyBarrel()
    {
        Destroy(transform.parent.gameObject);
    }
}
