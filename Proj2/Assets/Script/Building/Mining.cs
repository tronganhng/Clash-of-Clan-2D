using UnityEngine;
using UnityEngine.UI;
using Proj2.clashofclan_2d;
using DevelopersHub.RealtimeNetworking.Client;

public class Mining : MonoBehaviour
{
    public Button collectBut;
    int id;
    // Start is called before the first frame update
    void Start()
    {
        id = transform.parent.GetComponent<BuildingDefineData>().building.id;        
    }

    void Update()
    {
        if (GetComponent<LayerControll>().preview_mode) return;
        
        if (transform.parent.GetComponent<BuildingDefineData>().building.storage >= Data.minGoldcollect)
        {
            collectBut.interactable = true;
        }
        else
            collectBut.interactable = false;            
    }

    public void Collect()
    {        
        Packet packet = new Packet();
        packet.Write(5);
        packet.Write(transform.parent.GetComponent<BuildingDefineData>().building.buildingName);
        Sender.TCP_Send(packet);

        foreach(int id in Buildings.instance.build_prefab.Keys)
        {
            Buildings.instance.build_prefab[id].GetComponent<BuildingDefineData>().building.storage = 0; 
        }    
    }    
}
