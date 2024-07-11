using UnityEngine;
using UnityEngine.UI;
using Proj2.clashofclan_2d;
using System;
using DevelopersHub.RealtimeNetworking.Client;

public class Train_unit : MonoBehaviour
{
    public Image avatar, fill_cnt;
    public Text time_cooldown, quantity_txt; // tgian huấn luyện, số lượng sẽ huẩn luyện
    public Sprite sprite;
    bool isCooldown;
    private float current_time;
    public int train_cooldown;
    public string unitName;

    void Start()
    {
        fill_cnt.fillAmount = 0; // setup tgian training 1 unit 
        isCooldown = true;
        avatar.sprite = sprite; // gán avatar cho unit
        transform.localScale = new Vector3(1,1,1);
        current_time = train_cooldown;
    }

    private void OnEnable() //đếm khi UI đc bật
    {
        if (!Units.instance.units.ContainsKey(unitName)) return;
        current_time = (float)(Units.instance.units[unitName].trained_time - DateTime.Now).TotalSeconds;
        fill_cnt.fillAmount = (train_cooldown - current_time) / train_cooldown;
        quantity_txt.text = "x" + Units.instance.units[unitName].training;
    }

    void Update()
    {
        if(isCooldown) 
        {
            fill_cnt.fillAmount += Time.deltaTime/train_cooldown;   // cooldown = fill image
            current_time -= Time.deltaTime;     // cooldown = text
            time_cooldown.text = (int)current_time + "s";
            if(fill_cnt.fillAmount >= 1)
            {
                Units.instance.units[unitName].training--;// giảm số lượng sẽ huấn luyện 1
                Units.instance.units[unitName].ready++;// tăng số lượng unit lên 1
                fill_cnt.fillAmount = 1;
                isCooldown = false;

                if(Units.instance.units[unitName].training <= 0) // huấn luyện đủ  
                    Destroy(gameObject);
                else // chưa huấn luyện đủ
                {
                    quantity_txt.text = "x" + Units.instance.units[unitName].training;
                    fill_cnt.fillAmount = 0;
                    current_time = train_cooldown; 
                    isCooldown = true;
                }
            }
        }
    }

    public void Reduce_but() //giảm số lính huấn luyện
    {
        Create_soldier.instance.occupy--;
        Create_soldier.instance.slot.text = Create_soldier.instance.occupy + "/" + Create_soldier.instance.barrack_capa;
        Units.instance.units[unitName].training--;
        if (Units.instance.units[unitName].training <= 0) // huấn luyện đủ  
        {
            Destroy(gameObject);
            Units.instance.units[unitName].is_training = false;
            Units.instance.units[unitName].trained_time = DateTime.Now;
        }
        else // chưa huấn luyện đủ
        {
            quantity_txt.text = "x" + Units.instance.units[unitName].training;
        }
        Packet packet = new Packet();
        packet.Write(8);
        packet.Write(unitName);
        packet.Write(Units.instance.def_unit[unitName].req_gold);
        packet.Write(Units.instance.def_unit[unitName].req_wood);
        Sender.TCP_Send(packet);
    }
}
