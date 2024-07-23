using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Proj2.clashofclan_2d;
using UnityEngine.UI;

public class ArmyInfoUI : MonoBehaviour
{
    public UnitDatabaseOS unitDataOS;
    public GameObject Content, unitPrefab;

    private void OnEnable()
    {
        foreach(KeyValuePair<string, Data.Unit> kvp in Units.instance.units)
        {
            if (kvp.Value.ready <= 0) return;
            int unit_index = unitDataOS.unitData.FindIndex(data => data.Name == kvp.Key);
            GameObject prefab = Instantiate(unitPrefab);
            prefab.GetComponent<Image>().sprite = unitDataOS.unitData[unit_index].UiAvatar;
            prefab.GetComponentInChildren<Text>().text = "x" + kvp.Value.ready;
            prefab.transform.SetParent(Content.transform);
            prefab.transform.localScale = new Vector3(1, 1, 1);
        }
    }

    private void OnDisable()
    {
        foreach(Transform child in Content.transform)
        {
            Destroy(child.gameObject);
        }    
    }
}
