using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerAttack : MonoBehaviour
{
    public Transform scan_point, atk_point;
    public GameObject arrow;
    public LayerMask enemyLayer;
    public bool detect;
    public float atk_rate;
    public Vector2 ScanSize;
    Animator ani;
    private float AttackTime;
    private GameObject target;
    
    void Start()
    {
        ani = GetComponent<Animator>();
    }

    
    void Update()
    {
        if(target == null) 
            ScanEnemy();
        if(detect)
        {
            Rotation();
            SetAttack();
            LookAtEnemy();
        }
        else transform.localRotation = Quaternion.Euler(0,0,0);
    }

    void ScanEnemy()
    {
        Collider2D[] find_enemy = Physics2D.OverlapBoxAll(scan_point.position, ScanSize, 0f, enemyLayer);
        if(find_enemy.Length <= 0) {
            detect = false;
            target = null;
        }
        foreach (Collider2D enemy in find_enemy)
        {
            target = enemy.gameObject;
            detect = true;
        }
    }
    void SetAttack()
    {
        if(Time.time >= AttackTime){
            AttackTime = Time.time + atk_rate;
            ani.SetTrigger("atk");
        }       
    }
    void Rotation()
    {
            Vector3 targetpoint = new Vector3(target.transform.position.x, target.transform.position.y + 0.35f,0);
            Vector2 direct = targetpoint - transform.position;
            float angle = Mathf.Atan2(direct.y, Mathf.Abs(direct.x)) * Mathf.Rad2Deg;
            transform.localRotation = Quaternion.Euler(0,0,angle);           
    }
    void LookAtEnemy()
    {        
        if(transform.position.x < target.transform.position.x) transform.parent.rotation = Quaternion.Euler(0, 0,0);
        else transform.parent.rotation = Quaternion.Euler(0, 180,0);       
    }
    private void OnDrawGizmos() {
        Gizmos.DrawWireCube(scan_point.position, ScanSize);
    }
    void SpawnArrow()
    {
        GameObject newArrow = Instantiate(arrow, atk_point.position, transform.rotation);
        newArrow.GetComponent<ArrowMove>().dame = (int)GetComponentInParent<BuildingDefineData>().def_build.damage;
    }
}
