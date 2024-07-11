using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu] // tạo database ở engine
public class BuildingDatabaseOS : ScriptableObject
{
    public List<BuildingData> buildingData;
}

[Serializable]
public class BuildingData
{
    [field: SerializeField] // có thể edit trong engine
    public string Name{get; private set; }   // data ko thể chỉnh sửa
    [field: SerializeField]
    public int ID{get; private set; }
    [field: SerializeField]
    public Vector2Int Size{get; private set; } = Vector2Int.one;
    [field: SerializeField]
    public GameObject Prefab{get; private set; }
    [field: SerializeField]
    public int gold_require{get; private set; }
    [field: SerializeField]
    public int wood_require{get; private set; }
}
