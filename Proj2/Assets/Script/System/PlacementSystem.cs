namespace Proj2.clashofclan_2d
{
    using UnityEngine.EventSystems;
    using UnityEngine;
    using System.Collections;
    using DevelopersHub.RealtimeNetworking.Client;

    public class PlacementSystem : MonoBehaviour
    {
        public GameObject cellIndicator, erorTxt, guideTxt;
        public InputManager inputManager;
        public Grid grid;
        public BuildingDatabaseOS databaseOS;
        private int building_index = -1;
        private SpriteRenderer ci_sprite;
        public GridData floorData;
        public Data.Building buildData;
        public Data.DefineBuilding defData;
        public bool response, canPlace, preview_mode = false;

        // khai báo toàn cục
        public static PlacementSystem _instance = null;
        public static PlacementSystem instance { get { return _instance; } }
        void Start()
        {
            _instance = this;
            StopPlacement();
            floorData = new();

        }
        void Update()
        {
            if (building_index < 0)
                return;
            CellindicatorChanging();
        }

        // đổi màu & vị trí cellindicator
        void CellindicatorChanging()
        {
            Vector3 MousePosition = inputManager.GetMapPosition();
            MousePosition.z = 0;
            Vector3Int gridPos = grid.WorldToCell(MousePosition);

            if (!CheckPlacementValidity(gridPos, building_index))
            {
                Color cl = Color.red;
                cl.a = 0.6f;
                ci_sprite.color = cl;
            }
            else
            {
                Color cl = Color.white;
                cl.a = 0.6f;
                ci_sprite.color = cl;
            }
            cellIndicator.transform.position = grid.CellToWorld(gridPos);
        }

        // start process (preview)
        public void StartPlacement(int ID) 
        {
            StopPlacement();
            guideTxt.SetActive(true);
            building_index = ID;
            if (building_index < 0)
            {
                return;
            }

            preview_mode = true;

            cellIndicator = Instantiate(databaseOS.buildingData[building_index].Prefab);
            cellIndicator.GetComponentInChildren<LayerControll>().preview_mode = true;
            ci_sprite = cellIndicator.GetComponentInChildren<SpriteRenderer>();
            Color cl = ci_sprite.color;
            cl.a = 0.6f;
            ci_sprite.color = cl;

            inputManager.OnClicked += PlaceStructure; // gán hàm vào event click chuột
            inputManager.OnExit += StopPlacement;
        }    
        

        // end process
        public void StopPlacement() 
        {            
            guideTxt.SetActive(false);
            building_index = -1;
            //cellIndicator.SetActive(false);
            Destroy(cellIndicator);
            cellIndicator = null;
            preview_mode = false;
            inputManager.OnClicked -= PlaceStructure; // bỏ gán hàm vào event clicked
            inputManager.OnExit -= StopPlacement;
        }

        //spawn obj
        void PlaceStructure() 
        {
            if (EventSystem.current.IsPointerOverGameObject()) 
            {
                StopPlacement();
                return; 
            }

            Vector3 MousePosition = inputManager.GetMapPosition();            
            Vector3Int gridPos = grid.WorldToCell(MousePosition);
            // check hợp lệ:
            if (!CheckPlacementValidity(gridPos, building_index)) return;
            BuildRequest(gridPos.x, gridPos.y);
            StartCoroutine(WaitResponse(gridPos, building_index));
            StopPlacement();            
        }

        //check vtrí hợp lệ
        bool CheckPlacementValidity(Vector3Int gridPos,int build_index)
        {
            GridData selectedData = floorData;
            Vector2Int gridPos2D = new Vector2Int(gridPos.x, gridPos.y);
            return selectedData.CanPlaceObject(gridPos2D, databaseOS.buildingData[build_index].Size);
        }

        // request to server
        void BuildRequest(int posX, int posY)
        {
            Packet packet = new Packet();
            packet.Write((int)Player.RequestID.BUILD);
            packet.Write(SystemInfo.deviceUniqueIdentifier);
            packet.Write(databaseOS.buildingData[building_index].Name);
            packet.Write(posX);
            packet.Write(posY);
            Sender.TCP_Send(packet);
        }    

        private IEnumerator WaitResponse(Vector3Int gridPos, int buid_index)
        {            
            yield return new WaitUntil(() => response);   
            response = false;
            if (!canPlace)
            {
                erorTxt.SetActive(true);
                yield return new WaitForSeconds(0.8f);
                erorTxt.SetActive(false);
            }
            else
            {
                canPlace = false;
                // spawn và setup stat
                GameObject newBuilding = Instantiate(databaseOS.buildingData[buid_index].Prefab);
                newBuilding.GetComponent<BuildingDefineData>().def_build = defData;
                newBuilding.GetComponent<BuildingDefineData>().building = buildData;
                
                Buildings.instance.build_prefab[buildData.id] = newBuilding;
                if (Buildings.instance.build_cnt.ContainsKey(defData.buildingName))
                    Buildings.instance.build_cnt[defData.buildingName] += 1;
                else
                    Buildings.instance.build_cnt[defData.buildingName] = 1;

                newBuilding.transform.position = grid.CellToWorld(gridPos);

                Vector2Int gridPos2D = new Vector2Int(gridPos.x, gridPos.y);
                floorData.AddObjectAt(gridPos2D,
                                         databaseOS.buildingData[buid_index].Size,
                                         databaseOS.buildingData[buid_index].ID);
            }    
        }    
    }
}
