using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class TurnOffDetect : MonoBehaviour
{
    AIDestinationSetter aipoint;
    public float off_dis = 5f;
    void Start()
    {
        aipoint = GetComponent<AIDestinationSetter>();
    }

    void Update()
    {
        if(aipoint.target != null)
        {
            if(Vector2.Distance(transform.position, aipoint.target.position) > off_dis)
            {
                aipoint.target = null;
            }
        }
    }
}
