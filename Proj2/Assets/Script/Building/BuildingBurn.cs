using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingBurn : MonoBehaviour
{
    public GameObject fire;
    public Health health;
    public float threshold = 0.4f;
    void Update()
    {
        if(health.currentHeal <= health.MaxHeal * threshold)
        {
            fire.SetActive(true);
        }      
    }
}
