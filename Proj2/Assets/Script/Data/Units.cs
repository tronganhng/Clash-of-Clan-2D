using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Proj2.clashofclan_2d
{
    public class Units : MonoBehaviour
    {
        public Dictionary<string, Data.Unit> units;
        public Dictionary<string, Data.DefineUnit> def_unit;

        public static Units _instance = null;
        public static Units instance { get { return _instance; } }
        void Awake()
        {            
            _instance = this;
            units = new();
            def_unit = new();
        }
    }
}
