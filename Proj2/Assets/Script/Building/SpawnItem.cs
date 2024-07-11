using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnItem : MonoBehaviour
{
    public GameObject Item;
    public Transform spawn_pos;
    void Spawnitem() 
    {
        Instantiate(Item, spawn_pos.position, Quaternion.identity);
    }
}
