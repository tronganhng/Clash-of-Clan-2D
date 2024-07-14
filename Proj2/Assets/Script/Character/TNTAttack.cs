using Proj2.clashofclan_2d;
using UnityEngine;
using Pathfinding;

public class TNTAttack : MonoBehaviour
{
    public Transform atk_point;
    public GameObject TNT;
    public float atk_rate;
    private float attackTime;
    AIDestinationSetter ai_point;
    Animator ani;
    Data.DefineUnit stat;

    void Start()
    {
        ani = GetComponent<Animator>();
        ai_point = GetComponent<AIDestinationSetter>();
        stat = GetComponent<UnitStats>().stat;
    }

    
    void Update()
    {
        if(ani.GetInteger("State") == 0 && ai_point.target != null)
        {
            attackTime += Time.deltaTime;
            if(attackTime >= atk_rate)
            {
                ani.SetTrigger("atk");
                attackTime = Random.Range(-0.3f,0.4f);
            }    
        }    
    }

    void SpawnTNT()
    {
        GameObject clone_TNT = Instantiate(TNT, atk_point.position, transform.rotation);
        clone_TNT.GetComponent<TNT>().target = ai_point.target;
        clone_TNT.GetComponent<TNT>().dame = stat.damage;
    }
}
