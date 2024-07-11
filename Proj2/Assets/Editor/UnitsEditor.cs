using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Proj2.clashofclan_2d;

[CustomEditor(typeof(Units))]
public class UnitsEditor : Editor
{
    Dictionary<string, Data.Unit> dictionary;
    Dictionary<string, Data.DefineUnit> dictionary2;
    bool UnitGroup = false;
    bool UnitGroup2 = false;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        dictionary = new();
        dictionary2 = new();
        Units script = (Units)target;
        dictionary = script.units;
        dictionary2 = script.def_unit;
        if (dictionary != null)
        {
            // nhóm các unit
            UnitGroup = EditorGUILayout.BeginFoldoutHeaderGroup(UnitGroup, "Units");
            if (UnitGroup)
            {
                foreach (KeyValuePair<string, Data.Unit> kvp in dictionary)
                {
                    // hiển thị các chỉ số của mỗi unit


                    EditorGUILayout.LabelField("Name: " + kvp.Key);
                    EditorGUILayout.IntField("Level", kvp.Value.level);
                    EditorGUILayout.IntField("Training", kvp.Value.training);
                    EditorGUILayout.IntField("Ready", kvp.Value.ready);
                    EditorGUILayout.TextField("Trained Time", kvp.Value.trained_time.ToString());
                    EditorGUILayout.Toggle("Is Training", kvp.Value.is_training);
                    EditorGUILayout.Space();
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        if (dictionary2 != null)
        {
            // nhóm các unit
            UnitGroup2 = EditorGUILayout.BeginFoldoutHeaderGroup(UnitGroup2, "Define Unit");
            if (UnitGroup2)
            {
                foreach (KeyValuePair<string, Data.DefineUnit> kvp in dictionary2)
                {
                    // hiển thị các chỉ số của mỗi unit


                    EditorGUILayout.LabelField("Name: " + kvp.Key);
                    EditorGUILayout.IntField("Level", kvp.Value.level);
                    EditorGUILayout.IntField("Train Time", kvp.Value.train_time);
                    EditorGUILayout.IntField("Req Gold", kvp.Value.req_gold);
                    EditorGUILayout.IntField("Req Wood", kvp.Value.req_wood);
                    EditorGUILayout.IntField("Req Barrack level", kvp.Value.req_Barracklv);
                    EditorGUILayout.Space();
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }
    }
}
