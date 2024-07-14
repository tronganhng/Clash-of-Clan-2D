using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Threading.Tasks;
using DevelopersHub.RealtimeNetworking.Client;
using System;

namespace Proj2.clashofclan_2d
{
    public class Create_soldier : MonoBehaviour
    {
        public UnitDatabaseOS unitDataOS;
        public GameObject train_unit, Content;
        public Sprite[] char_sprite;
        public Button[] char_buts;
        public Text slot;
        public int barrack_capa = 0;
        public int occupy = 0;

        public static Create_soldier _instance = null;
        public static Create_soldier instance { get { return _instance; } }
        private void Awake()
        {
            _instance = this;
        }

        // đồng bộ training unit
        private void Start()
        {
            foreach (KeyValuePair<string, Data.Unit> kvp in Units.instance.units)
            {
                occupy += kvp.Value.training + kvp.Value.ready;
                if(kvp.Value.training > 0)
                {
                    int char_index = unitDataOS.unitData.FindIndex(data => data.Name == kvp.Key);
                    GameObject training_unit = Instantiate(train_unit);
                    training_unit.transform.SetParent(Content.transform);
                    training_unit.GetComponent<Train_unit>().sprite = char_sprite[char_index];
                    training_unit.GetComponent<Train_unit>().unitName = kvp.Key;
                    training_unit.GetComponent<Train_unit>().train_cooldown = Units.instance.def_unit[kvp.Key].train_time;
                    training_unit.GetComponent<Train_unit>().quantity_txt.text = "x" + kvp.Value.training;
                }           
            }
            foreach (KeyValuePair<int, GameObject> kvp in Buildings.instance.build_prefab)
            {
                BuildingDefineData buildDef = kvp.Value.GetComponent<BuildingDefineData>();
                if (buildDef.building.buildingName == "barrack")
                {
                    barrack_capa += buildDef.def_build.capacity;
                }
            }
            slot.text = occupy + "/" + barrack_capa;
        }

        private void OnEnable()
        {
            EnableButtons();
        }

        public void Spawn_TrainUnit(string unitName, int train_time)
        {
            occupy++;
            slot.text = occupy + "/" + barrack_capa;
            int char_index = unitDataOS.unitData.FindIndex(data => data.Name == unitName);
            if (Units.instance.units[unitName].training == 1)
            {
                GameObject training_unit = Instantiate(train_unit); // spawn avatar khi chưa có unit đc train 
                training_unit.transform.SetParent(Content.transform);
                training_unit.GetComponent<Train_unit>().sprite = char_sprite[char_index];
                training_unit.GetComponent<Train_unit>().unitName = unitName;
                training_unit.GetComponent<Train_unit>().train_cooldown = train_time;
            }
            else
            {
                foreach (Transform Train_unit in Content.transform)
                {
                    if (Train_unit.GetComponent<Train_unit>().unitName == unitName)
                        Train_unit.GetComponent<Train_unit>().quantity_txt.text = "x" + Units.instance.units[unitName].training;
                }
            }
        }  

        public void TrainRequest(int char_index)
        {
            int level = 1;
            Packet packet = new Packet();
            packet.Write(7);  //packetID
            packet.Write(unitDataOS.unitData[char_index].Name);
            packet.Write(level);
            Sender.TCP_Send(packet);
        }

        public void Delete_TrainUnit()
        {
            foreach (Transform Child in Content.transform)
            {
                Destroy(Child.gameObject);
            }

            foreach(KeyValuePair<string, Data.Unit> kvp in Units.instance.units)
            {
                occupy -= kvp.Value.training;
                kvp.Value.training = 0;
                kvp.Value.trained_time = DateTime.Now;
                kvp.Value.is_training = false;
            }
            slot.text = occupy + "/" + barrack_capa;
            Packet packet = new Packet();
            packet.Write(8);
            packet.Write("all");
            Sender.TCP_Send(packet);
        }

        void EnableButtons()
        {
            int barrackLV = 0;
            foreach(KeyValuePair<int, GameObject> kvp in Buildings.instance.build_prefab)
            {
                BuildingDefineData buildDef = kvp.Value.GetComponent<BuildingDefineData>();
                if (buildDef.building.buildingName == "barrack")
                {
                    if (buildDef.building.level >= barrackLV)
                        barrackLV = buildDef.building.level;
                }    
            }    
            for(int i=0; i<char_buts.Length; i++)
            {
                if (Units.instance.def_unit[unitDataOS.unitData[i].Name].req_Barracklv > barrackLV)
                    char_buts[i].interactable = false;
                else
                    char_buts[i].interactable = true;
            }    
        }    
    }
}
