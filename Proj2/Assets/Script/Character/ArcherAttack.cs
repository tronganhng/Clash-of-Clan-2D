using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

public class ArcherAttack : MonoBehaviour
{
    public AIDestinationSetter ai_point;
    Animator ani, parent_ani;
    SpriteRenderer Sprite, parent_sprite;
    public GameObject arrow;
    public Transform atk_point;
    public Health health;
    public float atk_range, atk_rate;
    private float AttackTime;
    
    void Start()
    {
        Sprite = GetComponent<SpriteRenderer>();
        parent_sprite = transform.parent.GetComponent<SpriteRenderer>();
        parent_ani = transform.parent.GetComponent<Animator>();
        ani = GetComponent<Animator>();
    }

    
    void Update()
    {
        Sprite.sortingLayerName = parent_sprite.sortingLayerName; 

        if(ai_point.target != null) // enemy found => rotate, attack
        {
            Rotation();
            SetAttack();
        }
        else transform.localRotation = Quaternion.Euler(0,0,0); // reset rotation

        if(health.currentHeal <= 0)
        {
            Destroy(gameObject);
        }
    }

    void Rotation(){
        if(parent_ani.GetInteger("State") == 0)
        {
            Vector3 target = new Vector3(ai_point.target.position.x, ai_point.target.position.y + 0.35f,0);
            Vector2 direct = target - transform.position;
            float angle = Mathf.Atan2(direct.y, Mathf.Abs(direct.x)) * Mathf.Rad2Deg;
            transform.localRotation = Quaternion.Euler(0,0,angle);
        }
    }

    void SetAttack(){
        if(Vector2.Distance(transform.position, ai_point.target.position) <= atk_range)
        {
            if(Time.time >= AttackTime && parent_ani.GetInteger("State") == 0){
                AttackTime = Time.time + Random.Range(atk_rate - 0.3f, atk_rate + 0.4f);
                ani.SetTrigger("atk");
            }
        }
        else transform.localRotation = Quaternion.Euler(0,0,0);
    }

    void SpawnArrow(){
        Instantiate(arrow, atk_point.position, transform.rotation);
    }
}
