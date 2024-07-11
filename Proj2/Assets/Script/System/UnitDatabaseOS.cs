using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu]
public class UnitDatabaseOS : ScriptableObject
{
    public List<UnitData> unitData;
}

[Serializable]
public class UnitData
{
    [field: SerializeField] // có thể edit trong engine
    public string Name{get; private set; }   // data ko thể chỉnh sửa
    [field: SerializeField]
    public int ID{get; private set; }
    [field: SerializeField]
    public int Meat_Cost;
    [field: SerializeField]
    public int MaxHealth;
    [field: SerializeField]
    public int Atk_Dame;
}
