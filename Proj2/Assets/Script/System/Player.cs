namespace Proj2.clashofclan_2d  // namespace giúp sử dụng lại các class trong nó cho các script khác
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using UnityEngine.SceneManagement;
    using DevelopersHub.RealtimeNetworking.Client;

    public class Player : MonoBehaviour
    {
        public Text Nofication;
        public enum RequestID
        {
            AUTH = 1, // thuộc header của segment vs ý nghĩa: xác thực ng chơi
            SYNC = 2 , // yêu cầu đồng bộ dữ liệu
            BUILD = 3,  // yêu cầu xây dựng
            REPLACE = 4, // đổi chỗ building
            COLLECT = 5,  // thu thập tài nguyên
            UPGRADE = 6,   // nâng cấp building
            TRAIN = 7,   // training unit
            CANCEL_TRAIN = 8,
            FIND_ENEMY = 9,
            CLAIM_ITEM = 10,
            CLEAR_STORAGE = 11,  // xóa storage của goldmine, woodmine khi bị đánh
            UPDATE_UNIT = 12 // cap nhat so luong troop
        }

        bool connected = false;
        float timer = 0;

        public static Player instance = null;

        void Start()
        {
            if (instance == null) instance = this;
            // thêm hàm vào event
            if (SceneManager.GetActiveScene().buildIndex != 0) return;
            RealtimeNetworking.OnPacketReceived += ReceivedPacket;
            if (Client.instance.isConnected)
            {                
                Packet packet = new Packet();
                packet.Write((int)RequestID.AUTH);
                packet.Write(SystemInfo.deviceUniqueIdentifier);
                Sender.TCP_Send(packet);
            }
            else
                ConnectToServer();
        }

        void Update()
        {
            // load tài nguyên mỗi 4s
            if(connected)
            {
                if(timer >= 4)
                {
                    timer = 0;
                    Packet packet1 = new Packet();
                    packet1.Write((int)RequestID.SYNC);
                    packet1.Write(SystemInfo.deviceUniqueIdentifier);
                    packet1.Write("update");
                    Sender.TCP_Send(packet1);
                }
                else
                {
                    timer += Time.deltaTime;
                } 
                    
            }    
        }

        // nhận đc gói tin packet từ server
        public void ReceivedPacket(Packet packet)
        {
            int packetID = packet.ReadInt();      
            switch (packetID)
            {
                case 1:// xác thực
                    connected = true;
                    timer = 0;
                    long account_id = packet.ReadLong();
                    Debug.Log("Login sucess, your accountID: " + account_id);
                    Packet packet1 = new Packet();
                    packet1.Write((int)RequestID.SYNC);
                    packet1.Write(SystemInfo.deviceUniqueIdentifier);
                    packet1.Write("all");
                    Sender.TCP_Send(packet1);                    
                    break;
                case 2: // đồng bộ dl
                    string data = packet.ReadString();
                    Data.Player playerdata = Data.Deserialize<Data.Player>(data); // giải mã string -> trường dl
                    string type = packet.ReadString();                   
                    // import data
                    if(type == "all")
                        SyncPlayerData(playerdata); // đồng bộ all
                    if (type == "update")
                        SyncUpdateData(playerdata); // đồng bộ dl cần update
                    break;
                case 3: // build
                    bool canPlace = packet.ReadBool();
                    PlacementSystem.instance.canPlace = canPlace;
                    PlacementSystem.instance.response = true;
                    if (canPlace)
                    {
                        string defbuild = packet.ReadString();
                        string build = packet.ReadString();                        
                        Data.DefineBuilding defBuild = Data.Deserialize<Data.DefineBuilding>(defbuild);
                        Data.Building Build = Data.Deserialize<Data.Building>(build);

                        PlacementSystem.instance.buildData = Build;
                        PlacementSystem.instance.defData = defBuild;

                        ResourceControll.instance.gold_cnt -= defBuild.req_gold;
                        ResourceControll.instance.wood_cnt -= defBuild.req_wood;
                        ResourceControll.instance.SetAllItemCount();
                    }
                    break;
                case 4: // replace
                    // Server ko cần phản hồi
                    break;
                case 5: // collect
                    int gold_cnt = packet.ReadInt();
                    int wood_cnt = packet.ReadInt();
                    ResourceControll.instance.gold_cnt = gold_cnt;
                    ResourceControll.instance.wood_cnt = wood_cnt;
                    ResourceControll.instance.SetAllItemCount();
                    break;
                case 6: // upgrade building
                    int canUpgrade = packet.ReadInt();
                    if(canUpgrade == 0) // success
                    {
                        int id = packet.ReadInt();
                        string build_def = packet.ReadString();
                        string building = packet.ReadString();
                        Data.DefineBuilding buildDef = Data.Deserialize<Data.DefineBuilding>(build_def);
                        Data.Building Building = Data.Deserialize<Data.Building>(building);

                        UpgradeBuilding(id, Building, buildDef);
                    }   
                    else if(canUpgrade == 1) // not enough THlv
                    {
                        Nofication.text = "You need to upgrade your TownHall";
                        Nofication.gameObject.SetActive(true);
                        StartCoroutine(TurnOffNofication());
                    }    
                    else if(canUpgrade == 2) // no resource
                    {
                        Nofication.text = "Not enough resource";
                        Nofication.gameObject.SetActive(true);
                        StartCoroutine(TurnOffNofication());
                    }
                    break;
                case 7: // training
                    int canTrain = packet.ReadInt();
                    if (canTrain == 0)
                    {
                        string Unit = packet.ReadString();                        
                        Data.Unit unit = Data.Deserialize<Data.Unit>(Unit);                       
                        Units.instance.units[unit.name] = unit;
                        Data.DefineUnit defUnit = Units.instance.def_unit[unit.name];
                        ResourceControll.instance.gold_cnt -= defUnit.req_gold;
                        ResourceControll.instance.wood_cnt -= defUnit.req_wood;
                        ResourceControll.instance.SetAllItemCount();
                        Create_soldier.instance.Spawn_TrainUnit(unit.name, defUnit.train_time);
                    }
                    else if (canTrain == 1)
                    {
                        Nofication.text = "Full";
                        Nofication.gameObject.SetActive(true);
                        StartCoroutine(TurnOffNofication());
                    }
                    else if (canTrain == 2)
                    {
                        Nofication.text = "No resource";
                        Nofication.gameObject.SetActive(true);
                        StartCoroutine(TurnOffNofication());
                    }
                    break;
                case 8: // cancel training & claimitem
                    gold_cnt = packet.ReadInt();
                    wood_cnt = packet.ReadInt();
                    ResourceControll.instance.gold_cnt = gold_cnt;
                    ResourceControll.instance.wood_cnt = wood_cnt;
                    ResourceControll.instance.SetAllItemCount();
                    break;
            }
        }        
        
        void SyncPlayerData(Data.Player playerdata)
        {
            // Resource
            Buildings.instance.max_resource = playerdata.townHallDef.max_resource;
            ResourceControll.instance.gold_cnt = playerdata.gold;
            ResourceControll.instance.wood_cnt = playerdata.wood;
            ResourceControll.instance.SetAllItemCount();
            // Townhall
            Buildings.instance.max_build["goldmine"] = playerdata.townHallDef.max_mining;
            Buildings.instance.max_build["woodmine"] = playerdata.townHallDef.max_mining;
            Buildings.instance.max_build["barrack"] = playerdata.townHallDef.max_barrack;
            Buildings.instance.max_build["archer tower"] = playerdata.townHallDef.max_tower;
            // Buildings & Defbuilding
            if (playerdata.buildings != null && playerdata.buildings.Count > 0)
            {
                foreach(var data in playerdata.buildings)
                {
                    if (Buildings.instance.build_cnt.ContainsKey(data.buildingName))
                        Buildings.instance.build_cnt[data.buildingName] += 1;
                    else
                        Buildings.instance.build_cnt[data.buildingName] = 1;
                    
                    Vector2 spawnPos = new Vector2(data.pos_x, data.pos_y);
                    int index = PlacementSystem.instance.databaseOS.buildingData.FindIndex(obj => obj.Name == data.buildingName);
                    GameObject prefab = PlacementSystem.instance.databaseOS.buildingData[index].Prefab;
                    Vector2Int int_spawnPos = Vector2Int.RoundToInt(spawnPos);
                    Vector2Int prefabSize = PlacementSystem.instance.databaseOS.buildingData[index].Size;

                    // add to gridData
                    PlacementSystem.instance.floorData.AddObjectAt(int_spawnPos, prefabSize, index);
                    // spawn
                    GameObject Building = Instantiate(prefab, spawnPos, Quaternion.identity);
                    BuildingDefineData stat = Building.GetComponent<BuildingDefineData>();
                    Buildings.instance.build_prefab[data.id] = Building;
                    // tìm chỉ số của building
                    Data.DefineBuilding defData = FindBuildingDefine(playerdata.Defbuildings, data);
                    if(defData != null)
                    {
                        stat.building = data;
                        stat.def_build = defData;
                    }    

                }    
            }
            // Units
            foreach(Data.Unit unit in playerdata.units)
            {
                Units.instance.units[unit.name] = unit;
            }
            // DefineUnit
            foreach (Data.DefineUnit defunit in playerdata.Defunits)
            {
                Units.instance.def_unit[defunit.name] = defunit;
            }
        }          
        Data.DefineBuilding FindBuildingDefine(List<Data.DefineBuilding> Defbuildings, Data.Building data)
        {
            foreach (var defData in Defbuildings)
            {
                if (defData.buildingName == data.buildingName && defData.level == data.level)
                {
                    return defData;
                }
            }
            return null;
        }    
        
        void SyncUpdateData(Data.Player playerdata)
        {
            // townhall
            Buildings.instance.max_build["goldmine"] = playerdata.townHallDef.max_mining;
            Buildings.instance.max_build["woodmine"] = playerdata.townHallDef.max_mining;
            Buildings.instance.max_build["barrack"] = playerdata.townHallDef.max_barrack;
            Buildings.instance.max_build["archer tower"] = playerdata.townHallDef.max_tower;
            // building
            if (playerdata.buildings != null && playerdata.buildings.Count > 0)
            {
                foreach (var data in playerdata.buildings)
                {
                    if(Buildings.instance.build_prefab.ContainsKey(data.id))
                        Buildings.instance.build_prefab[data.id].GetComponent<BuildingDefineData>().building = data;
                }
            }
            // units
            foreach(Data.Unit unit in playerdata.units)
            {
                if (Units.instance.units[unit.name] != null)
                    Units.instance.units[unit.name] = unit;
            }    
            // resource
            ResourceControll.instance.gold_cnt = playerdata.gold;
            ResourceControll.instance.wood_cnt = playerdata.wood;
            ResourceControll.instance.SetAllItemCount();
        }    

        void UpgradeBuilding(int id, Data.Building Building, Data.DefineBuilding buildDef)
        {
            GameObject prefab = Buildings.instance.build_prefab[id];
            ResourceControll.instance.gold_cnt -= prefab.GetComponent<BuildingDefineData>().def_build.req_gold;
            ResourceControll.instance.wood_cnt -= prefab.GetComponent<BuildingDefineData>().def_build.req_wood;
            ResourceControll.instance.SetAllItemCount();

            prefab.GetComponent<BuildingDefineData>().def_build = buildDef;
            prefab.GetComponent<BuildingDefineData>().building = Building;
            prefab.GetComponentInChildren<Constructing>().SetConstruction();
        }

        IEnumerator TurnOffNofication()
        {
            yield return new WaitForSeconds(1.4f);
            Nofication.gameObject.SetActive(false);
        }

        void ConnectionResponse(bool sucessful)
        {
            Debug.Log("Result");
            // sucessful: kết quả lắng nghe
            if(sucessful)
            {
                RealtimeNetworking.OnDisconnectedFromServer += DisconnectedFromServer; // check nếu bị disconnect
                string device = SystemInfo.deviceUniqueIdentifier; // mã thiết bị (giống đ/c MAC)
                Packet packet = new Packet();
                packet.Write((int)RequestID.AUTH);
                packet.Write(device);
                Sender.TCP_Send(packet); // gửi 1 TCP có header là AUTH, data là device

            }   
            else
            {
                // fail -> reconnect
            }    
            RealtimeNetworking.OnConnectingToServerResult -= ConnectionResponse; // xóa phản hồi đã nhận 
        }
        public void ConnectToServer()
        {
            //RealtimeNetworking.OnConnectingToServerResult: lắng nghe sever
            RealtimeNetworking.OnConnectingToServerResult += ConnectionResponse;  // nhận phản hồi
            RealtimeNetworking.Connect(); // kết nối đến server
        }
        void DisconnectedFromServer()
        {
            connected = false;
            RealtimeNetworking.OnDisconnectedFromServer -= DisconnectedFromServer;
            // reconnect
        }
    }
}
