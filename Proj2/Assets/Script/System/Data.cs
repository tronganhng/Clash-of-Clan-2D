using System.Xml.Serialization;
using System.IO;
using System.Collections.Generic;
using System;

namespace Proj2.clashofclan_2d
{
    public static class Data
    {
        public const int minGoldcollect = 10;

        public class Player // trường dữ liệu
        {
            public int gold = 0;
            public int wood = 0;
            public int meat = 0;
            public DateTime nowTime;
            public List<Building> buildings = new List<Building>();
            public List<DefineBuilding> Defbuildings = new List<DefineBuilding>();
            public TownHallDefine townHallDef = new TownHallDefine();
            public List<Unit> units = new List<Unit>();
            public List<DefineUnit> Defunits = new List<DefineUnit>();
        }

        [System.Serializable]
        public class Unit
        {
            public int id = 0;
            public long account_id = 0;
            public string name = "";
            public int level = 1;
            public int training = 0;
            public int ready = 0;
            public DateTime trained_time;
            public bool is_training;
        }

        public class DefineUnit
        {
            public string name = "";
            public int level = 1;
            public int train_time = 0;
            public int req_gold = 0;
            public int req_wood = 0;
            public int req_Barracklv = 1;
        }

        [System.Serializable]
        public class Building
        {
            public int id = 0;
            public string buildingName = "";
            public long account_id = 0;
            public int level = 0;
            public int pos_x = 0;
            public int pos_y = 0;
            public float storage = 0;
            public DateTime construct_time;
            public bool is_constructing = false;
        }

        // định nghĩa building
        [System.Serializable]
        public class DefineBuilding
        {
            public string buildingName = "";
            public int level = 0;
            public int req_gold = 0;
            public int req_wood = 0;
            public int req_THlv = 1;
            public int health = 200;
            public int build_time = 0;
            public float damage = 0;
            public int speed = 0;
            public int capacity = 0;
        }

        [System.Serializable]
        public class TownHallDefine
        {
            public int level = 1;
            public int max_resource = 0;
            public int max_mining = 0;
            public int max_barrack = 0;
            public int max_tower = 0;
        }

        public static string Serialize<T>(this T target)
        {
            // T: trường dữ liệu
            XmlSerializer xml = new XmlSerializer(typeof(T));
            StringWriter writer = new StringWriter();
            xml.Serialize(writer, target); // ghi trường dữ liệu của target vào writer
            return writer.ToString();
        }

        public static T Deserialize<T>(this string target)
        {
            XmlSerializer xml = new XmlSerializer(typeof(T));
            StringReader reader = new StringReader(target);
            return (T)xml.Deserialize(reader); // đổi dữ liệu từ string -> trường dữ liệu
        }
    }
}
