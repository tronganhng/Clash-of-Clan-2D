using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Proj2.clashofclan_2d;

public class EnemyResource : MonoBehaviour
{
    public Text gold_txt, wood_txt;
    public int max_gold = 0, max_wood = 0;
    public int gold = 0, wood = 0;

    public static EnemyResource instance = null;
    private void Start()
    {
        if(instance == null)
            instance = this;
    }
    public void SetAllItemCount()
    {
        gold_txt.text = gold.ToString();
        wood_txt.text = wood.ToString();
    }

    public void GetResource()
    {
        gold = 0; wood = 0;
        foreach(KeyValuePair<int, GameObject> kvp in Buildings.instance.build_prefab)
        {
            if(kvp.Value.GetComponent<BuildingDefineData>().building.buildingName == "goldmine")
            {
                gold += (int)kvp.Value.GetComponent<BuildingDefineData>().building.storage;
            }
            else if(kvp.Value.GetComponent<BuildingDefineData>().building.buildingName == "woodmine")
            {
                wood += (int)kvp.Value.GetComponent<BuildingDefineData>().building.storage;
            }
        }
        max_gold = gold;
        max_wood = wood;
        SetAllItemCount();
    }
}
