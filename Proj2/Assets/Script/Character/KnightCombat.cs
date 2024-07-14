using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;
using Proj2.clashofclan_2d;

public class KnightCombat : MonoBehaviour
{
    Animator ani;
    AIDestinationSetter ai_point;
    public UnitDatabaseOS unitDataOS;
    public Transform atk_point, atkup_point, atkdown_point;
    public LayerMask enemyLayer;
    public float atk_range ,atk_rate;
    private float AttackTime;
    Data.DefineUnit stat;
    private enum Direction {side, up, down}  //tao ra data type
    void Start()
    {
        ani = GetComponent<Animator>();
        ai_point = GetComponent<AIDestinationSetter>();
        AttackTime = Time.time + atk_rate;
        stat = GetComponent<UnitStats>().stat;
    }

    
    void Update()
    {
        if(ai_point.target != null){
            if(Vector2.Distance(transform.position, ai_point.target.position) <= 1.4f){
                if(Time.time >= AttackTime){
                    AttackTime = Time.time + Random.Range(atk_rate - .5f, atk_rate + .4f);
                    int random = Random.Range(1,11);
                    if(random <= 7) ani.SetTrigger("atk");
                    else ani.SetTrigger("atk2");
                }
            }
        }

        if(ai_point.target != null) UpdateDirect();
    }

    void UpdateDirect()
    {
        Direction direct;
        if(ai_point.target.position.y > transform.position.y + 0.86f){            
            direct = Direction.up;
        }
        else if(ai_point.target.position.y < transform.position.y - 0.86f){            
            direct = Direction.down;
        }
        else{
            direct = Direction.side;
        }
        ani.SetInteger("direct", (int)direct);
    }

    void Atk()
    {
        Collider2D[] hit_enemy = Physics2D.OverlapCircleAll(atk_point.position, atk_range, enemyLayer);
        foreach (Collider2D enemy in hit_enemy)
        {
            ai_point.target.GetComponentInChildren<Health>().TakeDame(stat.damage); // chỉ gây dame lên target
        }
    }

    void AtkUp()
    {
        Collider2D[] hit_enemy = Physics2D.OverlapCircleAll(atkup_point.position, atk_range, enemyLayer);
        foreach (Collider2D enemy in hit_enemy)
        {
            ai_point.target.GetComponentInChildren<Health>().TakeDame(stat.damage);
        }
    }

    void AtkDown()
    {
        Collider2D[] hit_enemy = Physics2D.OverlapCircleAll(atkdown_point.position, atk_range, enemyLayer);
        foreach (Collider2D enemy in hit_enemy)
        {
            ai_point.target.GetComponentInChildren<Health>().TakeDame(stat.damage);
        }
    }

    // private void OnDrawGizmos() {
    //     Gizmos.DrawWireSphere(atk_point.position, atk_range);
    // }
}
