using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using Unity.VisualScripting;
using UnityEngine;

public class Movement : MonoBehaviour
{
    public AIPath aipath;
    AIDestinationSetter aipoint;
    Animator ani;
    SpriteRenderer sprite;
    Rigidbody2D rb;
    public Transform scan_point;
    public LayerMask enemyLayer;
    public Vector2 Boxsize;
    public bool detect;
    private enum MovementState {idle, run}  //tao ra data type
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        ani = GetComponent<Animator>(); 
        aipoint = aipath.GetComponent<AIDestinationSetter>();
    }

    
    void Update()
    {
        Flip();
        UpdateState();
        if(aipoint.target != null)
        {
            LookAtEnemy();
        }

        if(aipoint.target == null) // ko co target => quet tim enemy
        {
            ScanEnemy();
        }
    }

    private void OnDrawGizmos() {
        Gizmos.DrawWireCube(scan_point.position, Boxsize);
    }

    void UpdateState()
    {
        MovementState state;
        if(aipath.desiredVelocity.sqrMagnitude == 0){
            state = MovementState.idle;
        }
        else{
            state = MovementState.run;
        } 
        ani.SetInteger("State", (int)state);
    }

    void Flip() // flip luc move
    { 
        if(aipath.desiredVelocity.x > 0.01f){
            transform.localRotation = Quaternion.Euler(0, 0, 0);
        }
        else if(aipath.desiredVelocity.x < -0.01f) 
            transform.localRotation = Quaternion.Euler(0, 180, 0);
    }

    void LookAtEnemy()
    {
        if(ani.GetInteger("State") == 0){ // in atk thi lookatenemy
            if(transform.position.x < aipoint.target.position.x) transform.rotation = Quaternion.Euler(0, 0,0);
            else transform.rotation = Quaternion.Euler(0, 180,0);
        }
    }

    void ScanEnemy()
    {
        Collider2D[] find_enemy = Physics2D.OverlapBoxAll(scan_point.position, Boxsize, 0f, enemyLayer);
        if(find_enemy.Length <= 0) // not found
        { 
            detect = false;
            aipath.enabled = false;
        }
        float closesDis = Mathf.Infinity;
        GameObject closesObj = null;
        foreach (Collider2D enemy in find_enemy) // found
        {
                float dis = Vector2.Distance(transform.position, enemy.transform.position);
                if(dis < closesDis) // tim ra enemy gan nhat
                {
                    closesDis= dis;
                    closesObj = enemy.gameObject;
                    aipoint.target = enemy.transform;
                }
                if(aipoint.target != null)
                {
                    detect = true;
                    aipath.enabled = true;
                }            
        }
    }
}
