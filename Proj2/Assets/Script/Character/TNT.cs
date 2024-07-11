using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TNT : MonoBehaviour
{
    public Transform target;
    public string enemyLayer;
    public float rotation_speed, dame, const_y;
    bool ishit;
    Rigidbody2D rb;
    Animator ani;
    Vector3 des_pos;
    void Start()
    {
        des_pos = new Vector3(target.position.x, target.position.y - 0.35f, 0);
        rb = GetComponent<Rigidbody2D>();
        ani = GetComponent<Animator>();
        float angle = SetupAngle();
        SetupSpeed(angle * Mathf.Deg2Rad);
        Debug.Log(angle);
    }

    private void Update()
    {

        if(Vector2.Distance(transform.position, des_pos) <= .2f) // no khi cham dat (ko trung target)
        {
            Destroy(gameObject);
        }
    }

    private void FixedUpdate()
    {
        // Quay đối tượng theo góc rotationSpeed mỗi frame
        if(rb.bodyType == RigidbodyType2D.Dynamic)
            rb.MoveRotation(rb.rotation + rotation_speed * Time.fixedDeltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer(enemyLayer) && !ishit)
        {
            ishit = true;
            ani.SetTrigger("hit");
            rb.bodyType = RigidbodyType2D.Static;
            if (other.GetComponent<Health>() != null) other.GetComponent<Health>().TakeDame(dame);
            Destroy(gameObject, 1f);
        }
    }

    float SetupAngle()
    {
        Vector3 clone_target = new Vector3(target.position.x, target.position.y - 0.35f, 0);
        Vector2 d = clone_target - transform.position;
        Vector2 direct = new Vector2(d.x,d.y + const_y);
        float angle = Mathf.Atan2(direct.y, direct.x) * Mathf.Rad2Deg;
        transform.localRotation = Quaternion.Euler(0, 0, angle);
        return angle;
    }

    void SetupSpeed(float angle)
    {
        Vector3 clone_target = new Vector3(target.position.x, target.position.y - 0.35f, 0);
        float xo = target.position.x - transform.position.x;
        float yo = target.position.y - transform.position.y;
        float v = xo / (Mathf.Cos(angle)*Mathf.Sqrt((-yo+xo*Mathf.Tan(angle))/4.9f));
        rb.velocity = transform.right * v;

    }

}
