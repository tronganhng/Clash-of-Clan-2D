using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using Proj2.clashofclan_2d;

public class GameControll : MonoBehaviour
{
    public GameObject[] char_prefab;
    public Texture2D cursorTexture;
    public CursorMode cursormode = CursorMode.Auto;
    public Vector2 hotSpot = Vector2.zero;
    public Camera mainCamera;
    public GameObject floor;
    void Start()
    {
        //unit_cnt = GetComponent<Unit_Quantity>();
        Cursor.SetCursor(cursorTexture, hotSpot, cursormode);
    }

    void Update() 
    {
        if(Input.GetMouseButtonDown(0) && UnitBattle.instance.char_select >= 0)
        {            
            Vector3 real_pos = mainCamera.ScreenToWorldPoint(Input.mousePosition); // chỉnh pos về camera
            Vector3 spawn_pos = new Vector3(real_pos.x, real_pos.y, 0);
            Vector3Int cellPosition = floor.GetComponent<Tilemap>().WorldToCell(real_pos); // vị trí chuột trên tilemap

            
            if(!IsMouseOverUI()) // check đã chọn nv và ko trỏ chuột lên UI
            {
                string unitName = UnitBattle.instance.unitDataOS.unitData[UnitBattle.instance.char_select].Name;
                if(Units.instance.units[unitName].ready > 0)
                {
                    if(IsMouseOverTile(floor, cellPosition))
                    {
                        GameObject newobj = Instantiate(char_prefab[UnitBattle.instance.char_select], spawn_pos, Quaternion.identity);
                        Units.instance.units[unitName].ready--;
                    }                  
                }
            }         
        }    
    }

    private bool IsMouseOverUI()
    {
        return EventSystem.current.IsPointerOverGameObject(); // ktra xem chuột đang trỏ đến đối tượng UI
    }

    private bool IsMouseOverTile(GameObject floor,Vector3Int cellPosition)
    {
        TileBase tile = floor.GetComponent<Tilemap>().GetTile(cellPosition); // lấy ra 1 ô từ vị trí cellPos đã có
        return tile != null; // trả về true nếu có tile
    }
}
