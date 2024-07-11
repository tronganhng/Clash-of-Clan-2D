using UnityEngine;
using UnityEngine.UI;
using Proj2.clashofclan_2d;
using DevelopersHub.RealtimeNetworking.Client;

public class StoreManager : MonoBehaviour
{
    public Text gold_text, wood_text;
    private int gold_cnt, wood_cnt;

    public void Select_Content(int index)
    {         
        gameObject.SetActive(false);  
    }

    private void OnEnable() 
    {
        gold_cnt = ResourceControll.instance.gold_cnt;
        gold_text.text = "" + gold_cnt;
        wood_cnt = ResourceControll.instance.wood_cnt;
        wood_text.text = "" + wood_cnt;
    }
}
