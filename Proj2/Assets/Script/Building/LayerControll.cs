using UnityEngine;
using DevelopersHub.RealtimeNetworking.Client;
using Proj2.clashofclan_2d;
public class LayerControll : MonoBehaviour
{
    public GameObject broken_building, Item, explosion;
    public Transform Spawn_pos;    
    public bool preview_mode;

    void BuildingBroke()
    {       
        broken_building.SetActive(true);
    }

    void Explosions()
    {
        Vector3 expolode_pos = new Vector3(transform.position.x, transform.position.y + 1f, 0f);
        GameObject Eff = Instantiate(explosion, expolode_pos, transform.rotation);
        Destroy(Eff, 1f);
    }

    void SpawnItem()
    {
        GameObject item = Instantiate(Item, Spawn_pos.position, Quaternion.identity);
        BuildingDefineData buildDef = GetComponentInParent<BuildingDefineData>();
        item.GetComponent<ItemMove>().quantity = (int)buildDef.building.storage;
        if(buildDef.def_build.buildingName == "goldmine")
           EnemyResource.instance.gold -= (int)buildDef.building.storage;    
        else if(buildDef.def_build.buildingName == "woodmine")
            EnemyResource.instance.wood -= (int)buildDef.building.storage;
        EnemyResource.instance.SetAllItemCount();
        Packet packet = new Packet();
        packet.Write(11); // xóa storage
        packet.Write(buildDef.building.id);
        Sender.TCP_Send(packet);
        buildDef.building.storage=0; 
    }
}
