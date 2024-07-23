using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Proj2.clashofclan_2d;
using DevelopersHub.RealtimeNetworking.Client;

public class Battle : MonoBehaviour
{
    public BuildingDatabaseOS buildDataOS;
    public GameObject DisconnectUI;
    public AstarPath Astar_map;
    public long enemy_id;

    public static Battle instance = null;
    public enum RequestID
    {
        AUTH = 1,
        SYNC = 2,
        BUILD = 3,
        REPLACE = 4,
        COLLECT = 5,
        UPGRADE = 6,
        TRAIN = 7,
        CANCEL_TRAIN = 8,
        FIND_ENEMY = 9, 
        CLAIM_ITEM = 10, // in ItemMove
        CLEAR_STORAGE = 11,  // xóa storage của goldmine, woodmine khi bị đánh
        UPDATE_UNIT = 12 // cap nhat so luong troop
    }

    bool connected = false;
    float timer = 0;
    void Start()
    {
        if (SceneManager.GetActiveScene().buildIndex != 2) return;
        if (instance == null) instance = this;
        RealtimeNetworking.OnPacketReceived += ReceivedPacket;
        RealtimeNetworking.OnDisconnectedFromServer += DisconnectedFromServer;
        if (Client.instance.isConnected)
        {
            Packet packet = new Packet();
            packet.Write((int)RequestID.AUTH);
            packet.Write(SystemInfo.deviceUniqueIdentifier);
            Sender.TCP_Send(packet);
        }
    }

    void Update()
    {
        if (connected)
        {
            if (timer >= 4)
            {
                timer = 0;
                
            }
            else
            {
                timer += Time.deltaTime;
            }

        }
        if (!Client.instance.isConnected)
        {
            DisconnectUI.SetActive(true);
        }
    }

    public void ReceivedPacket(Packet packet)
    {
        int packetID = packet.ReadInt();
        switch (packetID)
        {
            case 1:// AUTH
                connected = true;
                timer = 0;
                Packet packet1 = new Packet();
                packet1.Write((int)RequestID.SYNC);
                packet1.Write(SystemInfo.deviceUniqueIdentifier);
                packet1.Write("all");
                Sender.TCP_Send(packet1);
                Packet packet2 = new Packet();
                packet2.Write((int)RequestID.FIND_ENEMY);
                Sender.TCP_Send(packet2);
                break;
            case 2: // SYNC
                string data = packet.ReadString();
                Data.Player playerdata = Data.Deserialize<Data.Player>(data);
                SyncPlayerData(playerdata);
                CombatSystem.instance.unitAlive_cnt = 0;
                foreach (KeyValuePair<string, Data.Unit> kvp in Units.instance.units)
                {
                    CombatSystem.instance.unitAlive_cnt += kvp.Value.ready;
                }
                break;
            case 9: // FIND ENEMY
                Debug.Log("recieve 9");
                enemy_id = packet.ReadLong();
                string enemydata = packet.ReadString();
                Data.Player enemyData = Data.Deserialize<Data.Player>(enemydata);
                SyncEnemyData(enemyData);
                CombatSystem.instance.buildAlive_cnt = Buildings.instance.build_prefab.Count;
                EnemyResource.instance.GetResource();
                Astar_map.Scan();
                break;
            case 10: // cancel training & claimitem
                int gold_cnt = packet.ReadInt();
                int wood_cnt = packet.ReadInt();
                ResourceControll.instance.gold_cnt = gold_cnt;
                ResourceControll.instance.wood_cnt = wood_cnt;
                ResourceControll.instance.SetAllItemCount();
                break;
        }
    }

    void SyncEnemyData(Data.Player enemyData)
    {
        if (enemyData.buildings.Count > 0)
        {
            foreach (var data in enemyData.buildings)
            {
                Vector2 spawnPos = new Vector2(data.pos_x, data.pos_y);
                int index = buildDataOS.buildingData.FindIndex(obj => obj.Name == data.buildingName);
                GameObject prefab = buildDataOS.buildingData[index].Prefab;

                // spawn
                GameObject Building = Instantiate(prefab, spawnPos, Quaternion.identity);
                BuildingDefineData stat = Building.GetComponent<BuildingDefineData>();
                Buildings.instance.build_prefab[data.id] = Building;
                // tìm chỉ số của building
                Data.DefineBuilding defData = FindBuildingDefine(enemyData.Defbuildings, data);
                if (defData != null)
                {
                    stat.building = data;
                    stat.def_build = defData;
                }

            }
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
    void SyncPlayerData(Data.Player playerdata)
    {
        // Resource
        Buildings.instance.max_resource = playerdata.townHallDef.max_resource;
        ResourceControll.instance.gold_cnt = playerdata.gold;
        ResourceControll.instance.wood_cnt = playerdata.wood;
        ResourceControll.instance.SetAllItemCount();
        Buildings.instance.max_resource = playerdata.townHallDef.max_resource;
        // Units
        foreach (Data.Unit unit in playerdata.units)
        {
            Units.instance.units[unit.name] = unit;
        }
        // DefineUnit
        foreach (Data.DefineUnit defunit in playerdata.Defunits)
        {
            Units.instance.def_unit[defunit.name] = defunit;
        }
        // UI unit
        UnitBattle.instance.SetupUnitBattle();
    }
    
    public void DisconnectedFromServer()
    {
        connected = false;
        Debug.Log("Disconnected!");
        RealtimeNetworking.OnDisconnectedFromServer -= DisconnectedFromServer;
    }

    public void Reconnect()
    {
        RealtimeNetworking.OnPacketReceived -= ReceivedPacket;
        SceneManager.LoadScene(0);
    }
}
