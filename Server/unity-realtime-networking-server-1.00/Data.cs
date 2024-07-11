using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.IO;

namespace DevelopersHub.RealtimeNetworking.Server
{
    public static class Data
    {
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
        public class DefineBuilding
        {
            public string buildingName = "";
            public int level = 0;
            public int req_gold = 0;
            public int req_wood = 0;
            public int req_THlv = 1;
            public int health = 200;
            public float damage = 0;
            public int speed = 0;
            public int capacity = 0;
            public int build_time = 0;
        }

        public class TownHallDefine
        {
            public int level = 1;
            public int max_resource = 0;
            public int max_mining = 0;
            public int max_barrack = 0;
            public int max_tower = 0;
        }

        // tác vụ ko đồng bộ: luồng chính vẫn chạy, ko cần chờ mã hóa/giải mã xong
        public async static Task<string> Serialize<T>(this T target)
        {
            Task<string> task = Task.Run(() =>
            {
                // T: trường dữ liệu
                XmlSerializer xml = new XmlSerializer(typeof(T));
                StringWriter writer = new StringWriter();
                xml.Serialize(writer, target); // ghi trường dữ liệu của target vào writer
                return writer.ToString();
            });
            return await task;
        }

        public async static Task<T> Deserialize<T>(this string target)
        {
            Task<T> task = Task.Run(() =>
            {
                XmlSerializer xml = new XmlSerializer(typeof(T));
                StringReader reader = new StringReader(target);
                return (T)xml.Deserialize(reader); // đổi dữ liệu từ string -> trường dữ liệu
            });
            return await task;  
        }
    }
}
