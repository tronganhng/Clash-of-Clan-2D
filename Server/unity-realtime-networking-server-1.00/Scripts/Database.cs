using System;
using MySql.Data.MySqlClient;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace DevelopersHub.RealtimeNetworking.Server
{
    class Database
    {

        #region MySQL
        
        private const string _mysqlServer = "127.0.0.1";
        private const string _mysqlUsername = "root";
        private const string _mysqlPassword = "";
        private const string _mysqlDatabase = "clashofclan_2d";
        
        public static MySqlConnection GetMySqlConnection()
        {
            MySqlConnection newconnect = new MySqlConnection("SERVER=" + _mysqlServer + "; DATABASE=" + _mysqlDatabase + "; UID=" + _mysqlUsername + "; PASSWORD=" + _mysqlPassword + "; POOLING = TRUE;");
            newconnect.Open();
            return newconnect;
        }

        private static DateTime collectTime = DateTime.Now;
        private static bool waiting = false;

        private static DateTime constructTime = DateTime.Now;
        private static bool waiting1 = false;

        private static DateTime trainingTime = DateTime.Now;
        private static bool waiting2 = false;
        public static void Update()
        {
            if (!waiting)
            {
                double deltaTime = (DateTime.Now - collectTime).TotalSeconds;
                if(deltaTime >= 5) // 5s cập nhật 1 lần
                {
                    waiting = true;
                    collectTime = DateTime.Now;
                    UpdateCollect(deltaTime);
                    UpdateConstruct();
                }
            }

            if (!waiting1)
            {
                double deltaTime = (DateTime.Now - constructTime).TotalSeconds;
                if (deltaTime >= 5) // 5s cập nhật 1 lần
                {
                    waiting1 = true;
                    constructTime = DateTime.Now;
                    UpdateConstruct();
                }
            }

            if (!waiting2)
            {
                double deltaTime = (DateTime.Now - constructTime).TotalSeconds;
                if (deltaTime >= 3) // 3s cập nhật 1 lần
                {
                    waiting2 = true;
                    trainingTime = DateTime.Now;
                    UpdateTraining();
                }
            }
        }

        // hàm async: bất đồng bộ (giống Coroutine)
        // 1----AUTH----
        public async static void AuthenticatePlayer(int clientId, string device)
        {
            long account_id = await AuthenticatePlayerAsync(clientId, device);
            // Xác thực thành công -> lưu ng chơi đang online
            Server.clients[clientId].device = device;
            Server.clients[clientId].account_id = account_id;

            Packet packet = new Packet();
            packet.Write(1); // packetID
            packet.Write(account_id);
            Sender.TCP_Send(clientId, packet);
        }
        async static Task<long> AuthenticatePlayerAsync(int clientId, string device)
        {
            // chạy 1 tác vụ
            Task<long> task = Task.Run(() =>
            {
                long account_id = 0;
                Console.WriteLine(device);
                // lấy ra account_id có device_id = device
                using (MySqlConnection connection = GetMySqlConnection())
                {
                    string query = String.Format("SELECT id FROM account WHERE device_id = '{0}';", device);
                    bool found_id = false;
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataReader reader = command.ExecuteReader()) // đọc dl
                        {
                            if (reader.HasRows) // đã có account_id
                            {
                                while (reader.Read())
                                {
                                    found_id = true;
                                    account_id = long.Parse(reader["id"].ToString());
                                }
                            }
                        }
                    }
                    if(found_id)
                    {
                        query = String.Format("UPDATE account SET is_online = 1 WHERE id = {0};", account_id);
                        using (MySqlCommand command = new MySqlCommand(query, connection))
                        {
                            command.ExecuteNonQuery();
                        }
                    }
                    else // ko thấy accID -> lập acc
                    {
                        // tạo id mới
                        query = String.Format("INSERT INTO account (device_id, is_online) VALUES('{0}', 1);", device);
                        using (MySqlCommand command = new MySqlCommand(query, connection))
                        {
                            command.ExecuteNonQuery();
                            account_id = command.LastInsertedId;
                        }
                        // thêm default building
                        query = String.Format("INSERT INTO buildings (name, account_id, level, posX, posY) VALUES('{0}','{1}','{2}','{3}','{4}');", "town hall", account_id, 1, -5, -3);
                        using (MySqlCommand command = new MySqlCommand(query, connection))
                        {
                            command.ExecuteNonQuery();
                        }
                    }
                    connection.Close();
                }
                return account_id;
            });
            return await task;
        }
        public async static void PlayerDisconnect(int clientId)
        {
            await Task.Run(() =>
            {
                using (MySqlConnection connection = GetMySqlConnection())
                {
                    long account_id = Server.clients[clientId].account_id;
                    string query = String.Format("UPDATE account SET is_online = 0 WHERE id = {0};", account_id);
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                    connection.Close();
                }
                Server.clients[clientId].account_id = 0;
            });
        }

        // 2----SYNC----
        public async static void SyncPlayerData(int clientId, string device , string type)
        {
            long account_id = Server.clients[clientId].account_id;
            Data.Player playerdata = await GetPlayerDataAsync(clientId); // lấy dl player trong sql
            List<Data.Building> listbuilding = await GetAllBuildingAsync(account_id); // lấy dl nhà trong sql
            List<Data.DefineBuilding> listDefbuilding = await GetAllBuildingDefAsync(account_id);
            Data.TownHallDefine townhallDef = await GetTownhallDefineAsync(account_id);
            List<Data.Unit> listUnit = await GetAllUnitAsync(account_id);
            List<Data.DefineUnit> listDefUnit = await GetAllUnitDefAsync();
            playerdata.buildings = listbuilding;
            playerdata.Defbuildings = listDefbuilding;
            playerdata.townHallDef = townhallDef;
            playerdata.units = listUnit;
            playerdata.Defunits = listDefUnit;
            string data = await Data.Serialize<Data.Player>(playerdata); // mã hóa ko đồng bộ thành string
            Packet packet = new Packet();
            packet.Write(2);  // packetID
            packet.Write(data);
            packet.Write(type);
            Sender.TCP_Send(clientId, packet);
        }
        async static Task<Data.Player> GetPlayerDataAsync(int clientId) // resource
        {
            Task<Data.Player> task = Task.Run(() =>
            {
                Data.Player playerdata = new Data.Player();
                long account_id = Server.clients[clientId].account_id;
                // truy vấn
                using (MySqlConnection connection = GetMySqlConnection())
                {
                    string query = String.Format("SELECT id, wood, gold, meat FROM account WHERE id = {0};", account_id);
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    playerdata.gold = int.Parse(reader["gold"].ToString());
                                    playerdata.wood = int.Parse(reader["wood"].ToString());
                                    playerdata.meat = int.Parse(reader["meat"].ToString());
                                }
                            }
                        }
                    }
                    connection.Close();
                }
                return playerdata;
            });
            return await task;
        }
        async static Task<List<Data.Building>> GetAllBuildingAsync(long account_id) // buildings
        {
            Task<List<Data.Building>> task = Task.Run(() =>
            {
                List<Data.Building> playerdata = new List<Data.Building>();
                using (MySqlConnection connection = GetMySqlConnection())
                {
                    string query = String.Format("SELECT id, name, level, posX, posY, storage, construct_time, is_constructing FROM buildings WHERE account_id = '{0}';", account_id);
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    Data.Building building = new Data.Building();
                                    building.id = int.Parse(reader["id"].ToString());
                                    building.account_id = account_id;
                                    building.buildingName = reader["name"].ToString();
                                    building.level = int.Parse(reader["level"].ToString());
                                    building.pos_x = int.Parse(reader["posX"].ToString());
                                    building.pos_y = int.Parse(reader["posY"].ToString());
                                    building.storage = float.Parse(reader["storage"].ToString());
                                    building.construct_time = DateTime.Parse(reader["construct_time"].ToString());
                                    building.is_constructing = int.Parse(reader["is_constructing"].ToString()) > 0;
                                    playerdata.Add(building);  // add thông tin từng building vào list
                                }
                            }
                        }
                    }
                    connection.Close();
                }
                return playerdata;
            });
            return await task;
        }
        async static Task<List<Data.DefineBuilding>> GetAllBuildingDefAsync(long account_id) // buildingsDefine
        {
            Task<List<Data.DefineBuilding>> task = Task.Run(() =>
            {
                List<Data.DefineBuilding> playerdata = new List<Data.DefineBuilding>();
                // join buildings & define_building để lấy ra các definedata cần thiết
                using (MySqlConnection connection = GetMySqlConnection())
                {
                    string query = String.Format("SELECT db.name, db.level, req_gold, req_wood, req_THlv, db.health, db.build_time, db.damage, db.speed, db.capacity FROM buildings b INNER JOIN define_building db ON b.name = db.name AND b.level = db.level WHERE b.account_id = '{0}';", account_id);
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    Data.DefineBuilding building = new Data.DefineBuilding();
                                    building.buildingName = reader["name"].ToString();
                                    building.level = int.Parse(reader["level"].ToString());
                                    building.req_gold = int.Parse(reader["req_gold"].ToString());
                                    building.req_wood = int.Parse(reader["req_wood"].ToString());
                                    building.req_THlv = int.Parse(reader["req_THlv"].ToString());
                                    building.health = int.Parse(reader["health"].ToString());
                                    building.build_time = int.Parse(reader["build_time"].ToString());
                                    building.damage = float.Parse(reader["damage"].ToString());
                                    building.speed = int.Parse(reader["speed"].ToString());
                                    building.capacity = int.Parse(reader["capacity"].ToString());
                                    playerdata.Add(building);  // add thông tin từng building vào list
                                }
                            }
                        }
                    }
                    connection.Close();
                }
                return playerdata;
            });
            return await task;
        }
        async static Task<Data.TownHallDefine> GetTownhallDefineAsync(long account_id)
        {
            Task<Data.TownHallDefine> task = Task.Run(() =>
            {
                Data.TownHallDefine townHall = new Data.TownHallDefine();
                using (MySqlConnection connection = GetMySqlConnection())
                {
                    string query = String.Format("SELECT * FROM define_townhall dt INNER JOIN buildings b ON (b.is_constructing = 0 AND b.level = dt.level) OR (b.is_constructing > 0 AND b.level - 1 = dt.level) WHERE b.name = 'town hall' AND b.account_id = {0};", account_id);
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    townHall.level = int.Parse(reader["level"].ToString());
                                    townHall.max_resource = int.Parse(reader["resource_limit"].ToString());
                                    townHall.max_barrack = int.Parse(reader["barrack_limit"].ToString());
                                    townHall.max_tower = int.Parse(reader["archertower_limit"].ToString());
                                    townHall.max_mining = int.Parse(reader["mining_limit"].ToString());
                                }
                            }
                        }
                    }
                    connection.Close();
                }
                return townHall;
            });
            return await task;
        }
        async static Task<List<Data.Unit>> GetAllUnitAsync(long account_id) 
        {
            Task<List<Data.Unit>> task = Task.Run(() =>
            {
                List<Data.Unit> unitsData = new List<Data.Unit>();
                using (MySqlConnection connection = GetMySqlConnection())
                {
                    string query = String.Format("SELECT * FROM units WHERE account_id = '{0}';", account_id);
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    Data.Unit unit = new Data.Unit();
                                    unit.id = int.Parse(reader["id"].ToString());
                                    unit.account_id = account_id;
                                    unit.name = reader["name"].ToString();
                                    unit.level = int.Parse(reader["level"].ToString());
                                    unit.training = int.Parse(reader["training"].ToString());
                                    unit.ready = int.Parse(reader["ready"].ToString());
                                    unit.trained_time = DateTime.Parse(reader["trained_time"].ToString());
                                    unit.is_training = int.Parse(reader["is_training"].ToString()) > 0;
                                    unitsData.Add(unit);
                                }
                            }
                        }
                    }
                    connection.Close();
                }
                return unitsData;
            });
            return await task;
        }
        async static Task<List<Data.DefineUnit>> GetAllUnitDefAsync() 
        {
            Task<List<Data.DefineUnit>> task = Task.Run(() =>
            {
                List<Data.DefineUnit> defData = new List<Data.DefineUnit>();
                
                using (MySqlConnection connection = GetMySqlConnection())
                {
                    string query = String.Format("SELECT * FROM define_unit;");
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    Data.DefineUnit defUnit = new Data.DefineUnit();
                                    defUnit.name = reader["name"].ToString();
                                    defUnit.level = int.Parse(reader["level"].ToString());
                                    defUnit.train_time = int.Parse(reader["train_time"].ToString());
                                    defUnit.req_gold = int.Parse(reader["req_gold"].ToString());
                                    defUnit.req_wood = int.Parse(reader["req_wood"].ToString());
                                    defUnit.req_Barracklv = int.Parse(reader["req_Barracklv"].ToString());
                                    defData.Add(defUnit);
                                }
                            }
                        }
                    }
                    connection.Close();
                }
                return defData;
            });
            return await task;
        }


        // 3----BUILD----
        public async static void PlaceBuilding(int clientId, string device, string buildingName, int posX, int posY)
        {
            Data.Player playerdata = await GetPlayerDataAsync(clientId);
            Data.DefineBuilding buildingdef = await GetBuildingDefineAsync(buildingName, 0);
            Data.DefineBuilding buildingdef1 = await GetBuildingDefineAsync(buildingName, 1);

            if (playerdata.gold >= buildingdef.req_gold && playerdata.wood >= buildingdef.req_wood)
            {
                long account_id = Server.clients[clientId].account_id;
                int id, buildTime = 0;

                // thêm building vào sql
                using (MySqlConnection connection = GetMySqlConnection())
                {
                    id = await Task.Run(() =>
                    {
                        string query = String.Format("SELECT build_time FROM define_building WHERE name = '{0}' AND level = 1;", buildingName);
                        using (MySqlCommand command = new MySqlCommand(query, connection))
                        {
                            using (MySqlDataReader reader = command.ExecuteReader()) // đọc dl
                            {
                                if (reader.HasRows) // đã có account_id
                                {
                                    while (reader.Read())
                                    {                                    
                                        buildTime = int.Parse(reader["build_time"].ToString());
                                    }
                                }
                            }
                        }

                        query = String.Format("INSERT INTO buildings (name, account_id, level, posX, posY, is_constructing, construct_time) VALUES('{0}', {1}, {2}, {3}, {4}, 1, NOW() + INTERVAL {5} SECOND);", buildingName, account_id, 1, posX, posY, buildTime);
                        using (MySqlCommand command = new MySqlCommand(query, connection))
                        {
                            command.ExecuteNonQuery();
                            id = (int)command.LastInsertedId;
                        }

                        // cập nhật player resource 
                        query = String.Format("UPDATE account SET gold = gold - {0}, wood = wood - {1} WHERE device_id = '{2}';", buildingdef.req_gold, buildingdef.req_wood, device);
                        using (MySqlCommand command = new MySqlCommand(query, connection))
                        {
                            command.ExecuteNonQuery();
                        }
                        return id;
                    });
                    Data.Building building = await GetBuildingAsync(clientId, id);
                    string build = await Data.Serialize<Data.Building>(building);
                    string buildDef = await Data.Serialize<Data.DefineBuilding>(buildingdef1);
                    Packet packet = new Packet();
                    packet.Write(3);
                    packet.Write(true);
                    packet.Write(buildDef);
                    packet.Write(build);
                    Sender.TCP_Send(clientId, packet);

                    connection.Close();
                }
            }
            else
            {
                Packet packet = new Packet();
                packet.Write(3);
                packet.Write(false);
                Sender.TCP_Send(clientId, packet);
            }
        }
        async static Task<Data.DefineBuilding> GetBuildingDefineAsync(string buildingName, int level)
        {
            Task<Data.DefineBuilding> task = Task.Run(() =>
            {
                Data.DefineBuilding buildingdata = new Data.DefineBuilding();
                // truy vấn
                using (MySqlConnection connection = GetMySqlConnection())
                {
                    string query = String.Format("SELECT * FROM define_building WHERE name = '{0}' AND level = {1};", buildingName, level);
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    buildingdata.buildingName = buildingName;
                                    buildingdata.level = level;
                                    buildingdata.req_gold = int.Parse(reader["req_gold"].ToString());
                                    buildingdata.req_wood = int.Parse(reader["req_wood"].ToString());
                                    buildingdata.req_THlv = int.Parse(reader["req_THlv"].ToString());
                                    buildingdata.health = int.Parse(reader["health"].ToString());
                                    buildingdata.build_time = int.Parse(reader["build_time"].ToString());
                                    buildingdata.damage = float.Parse(reader["damage"].ToString());
                                    buildingdata.speed = int.Parse(reader["speed"].ToString());
                                    buildingdata.capacity = int.Parse(reader["capacity"].ToString());
                                }
                            }
                        }
                    }
                    connection.Close();
                }
                return buildingdata;
            });
            return await task;
        }
        async static Task<Data.Building> GetBuildingAsync(int clientID, int id)
        {
            Task<Data.Building> task = Task.Run(() =>
            {
                Data.Building building = new Data.Building();
                long account_id = Server.clients[clientID].account_id;
                // truy vấn
                using (MySqlConnection connection = GetMySqlConnection())
                {
                    string query = String.Format("SELECT * FROM buildings WHERE account_id = {0} AND id = {1};", account_id, id);
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    building.id = int.Parse(reader["id"].ToString());
                                    building.buildingName = reader["name"].ToString();
                                    building.account_id = account_id;
                                    building.level = int.Parse(reader["level"].ToString());
                                    building.pos_x = int.Parse(reader["posX"].ToString());
                                    building.pos_y = int.Parse(reader["posY"].ToString());
                                    building.storage = (int)float.Parse(reader["storage"].ToString());
                                    building.construct_time = DateTime.Parse(reader["construct_time"].ToString());
                                    building.is_constructing = int.Parse(reader["is_constructing"].ToString()) > 0;
                                }
                            }
                        }
                    }
                    connection.Close();
                }
                return building;
            });
            return await task;
        }


        // 4----REPLACE----
        public async static void ReplaceBuilding(int clientId, int old_posX, int old_posY, int new_posX, int new_posY)
        {            
            long account_id = Server.clients[clientId].account_id;
            await Task.Run(() =>
            {
                // cập nhật vị trí
                using (MySqlConnection connection = GetMySqlConnection())
                {
                    string query = String.Format("UPDATE buildings SET posX = {0}, posY = {1} WHERE account_id = {2} AND posX = {3} AND posY = {4};", new_posX, new_posY, account_id, old_posX, old_posY);
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                    connection.Close();
                }
            });
        }

        
        // 5----COLLECT----
        public async static void Collect(int clientID, string buildingName)
        {
            int amount = await CollectAsync(clientID, buildingName);
            int gold = 0, wood = 0;

            await Task.Run(() =>
            {
                using (MySqlConnection connection = GetMySqlConnection())
                {
                    string query = String.Format("SELECT  gold, wood FROM account WHERE id = {0};", Server.clients[clientID].account_id);
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    gold = int.Parse(reader["gold"].ToString());
                                    wood = int.Parse(reader["wood"].ToString());
                                }
                            }
                        }
                    }
                }

                Packet packet = new Packet();
                packet.Write(5);
                packet.Write(gold);
                packet.Write(wood);
                Sender.TCP_Send(clientID, packet);
            });
        }
        async static Task<int> CollectAsync(int clientID, string buildingName)
        {
            Task<int> task = Task.Run(() =>
            {
                long account_id = Server.clients[clientID].account_id;
                int amount = 0;
                using (MySqlConnection connection = GetMySqlConnection())
                {
                    string query = String.Format("SELECT  sum(storage) AS amount FROM buildings WHERE account_id = {0} AND name = '{1}';", account_id, buildingName);
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    amount = (int)float.Parse(reader["amount"].ToString());
                                }
                            }
                        }
                    }

                    query = String.Format("UPDATE buildings SET storage = 0 WHERE account_id = {0} AND name = '{1}';", account_id, buildingName);
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.ExecuteNonQuery();
                    }

                    int thresh_hold = 0;
                    query = String.Format("SELECT dt.resource_limit AS thresh_hold FROM define_townhall dt INNER JOIN buildings b ON b.level = dt.level WHERE b.name = 'town hall' AND b.account_id = {0};", account_id);
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    thresh_hold = int.Parse(reader["thresh_hold"].ToString());
                                }
                            }
                        }
                    }

                    if (buildingName == "goldmine")
                        query = String.Format("UPDATE account SET gold = LEAST(gold + {0}, {1}) WHERE id = {2};", amount,thresh_hold, account_id);
                    if(buildingName == "woodmine")
                        query = String.Format("UPDATE account SET wood = LEAST(wood + {0}, {1}) WHERE id = {2};", amount,thresh_hold, account_id);
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.ExecuteNonQuery();
                    }


                    connection.Close();
                }
                return amount;
            });
            return await task;            
        }


        // 6----UPGRADE----
        public async static void UpgradeBuilding(int clientID, int id)
        {
            Data.DefineBuilding buildDef = new Data.DefineBuilding();
            long account_id = Server.clients[clientID].account_id;
            int THlv = 1;
            bool TH_constructing = false;

            using (MySqlConnection connection = GetMySqlConnection())
            {
                // tìm building cần upgrade và dl của townhall
                await Task.Run(() =>
                {                    
                    string query = String.Format("SELECT name, level FROM buildings WHERE id = {0};", id);
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    buildDef.buildingName = reader["name"].ToString();
                                    buildDef.level = int.Parse(reader["level"].ToString());                                
                                }
                            }
                        }
                    }

                    query = String.Format("SELECT level, is_constructing FROM buildings WHERE account_id = {0} AND name = 'town hall';", account_id);
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    THlv = int.Parse(reader["level"].ToString());
                                    TH_constructing = int.Parse(reader["is_constructing"].ToString()) > 0;
                                }
                            }
                        }
                    }
                });

                buildDef = await GetBuildingDefineAsync(buildDef.buildingName, buildDef.level);
                Data.DefineBuilding buildDef1 = await GetBuildingDefineAsync(buildDef.buildingName, buildDef.level+1);
                Data.Player playerdata = await GetPlayerDataAsync(clientID);

                // ktra đk
                Packet packet = new Packet();
                packet.Write(6);
                if (playerdata.gold >= buildDef.req_gold && playerdata.wood >= buildDef.req_wood)
                {
                    if ((THlv >= buildDef.req_THlv && !TH_constructing) || (THlv-1 >= buildDef.req_THlv && TH_constructing))
                    {
                        // upgarde
                        await Task.Run(() =>
                        {
                            string query = String.Format("UPDATE buildings SET level = level + 1, construct_time = NOW() + INTERVAL {0} SECOND, is_constructing = 1 WHERE account_id = {1} AND id = {2};", buildDef1.build_time, account_id, id);
                            using (MySqlCommand command = new MySqlCommand(query, connection))
                            {
                                command.ExecuteNonQuery();
                            }

                            // update resource 
                            query = String.Format("UPDATE account SET gold = gold - {0}, wood = wood - {1} WHERE id = {2};", buildDef.req_gold, buildDef.req_wood, account_id);
                            using (MySqlCommand command = new MySqlCommand(query, connection))
                            {
                                command.ExecuteNonQuery();
                            }
                        });
                        Data.Building Building = await GetBuildingAsync(clientID, id);
                        string building = await Data.Serialize<Data.Building>(Building);
                        string build_def = await Data.Serialize<Data.DefineBuilding>(buildDef1);
                        
                        
                        packet.Write(0); // success
                        packet.Write(id);
                        packet.Write(build_def);
                        packet.Write(building);                        
                    }
                    else                                
                        packet.Write(1); // not enough townhall lv                         
                }
                else
                {
                    packet.Write(2); // no resource                
                }

                Sender.TCP_Send(clientID, packet);
                connection.Close();
            }
        }


        // 7----TRAIN----
        public async static void TrainingUnit(int clientId, string unitName, int level)
        {
            Data.Player playerdata = await GetPlayerDataAsync(clientId);
            Data.DefineUnit unitDef = await GetUnitDefineAsync(unitName, level);

            Packet packet = new Packet();
            packet.Write(7);
            if (playerdata.gold >= unitDef.req_gold && playerdata.wood >= unitDef.req_wood)
            {
                long account_id = Server.clients[clientId].account_id;
                int train_time = unitDef.train_time;
                int capacity = await GetBarrackCapacityAsync(account_id);
                int storage = await GetUnitStorageAsync(account_id);
                if (storage >= capacity)
                {
                    packet.Write(1); // full
                }
                else
                {
                    // add or update unit vào sql
                    await Task.Run(() =>
                    {
                        using (MySqlConnection connection = GetMySqlConnection())
                        {
                            string query = String.Format("UPDATE account SET gold = gold - {0}, wood = wood - {1} WHERE id = {2};", unitDef.req_gold, unitDef.req_wood, account_id);
                            using (MySqlCommand command = new MySqlCommand(query, connection))
                            {
                                command.ExecuteNonQuery();
                            }

                            bool found = false;
                            query = String.Format("SELECT * FROM units WHERE account_id = {0} AND name = '{1}';", account_id, unitName);
                            using (MySqlCommand command = new MySqlCommand(query, connection))
                            {
                                using (MySqlDataReader reader = command.ExecuteReader())
                                {
                                    if (reader.HasRows) // id
                                    {
                                        found = true;
                                    }
                                }
                            }

                            if (found)
                            {
                                // chỉ update trained_time khi training = 0
                                query = String.Format("UPDATE units SET level = {0}, trained_time = IF(training = 0, NOW() + INTERVAL {1} SECOND, trained_time), training = training + 1, is_training = 1 WHERE account_id = {2} AND name = '{3}';", level, train_time, account_id, unitName);
                                using (MySqlCommand command = new MySqlCommand(query, connection))
                                {
                                    command.ExecuteNonQuery();
                                }
                            }
                            else
                            {
                                query = String.Format("INSERT INTO units (account_id, name, level, training, ready, trained_time, is_training) VALUES({0}, '{1}', {2}, 1, 0, NOW() + INTERVAL {3} SECOND, 1);", account_id, unitName, level, train_time);
                                using (MySqlCommand command = new MySqlCommand(query, connection))
                                {
                                    command.ExecuteNonQuery();
                                }
                            }
                            connection.Close();
                        }
                    });                    
                    Data.Unit unit = await GetUnitAsync(unitName, level);
                    string unitData = await Data.Serialize<Data.Unit>(unit);
                    packet.Write(0); // sucess
                    packet.Write(unitData);;
                }
            }
            else
            {                
                packet.Write(2); // no resource                
            }
            Sender.TCP_Send(clientId, packet);
        }
        async static Task<Data.Unit> GetUnitAsync(string unitName, int level)
        {
            Task<Data.Unit> task = Task.Run(() =>
            {
                Data.Unit unitdata = new Data.Unit();
                // truy vấn
                using (MySqlConnection connection = GetMySqlConnection())
                {
                    string query = String.Format("SELECT * FROM units WHERE name = '{0}' AND level = {1};", unitName, level);
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    unitdata.name = unitName;
                                    unitdata.level = level;
                                    unitdata.training = int.Parse(reader["training"].ToString());
                                    unitdata.ready = int.Parse(reader["ready"].ToString());
                                    unitdata.trained_time = DateTime.Parse(reader["trained_time"].ToString());
                                    unitdata.is_training = int.Parse(reader["is_training"].ToString()) > 0;

                                }
                            }
                        }
                    }
                    connection.Close();
                }
                return unitdata;
            });
            return await task;
        }
        async static Task<Data.DefineUnit> GetUnitDefineAsync(string unitName, int level)
        {
            Task<Data.DefineUnit> task = Task.Run(() =>
            {
                Data.DefineUnit unitdata = new Data.DefineUnit();
                // truy vấn
                using (MySqlConnection connection = GetMySqlConnection())
                {
                    string query = String.Format("SELECT * FROM define_unit WHERE name = '{0}' AND level = {1};", unitName, level);
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    unitdata.name = unitName;
                                    unitdata.level = level;
                                    unitdata.train_time = int.Parse(reader["train_time"].ToString());
                                    unitdata.req_gold = int.Parse(reader["req_gold"].ToString());
                                    unitdata.req_wood = int.Parse(reader["req_wood"].ToString());
                                    unitdata.req_Barracklv = int.Parse(reader["req_Barracklv"].ToString());
                                    
                                }
                            }
                        }
                    }
                    connection.Close();
                }
                return unitdata;
            });
            return await task;
        }
        async static Task<int> GetBarrackCapacityAsync(long account_id)
        {
            Task<int> task = Task.Run(() =>
            {
                int capacity = 0;
                using (MySqlConnection connection = GetMySqlConnection())
                {
                    string query = String.Format("SELECT sum(db.capacity) AS capa FROM buildings b JOIN define_building db ON b.name = db.name AND b.level = db.level WHERE b.account_id = {0} AND b.name = 'barrack';", account_id);
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    capacity = int.Parse(reader["capa"].ToString());

                                }
                            }
                        }
                    }
                }
                return capacity;
            });
            return await task;
        }
        async static Task<int> GetUnitStorageAsync(long account_id)
        {
            Task<int> task = Task.Run(() =>
            {
                int storage = 0;
                using (MySqlConnection connection = GetMySqlConnection())
                {
                    string query = String.Format("SELECT sum(u.training)+sum(u.ready) AS storage FROM units u JOIN define_unit du ON u.name = du.name AND u.level = du.level WHERE u.account_id = {0};", account_id);
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    storage = int.Parse(reader["storage"].ToString());

                                }
                            }
                        }
                    }
                }
                return storage;
            });
            return await task;
        }

        // 8----CANCEL TRAIN----
        public async static void CancelTrainingUnit(int clientId, string unitName, int req_gold, int req_wood)
        {
            long account_id = Server.clients[clientId].account_id;
            await CancelTrainingUnitAsync(account_id, unitName, req_gold, req_wood);
            Data.Player playerdata = await GetPlayerDataAsync(clientId);

            Packet packet = new Packet();
            packet.Write(8);
            packet.Write(playerdata.gold);
            packet.Write(playerdata.wood);
            Sender.TCP_Send(clientId, packet);
        }
        public async static void DeleteAllTrainingUnit(int clientId)
        {
            // task update resource, units
            await Task.Run(() =>
            {
                long account_id = Server.clients[clientId].account_id;
                using (MySqlConnection connection = GetMySqlConnection())
                {
                    int gold_amount = 0, wood_amount = 0;
                    string query = String.Format("SELECT sum(u.training * du.req_gold) AS gold_amount, sum(u.training * du.req_wood) AS wood_amount FROM units u JOIN define_unit du ON u.name = du.name AND u.level = du.level WHERE u.account_id = {0};", account_id);
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    gold_amount = int.Parse(reader["gold_amount"].ToString());
                                    wood_amount = int.Parse(reader["wood_amount"].ToString());
                                }
                            }
                        }
                    }
                    UpdateResourceAsync(connection, account_id, gold_amount, wood_amount);

                    query = String.Format("UPDATE units SET training = 0, is_training = 0, trained_time = NOW() WHERE account_id = {0} AND training > 0;", account_id);
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                    connection.Close();
                }
            });
            Data.Player playerdata = await GetPlayerDataAsync(clientId);
            Packet packet = new Packet();
            packet.Write(8);
            packet.Write(playerdata.gold);
            packet.Write(playerdata.wood);
            Sender.TCP_Send(clientId, packet);
        }
        async static Task CancelTrainingUnitAsync(long account_id, string unitName, int req_gold, int req_wood)
        {
            await Task.Run(() =>
            {
                using (MySqlConnection connection = GetMySqlConnection())
                {
                    string query = String.Format("UPDATE units SET training = training - 1 WHERE account_id = {0} AND name = '{1}' AND training > 1;", account_id, unitName);
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                    query = String.Format("UPDATE units SET training = 0, trained_time = NOW(), is_training = 0 WHERE account_id = {0} AND name = '{1}' AND training = 1;", account_id, unitName);
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.ExecuteNonQuery();
                    }

                    UpdateResourceAsync(connection, account_id, req_gold, req_wood);
                    connection.Close();
                }
            });
        }
        static void UpdateResourceAsync(MySqlConnection connection, long account_id, int gold_amount, int wood_amount)
        {
            int thresh_hold = 0;
            string query = String.Format("SELECT dt.resource_limit AS thresh_hold FROM define_townhall dt INNER JOIN buildings b ON b.level = dt.level WHERE b.name = 'town hall' AND b.account_id = {0};", account_id);
            using (MySqlCommand command = new MySqlCommand(query, connection))
            {
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            thresh_hold = int.Parse(reader["thresh_hold"].ToString());
                        }
                    }
                }
            }
            query = String.Format("UPDATE account SET gold = LEAST(gold + {0}, {2}), wood = LEAST(wood + {1}, {2}) WHERE id = {3};", gold_amount, wood_amount, thresh_hold, account_id);
            using (MySqlCommand command = new MySqlCommand(query, connection))
            {
                command.ExecuteNonQuery();
            }
        }

        // 9----FIND ENEMY----
        public async static void FindEnemy(int clientId)
        {
            long enemyID = await GetEnemyDataAsync();
            Data.Player enemyData = new Data.Player();
            enemyData.buildings = await GetAllBuildingAsync(enemyID);
            enemyData.Defbuildings = await GetAllBuildingDefAsync(enemyID);
            string enemydata = await Data.Serialize<Data.Player>(enemyData);

            Packet packet = new Packet();
            packet.Write(9);
            packet.Write(enemydata);
            Sender.TCP_Send(clientId, packet);
        }
        async static Task<long> GetEnemyDataAsync()
        {
            Task<long> task = Task.Run(() =>
            {
                long enemy_id = 0;
                using (MySqlConnection connection = GetMySqlConnection())
                {                    
                    string query = String.Format("SELECT id FROM account WHERE is_online = 0 ORDER BY RAND() LIMIT 1;");
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    enemy_id = int.Parse(reader["id"].ToString());
                                }
                            }
                        }
                    }
                    connection.Close();
                }
                return enemy_id;
            });
            return await task;
        }

        // ----UPDATE----
        async static void UpdateCollect(double deltaTime)
        {
            // sau deltaTime cập nhật 1 lần
            await UpdateCollectAsync(deltaTime);
            waiting = false;  // chờ tác vụ hoàn thành mới xét deltaTime tiếp ở hàm update()
        }
        async static Task<bool> UpdateCollectAsync(double deltaTime)
        {
            Task<bool> task = Task.Run(() =>
            {
                using(MySqlConnection connection = GetMySqlConnection())
                {
                    // mining
                    string query = String.Format("UPDATE buildings b INNER JOIN define_building db ON b.name = db.name AND b.level = db.level SET b.storage = b.storage + (db.speed * '{0}')  WHERE b.name IN ('goldmine', 'woodmine') AND is_constructing = 0;", deltaTime/3600d);
                    using(MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                    
                    // giới hạn mine
                    query = String.Format("UPDATE buildings b INNER JOIN define_building db ON b.name = db.name AND b.level = db.level SET b.storage = db.capacity  WHERE b.storage >= db.capacity AND b.name IN ('goldmine', 'woodmine');");
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                    connection.Close();
                }
                return true;
            });
            return await task;
        }
        async static void UpdateConstruct()
        {
            // sau deltaTime cập nhật 1 lần
            await UpdateConstructAsync();
            waiting1 = false;  // chờ tác vụ hoàn thành mới xét deltaTime tiếp ở hàm update()
        }
        async static Task<bool> UpdateConstructAsync()
        {
            Task<bool> task = Task.Run(() =>
            {
                using (MySqlConnection connection = GetMySqlConnection())
                {
                    // construct complete
                    string query = String.Format("UPDATE buildings SET is_constructing = 0 WHERE is_constructing > 0 AND construct_time <= NOW();");
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.ExecuteNonQuery();
                    }

                    
                    connection.Close();
                }
                return true;
            });
            return await task;
        }
        async static void UpdateTraining()
        {
            // sau deltaTime cập nhật 1 lần
            await UpdateTrainingAsync();
            waiting2 = false;  // chờ tác vụ hoàn thành mới xét deltaTime tiếp ở hàm update()
        }
        async static Task<bool> UpdateTrainingAsync()
        {
            Task<bool> task = Task.Run(() =>
            {
                using (MySqlConnection connection = GetMySqlConnection())
                {
                    // construct complete
                    string query = String.Format("UPDATE units SET training = 0, ready = ready + 1, is_training = 0 WHERE trained_time <= NOW() AND training = 1;");
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                    query = String.Format("UPDATE units u INNER JOIN define_unit du ON u.name = du.name AND u.level = du.level SET u.training = u.training - 1, u.ready = u.ready + 1,  u.trained_time = NOW() + INTERVAL du.train_time SECOND WHERE u.training > 1 AND u.trained_time <= NOW();");
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.ExecuteNonQuery();
                    }

                    connection.Close();
                }
                return true;
            });
            return await task;
        }

        #endregion
    }
}