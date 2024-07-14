using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowMove : MonoBehaviour
{
    Animator ani;
    Rigidbody2D rb;
    public string enemyLayer;
    public float speed;
    public int dame;
    void Start()
    {
        ani = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        rb.velocity = transform.right * speed;
        Invoke("DestroyArrow", 2f);
    }

    private void OnTriggerEnter2D(Collider2D other) 
    {
        if(other.gameObject.layer == LayerMask.NameToLayer(enemyLayer)){
            ani.SetTrigger("hit");
            rb.velocity = new Vector2(0,0);
            if(other.GetComponent<Health>() != null) other.GetComponent<Health>().TakeDame(dame);
        }
    }

    void DestroyArrow()
    {
        Destroy(gameObject);
    }
}
