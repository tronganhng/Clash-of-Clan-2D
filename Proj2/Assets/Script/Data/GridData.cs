using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Proj2.clashofclan_2d;

public class GridData
{
    Dictionary<Vector2Int, PlacementData> placedObject = new();     // Dictionary: cặp key-value
    // mỗi Dictionary trên lưu vị trí các ô chiếm dụng của Obj

    // thêm obj vào gridData
    public void AddObjectAt(Vector2Int GridPos, Vector2Int objSize, int ID) 
    {
        // GridPosition: vị trí down-left của prefab
        List<Vector2Int> PosToOccupy = CalculatePosition(GridPos, objSize); // list ô sẽ chiếm dụng 
        PlacementData data = new PlacementData(PosToOccupy, ID); // import vào Data

        foreach (var pos in PosToOccupy)
        {
            if(!placedObject.ContainsKey(pos)) // check các ô đã chiếm có trùng vs ô của obj đang đặt ko
                //throw new Exception("Dictionary already contain"); // báo lỗi & out vòng lặp
                placedObject[pos] = data; // đặt obj & lưu vào GridData
        }
    }

    // xóa object khỏi gridData
    public void DeleteObjectAt(Vector2Int GridPos, Vector2Int objSize)
    {       
        List<Vector2Int> PosToDelete = CalculatePosition(GridPos, objSize); // list ô sẽ xóa 
        //PlacementData data = new PlacementData(PosToOccupy, ID, placedObjectIndex);

        foreach (var pos in PosToDelete)
        {           
            placedObject.Remove(pos); // đặt obj & lưu vào GridData
        }
    }

    // trả về các ô sẽ chiếm dụng
    private List<Vector2Int> CalculatePosition(Vector2Int gridPos, Vector2Int objSize) 
    {
        List<Vector2Int> returnValue = new();
        for(int x = 0; x < objSize.x; x++){
            for(int y = 0; y < objSize.y; y++){
                returnValue.Add(gridPos + new Vector2Int(x, y));  // add các ô sẽ chiếm vào tập gtri trả về
            } 
        }
        return returnValue;
    }

    // check xem có đặt đc Obj ko
    public bool CanPlaceObject(Vector2Int gridPos, Vector2Int objSize) 
    {
        List<Vector2Int> PosToOccupy = CalculatePosition(gridPos, objSize);
        foreach (var pos in PosToOccupy)
        {
            if(placedObject.ContainsKey(pos)) 
                return false;
        }
        return true;
    } 
}

public class PlacementData // Lưu data về các ô bị chiếm của 1 Obj
{
    public List<Vector2Int> occupiedPosition;   // các vtri(ô) bị chiếm dụng bởi 1 obj
    public int ID { get; private set; }   // ID obj

    public PlacementData(List<Vector2Int> occupiedPosition, int iD) // gán gtri vào Data
    {
        this.occupiedPosition = occupiedPosition;
        ID = iD;       
    }
}
