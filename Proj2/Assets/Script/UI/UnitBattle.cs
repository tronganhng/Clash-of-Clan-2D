using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Proj2.clashofclan_2d;
using UnityEngine.UI;
public class UnitBattle : MonoBehaviour
{
    public UnitDatabaseOS unitDataOS;
    public GameObject Content, unit_prefab;
    public Sprite[] unit_sprite;
    public int char_select = -1;
    public static UnitBattle instance;
    void Start()
    {
        instance = this;
    }

    public void SetupUnitBattle()
    {
        foreach(KeyValuePair<string, Data.Unit> kvp in Units.instance.units)
        {
            if(kvp.Value.ready > 0)
            {
                int index = unitDataOS.unitData.FindIndex(data => data.Name == kvp.Key);
                GameObject newUnit = Instantiate(unit_prefab);
                newUnit.transform.SetParent(Content.transform);
                newUnit.transform.localScale = new Vector3(1, 1, 1);
                newUnit.GetComponentInChildren<Text>().text = "x" + kvp.Value.ready;
                newUnit.GetComponent<Image>().sprite = unit_sprite[index];
                newUnit.GetComponentInChildren<Button>().GetComponent<Image>().sprite = unit_sprite[index];
                newUnit.GetComponentInChildren<Button>().onClick.AddListener(() => UnitSelecting(index));
            }
        }
    }

    public void UnitSelecting(int index)
    {
        char_select = index;
    }
}
