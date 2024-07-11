﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Proj2.clashofclan_2d;
using DevelopersHub.RealtimeNetworking.Client;

public class Battle : MonoBehaviour
{
    public BuildingDatabaseOS buildDataOS;
    public AstarPath Astar_map;
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
        FIND_ENEMY = 9
    }

    bool connected = false;
    float timer = 0;
    void Start()
    {
        RealtimeNetworking.OnPacketReceived += ReceivedPacket;
        ConnectToServer();
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
    }

    void ReceivedPacket(Packet packet)
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
                break;
            case 9: // FIND ENEMY
                Debug.Log("recieve 9");
                string enemydata = packet.ReadString();
                Data.Player enemyData = Data.Deserialize<Data.Player>(enemydata);
                SyncEnemyData(enemyData);
                Astar_map.Scan();
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
        ResourceControll.instance.gold_cnt = playerdata.gold;
        ResourceControll.instance.wood_cnt = playerdata.wood;
        ResourceControll.instance.meat_cnt = playerdata.meat;
        ResourceControll.instance.SetAllItemCount();        
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

    void ConnectionResponse(bool sucessful)
    {
        if (sucessful)
        {
            RealtimeNetworking.OnDisconnectedFromServer += DisconnectedFromServer;
            string device = SystemInfo.deviceUniqueIdentifier;
            Packet packet = new Packet();
            packet.Write((int)RequestID.AUTH);
            packet.Write(device);
            Sender.TCP_Send(packet);
            Sender.TCP_Send(packet);


        }
        else
        {
            // fail -> reconnect
        }
        RealtimeNetworking.OnConnectingToServerResult -= ConnectionResponse;
    }
    public void ConnectToServer()
    {        
        RealtimeNetworking.OnConnectingToServerResult += ConnectionResponse;  
        RealtimeNetworking.Connect();
    }
    void DisconnectedFromServer()
    {
        connected = false;
        RealtimeNetworking.OnDisconnectedFromServer -= DisconnectedFromServer;
        // reconnect
    }
}
