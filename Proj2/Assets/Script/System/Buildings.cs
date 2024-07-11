using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Scripts ch?a thông tin các building c?a player
namespace Proj2.clashofclan_2d
{
    public class Buildings : MonoBehaviour
    {        

        public Dictionary<int, GameObject> build_prefab;
        public Dictionary<string, int> build_cnt;  // lưu tên-số lượng các building
        public Dictionary<string, int> max_build;  // tên-số lượng build max
        public static Buildings _instance = null;
        public static Buildings instance { get { return _instance; } }
        void Start()
        {
            build_prefab = new();
            build_cnt = new();
            max_build = new();
            _instance = this;
        }
    }
}
