using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using Proj2.clashofclan_2d;
using DevelopersHub.RealtimeNetworking.Client;

public class BuildingButtonUI : MonoBehaviour
{
    public GameObject[] buttons;
    private bool waitToselect;

    void Update() 
    {
        if (PlacementSystem.instance.preview_mode) return;
        // select & unselect building
        if(Input.GetMouseButtonDown(0))
        {
            if (!IsMouseOver() && buttons[0].activeSelf)
            {
                // tắt thanh công cụ;
                foreach (GameObject but in buttons)
                {
                    but.GetComponent<Animator>().SetTrigger("disappear");
                }
                StartCoroutine(HouseCanvas_off());
            }
            if (!EventSystem.current.IsPointerOverGameObject()) // ko trỏ vào UI
            {
                if(IsMouseOver() && !buttons[0].activeSelf)
                {
                    // bật thanh công cụ
                    if(!GetComponent<LayerControll>().preview_mode) // ko phản ứng khi preview
                    {
                        waitToselect = true;                        
                    }    
                }    
            }
        }
        if (Input.GetMouseButtonUp(0) && waitToselect && IsMouseOver())
        {
            waitToselect = false;
            foreach (GameObject but in buttons)
            {
                but.SetActive(true);
            }
        }
    }

    // check click chuột
    private bool IsMouseOver()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0f;
        Collider2D collider = GetComponent<Collider2D>();
        return collider.OverlapPoint(mousePosition);
    }

    // tắt thanh công cụ
    private IEnumerator HouseCanvas_off()
    {
        yield return new WaitForSeconds(0.15f);
        foreach (GameObject but in buttons)
        {
            but.SetActive(false);
        }
    }

    // bật bảng UI
    public void turnOn_barackUI()
    {
        GameObject create_soilder = GameObject.Find("Barrack UI").transform.GetChild(0).gameObject;
        create_soilder.SetActive(true);
    }

    public void turnOn_UpgradeUI()
    {
        ConfirmUpgradeUI upgradeUI = GameObject.Find("Upgrade UI").transform.GetChild(0).GetComponent<ConfirmUpgradeUI>();
        upgradeUI.gameObject.SetActive(true);
        upgradeUI.id = transform.parent.GetComponent<BuildingDefineData>().building.id;
        
        upgradeUI.goldtxt.text = ""+transform.parent.GetComponent<BuildingDefineData>().def_build.req_gold;
        upgradeUI.woodtxt.text = "" + transform.parent.GetComponent<BuildingDefineData>().def_build.req_wood;
    }       
}
