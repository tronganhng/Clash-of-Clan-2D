using UnityEngine;
using UnityEngine.UI;
using Proj2.clashofclan_2d;
using System;

public class BuildingDefineData : MonoBehaviour
{
    public Data.Building building = new();
    public Data.DefineBuilding def_build = new();

    public Text level_txt;
    //  Setup stats

    private void Start()
    {
        level_txt.text = def_build.buildingName + "\nlevel " + def_build.level;
        GetComponentInChildren<Health>().MaxHeal = def_build.health;
        GetComponentInChildren<Health>().currentHeal = def_build.health;
        GetComponentInChildren<Health>().healbar.SetMaxHealth(def_build.health);
    }    
}
