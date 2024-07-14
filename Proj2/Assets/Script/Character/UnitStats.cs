using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Proj2.clashofclan_2d;

public class UnitStats : MonoBehaviour
{
    public Data.DefineUnit stat;
    void Start()
    {
        Health health_script = GetComponentInChildren<Health>();
        health_script.MaxHeal = stat.health;
        health_script.currentHeal = stat.health;
        health_script.healbar.SetMaxHealth(stat.health);
    }
}
