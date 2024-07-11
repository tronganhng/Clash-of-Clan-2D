using System;
using System.Numerics;

namespace DevelopersHub.RealtimeNetworking.Server
{
    class Terminal
    {

        #region Update
        public const int updatesPerSecond = 30;
        public static void Update()
        {
            Database.Update();
        }
        #endregion

        #region Connection
        public const int maxPlayers = 100000;
        public const int port = 5555;
        public static void OnClientConnected(int id, string ip)
        {
            
        }

        public static void OnClientDisconnected(int id, string ip)
        {
            
        }
        #endregion

        #region Data
        public enum RequestID
        {
            AUTH = 1, // xác thực player
            SYNC = 2, // đồng bộ data
            BUILD = 3,  // xây dựng
            REPLACE = 4, // đổi chỗ building
            COLLECT = 5,  // thu thập tài nguyên
            UPGRADE = 6,   // nâng cấp building
            TRAIN = 7,   // training unit
            CANCEL_TRAIN = 8,
            FIND_ENEMY = 9
        }

        public static void ReceivedPacket(int clientID, Packet packet)
        {
            // đọc packet
            int packetID = packet.ReadInt();
            string device = "";
            switch (packetID)
            {
                case 1:
                    device = packet.ReadString();
                    Database.AuthenticatePlayer(clientID, device);
                    break;
                case 2:
                    device = packet.ReadString();
                    string type = packet.ReadString();
                    Database.SyncPlayerData(clientID, device, type);
                    break;
                case 3:
                    device = packet.ReadString();
                    string buildingName = packet.ReadString();
                    int posX = packet.ReadInt();
                    int posY = packet.ReadInt();
                    Database.PlaceBuilding(clientID, device, buildingName, posX, posY);
                    break;
                case 4:
                    int old_posX = packet.ReadInt();
                    int old_posY = packet.ReadInt();
                    int new_posX = packet.ReadInt();
                    int new_posY = packet.ReadInt();
                    Database.ReplaceBuilding(clientID, old_posX, old_posY, new_posX, new_posY);
                    break;
                case 5:
                    string nameOfbuilding = packet.ReadString();
                    Database.Collect(clientID, nameOfbuilding);
                    break;
                case 6:
                    int id = packet.ReadInt();
                    Database.UpgradeBuilding(clientID, id);
                    break;
                case 7:
                    string unitName = packet.ReadString();
                    int level = packet.ReadInt();
                    Database.TrainingUnit(clientID, unitName, level);
                    break;
                case 8:
                    unitName = packet.ReadString();
                    if(unitName == "all")
                    {
                        Database.DeleteAllTrainingUnit(clientID);
                    }    
                    else
                    {
                        int req_gold = packet.ReadInt();
                        int req_wood = packet.ReadInt();
                        Database.CancelTrainingUnit(clientID, unitName, req_gold, req_wood);
                    }    
                    break;
                case 9:
                    Database.FindEnemy(clientID);
                    break;
            }
        }

        public static void ReceivedBytes(int clientID, int packetID, byte[] data)
        {
            
        }

        public static void ReceivedString(int clientID, int packetID, string data)
        {
            
        }

        public static void ReceivedInteger(int clientID, int packetID, int data)
        {            
                
        }

        public static void ReceivedFloat(int clientID, int packetID, float data)
        {

        }

        public static void ReceivedBoolean(int clientID, int packetID, bool data)
        {

        }

        public static void ReceivedVector3(int clientID, int packetID, Vector3 data)
        {

        }

        public static void ReceivedQuaternion(int clientID, int packetID, Quaternion data)
        {

        }

        public static void ReceivedLong(int clientID, int packetID, long data)
        {

        }

        public static void ReceivedShort(int clientID, int packetID, short data)
        {

        }

        public static void ReceivedByte(int clientID, int packetID, byte data)
        {

        }

        public static void ReceivedEvent(int clientID, int packetID)
        {

        }
        #endregion

    }
}