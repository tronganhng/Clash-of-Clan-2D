using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Proj2.clashofclan_2d;

public class UnitContentText : MonoBehaviour
{
    public BuildingDatabaseOS buildDataOS;
    public int unit_index;
    public Text gold_require, wood_require, limit;
    public Button button;
    string buildname;
    
    void Awake()
    {
        buildname = buildDataOS.buildingData[unit_index].Name;
        gold_require.text = "" + buildDataOS.buildingData[unit_index].gold_require;
        wood_require.text = "" + buildDataOS.buildingData[unit_index].wood_require;
    }

    private void OnEnable()
    {
        if (Buildings.instance.build_cnt.ContainsKey(buildname))
        {
            limit.text = Buildings.instance.build_cnt[buildname] + "/" + Buildings.instance.max_build[buildname];
            if (Buildings.instance.build_cnt[buildname] >= Buildings.instance.max_build[buildname])
            {
                button.interactable = false;
            }
            else button.interactable = true;
        }
        else
            limit.text = "0/" + Buildings.instance.max_build[buildname];


    }
}
