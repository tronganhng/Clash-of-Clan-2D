using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

public class RandomMove : MonoBehaviour
{
    SpriteRenderer sprite;
    Seeker seeker;
    AIPath aIPath;
    Animator ani;
    [SerializeField] private GameObject random_point;
    AIDestinationSetter aides;
    public Health health;
    public Canvas canvas;
    void Start()
    {
        aides = GetComponent<AIDestinationSetter>();
        sprite = GetComponent<SpriteRenderer>();
        seeker = GetComponent<Seeker>();
        aIPath = GetComponent<AIPath>();
        ani = GetComponent<Animator>();
        aides.target = transform;
    }


    void Update()
    {
        Flip();
        UpdateState();
        if(health.in_hurt) // di chuyen khi bi attack
        {
            aIPath.enabled = true;
            if(Vector2.Distance(transform.position, aides.target.position) < 1f)
            {
                GetPoint(); // thay doi aides
            }
        }
        else aIPath.enabled = false;
    }

    void Flip() // flip luc move
    { 
        if(aIPath.desiredVelocity.x > 0.01f){
            transform.localRotation = Quaternion.Euler(0, 0, 0);
        }
        else if(aIPath.desiredVelocity.x < -0.01f) 
            transform.localRotation = Quaternion.Euler(0, 180, 0);
    }

    void UpdateState()
    {
        if(aIPath.velocity == new Vector3(0,0,0))
        {
            ani.SetBool("isrun", false);
        }
        else
        {
            ani.SetBool("isrun", true);
        }
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

    void GetPoint() // random aides
    {
        int random_index = Random.Range(0, random_point.transform.childCount);
        Debug.Log(random_index);
        Transform Target = random_point.transform.GetChild(random_index);
        aides.target = Target;
    }
}
