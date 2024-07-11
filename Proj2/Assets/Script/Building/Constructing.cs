using UnityEngine;
using Proj2.clashofclan_2d;
using System;
using UnityEngine.UI;

public class Constructing : MonoBehaviour
{
    public GameObject[] turnoff_obj;
    public GameObject Canvas, Name;
    public Image fillImage;
    public Text timetxt;
    Animator ani;
    double constructing_time = 0; // biến chạy
    int build_time; // biến static
    int level = 0;
    bool waitToSellect = false;
    void Start()
    {
        if (GetComponent<LayerControll>().preview_mode) return;

        ani = GetComponent<Animator>();
        level = transform.parent.GetComponent<BuildingDefineData>().building.level;

        if (transform.parent.GetComponent<BuildingDefineData>().building.is_constructing)
        {
            SetConstruction();
        }
    }

    
    void Update()
    {
        if (GetComponent<LayerControll>().preview_mode) return;

        if (transform.parent.GetComponent<BuildingDefineData>().building.is_constructing)
        {
            fillImage.fillAmount -= Time.deltaTime / (float)build_time;
            int minutes = Mathf.FloorToInt((float)constructing_time / 60); // Lấy số phút
            int seconds = Mathf.FloorToInt((float)constructing_time % 60); // Lấy số giây
            timetxt.text = minutes.ToString("00") + ":" + seconds.ToString("00");
            constructing_time -= Time.deltaTime;
            if(constructing_time <= 0)
            {
                ani.SetBool("construct", false);
                transform.parent.GetComponent<BuildingDefineData>().building.is_constructing = false;
                foreach (GameObject x in turnoff_obj) x.SetActive(true);
                Canvas.SetActive(false);
                transform.parent.GetComponent<BuildingDefineData>().level_txt.text = Name.GetComponent<Text>().text;
            }    
        }

        // Hiện tên
        if (Input.GetMouseButtonDown(0))
        {
            if (IsMouseOver()) waitToSellect = true;
        }

        if(Input.GetMouseButtonUp(0))
        {            
            if (IsMouseOver() && !Name.activeSelf && waitToSellect)
            {
                Name.SetActive(true);
                waitToSellect = false;
            }    
            else if (!IsMouseOver() && Name.activeSelf)
                Name.SetActive(false);
        }    
    }

    private bool IsMouseOver()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0f;
        Collider2D collider = GetComponent<Collider2D>();
        return collider.OverlapPoint(mousePosition);
    }

    public void SetConstruction()
    {
        ani.SetBool("construct", true);
        foreach (GameObject x in turnoff_obj) x.SetActive(false); // tắt 1 số obj khi đang xây
        constructing_time = (transform.parent.GetComponent<BuildingDefineData>().building.construct_time - DateTime.Now).TotalSeconds;
        build_time = transform.parent.GetComponent<BuildingDefineData>().def_build.build_time;
        Canvas.SetActive(true);
        Name.GetComponent<Text>().text = transform.parent.GetComponent<BuildingDefineData>().def_build.buildingName + "\nlevel " + transform.parent.GetComponent<BuildingDefineData>().def_build.level;
        fillImage.fillAmount = (float)constructing_time / build_time;
    }    
}

