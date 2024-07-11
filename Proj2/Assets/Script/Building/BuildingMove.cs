using UnityEngine;
using UnityEngine.EventSystems;
using Proj2.clashofclan_2d;
using DevelopersHub.RealtimeNetworking.Client;
using System.Collections;

namespace Proj2.clashofclan_2d
{
    public class BuildingMove : MonoBehaviour
    {
        public LayerMask placementLayer;
        public bool selected, dragging;
        public int building_index;
        private bool waitToselect;
        Animator ani;
        Grid grid;
        SpriteRenderer sprite;
        Vector3Int distance, startPos;
        private Vector3 lastPosition;

        
        void Start()
        {
            ani = GetComponent<Animator>();
            sprite = GetComponent<SpriteRenderer>();
            grid = GameObject.Find("Grid").GetComponent<Grid>();
        }

        void Update()
        {
            if (PlacementSystem.instance.preview_mode) return;

            if (GetComponent<LayerControll>().preview_mode) return;
            // bật & tắt mode: đang đc chọn
            if (Input.GetMouseButtonDown(0))
            {
                if (!EventSystem.current.IsPointerOverGameObject())
                {
                    if (IsMouseOver())
                    {
                        waitToselect = true;
                    }
                    else
                        selected = false;
                }
            }
            if(Input.GetMouseButtonUp(0) && waitToselect && IsMouseOver())
            {
                selected = true;
                waitToselect = false;
                ani.SetTrigger("select");
            }    

            // chỉnh màu
            if (dragging)
                SpriteColorChange();
            else
                    if (selected)
            {
                sprite.color = Color.white;
            }

            // di chuyển
            DraggingBuilding();
        }

        #region Method
        private void DraggingBuilding()
        {
            if (selected && !dragging)
            {
                Vector2 mousePos = GetMapPosition();
                Vector3Int mousegridPos = grid.WorldToCell(mousePos);
                startPos = grid.WorldToCell(transform.parent.position);
                distance = mousegridPos - startPos;
            }

            if (Input.GetMouseButtonUp(0) && dragging) // đã đặt vào vị trí mới
            {
                dragging = false;
                ani.SetTrigger("select");

                Vector3Int newPos = grid.WorldToCell(transform.parent.position);
                if (!CheckPlacementValidity(newPos, building_index) || MouseOverUI())
                {
                    PlacementSystem.instance.floorData.AddObjectAt((Vector2Int)startPos, PlacementSystem.instance.databaseOS.buildingData[building_index].Size, building_index);
                    transform.parent.position = startPos;
                    return;
                }
                // request to server
                ReplaceRequest((Vector2Int)startPos, (Vector2Int)newPos);
                PlacementSystem.instance.floorData.AddObjectAt((Vector2Int)newPos, PlacementSystem.instance.databaseOS.buildingData[building_index].Size, building_index);
            }

            if (Input.GetMouseButton(0) && selected && IsMouseOver() && !MouseOverUI())  // bắt đầu kéo chuột
            {
                dragging = true;
            }

            if (dragging)
            {
                // xóa vtri cũ khỏi gridData
                PlacementSystem.instance.floorData.DeleteObjectAt((Vector2Int)startPos, PlacementSystem.instance.databaseOS.buildingData[building_index].Size);

                // follow chuột
                Vector2 mousePosition = GetMapPosition();
                Vector3Int gridPos = grid.WorldToCell(mousePosition);
                transform.parent.position = grid.CellToWorld(gridPos - distance);
            }
        }

        void ReplaceRequest(Vector2Int startPos, Vector2Int newPos)
        {
            if (startPos == newPos) return;

            Packet packet = new Packet();
            packet.Write((int)Player.RequestID.REPLACE);
            packet.Write(startPos.x);
            packet.Write(startPos.y);
            packet.Write(newPos.x);
            packet.Write(newPos.y);

            Sender.TCP_Send(packet);
        }

        // đổi màu sprite khi drag
        private void SpriteColorChange()
        {
            Vector3Int gridPos = grid.WorldToCell(transform.parent.position);
            if (!CheckPlacementValidity(gridPos, building_index) || MouseOverUI())
            {
                Color cl = Color.red;
                sprite.color = cl;
            }
            else
            {
                Color cl = Color.white;
                sprite.color = cl;
            }
        }

        // kiểm tra vtrí hợp lệ
        bool CheckPlacementValidity(Vector3Int gridPos, int build_index)
        {
            GridData selectedData = PlacementSystem.instance.floorData;
            Vector2Int gridPos2D = new Vector2Int(gridPos.x, gridPos.y);
            return selectedData.CanPlaceObject(gridPos2D, PlacementSystem.instance.databaseOS.buildingData[build_index].Size);


        }

        // chỉ lấy Position chuột trên cỏ
        public Vector3 GetMapPosition()
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Collider2D hitCollider = Physics2D.OverlapPoint(mousePos, placementLayer);
            if (hitCollider != null)
            {
                // Đối tượng có Layer "place" được nhấp vào
                lastPosition = mousePos;
            }
            return lastPosition;
        }

        private bool IsMouseOver()
        {
            Vector2 mousePosition = GetMapPosition();
            Collider2D collider = GetComponent<Collider2D>();
            return collider.OverlapPoint(mousePosition);
        }

        bool MouseOverUI()
        {
            return EventSystem.current.IsPointerOverGameObject();
        }


        #endregion
    }
}

