using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using Proj2.clashofclan_2d;

public class TilemapCellPos : MonoBehaviour
{
    public Tilemap tilemap;

    private void Start()
    {        
        StartCoroutine(AddTileinGridData());
    }

    // chuyển các ô của tilemap vào gridData
    IEnumerator AddTileinGridData()
    {
        yield return new WaitForSeconds(.5f);

        // Lấy tất cả ô trong Tilemap
        BoundsInt bounds = tilemap.cellBounds;

        // Lặp qua từng ô trong Tilemap
        foreach (Vector3Int position in bounds.allPositionsWithin)
        {
            // Kiểm tra xem ô có Tile không
            if (tilemap.HasTile(position))
            {
                Vector3 cellCenterWorld = tilemap.GetCellCenterWorld(position);
                cellCenterWorld -= new Vector3(0.5f, 0.5f, 0);  // trừ hao độ lệch
                Vector3Int intcellPos = Vector3Int.RoundToInt(cellCenterWorld);
                Vector2Int size = new Vector2Int(1, 1);
                PlacementSystem.instance.floorData.AddObjectAt((Vector2Int)intcellPos, size, 10);
            }
        }
    }
}
