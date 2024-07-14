using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using System.Collections;
using Proj2.clashofclan_2d;
using DevelopersHub.RealtimeNetworking.Client;

public class CombatSystem : MonoBehaviour
{
    public UnitDatabaseOS unitDataOS;
    public Camera mainCamera;
    public GameObject floor, scene, end_UI;
    public LayerMask GroundLayer;
    public Text time;
    public Button Home,Home2, Next;
    public int battle_time = 100;
    float currentTime = 0;
    public bool is_battle = false;
    private void Start()
    {
        time.enabled = false;
        currentTime = battle_time;
        Home.onClick.AddListener(() => LoadScene(0));
        Next.onClick.AddListener(() => LoadScene(1));
        Home2.onClick.AddListener(() => LoadScene(0));
    }

    void Update() 
    {
        SpawnUnit();
        if (is_battle)
        {
            Home.gameObject.SetActive(false);
            Next.gameObject.SetActive(false);
            time.enabled = true;
            currentTime -= Time.deltaTime;
            time.text = (int)currentTime + "s";
            if (currentTime <= 0)
            {
                is_battle = false;
                end_UI.SetActive(true);
                time.text = "END";
            }
        }
    }

    void SpawnUnit()
    {
        if (Input.GetMouseButtonDown(0) && UnitBattle.instance.char_select >= 0)
        {
            Vector3 real_pos = mainCamera.ScreenToWorldPoint(Input.mousePosition); // chỉnh pos về camera
            Vector3 spawn_pos = new Vector3(real_pos.x, real_pos.y, 0);
            Vector3Int cellPosition = floor.GetComponent<Tilemap>().WorldToCell(real_pos); // vị trí chuột trên tilemap
            if (Physics2D.OverlapPoint(spawn_pos, GroundLayer)) return;

            if (!IsMouseOverUI()) // check đã chọn nv và ko trỏ chuột lên UI
            {
                string unitName = UnitBattle.instance.unitDataOS.unitData[UnitBattle.instance.char_select].Name;
                if (Units.instance.units[unitName].ready > 0)
                {
                    if (IsMouseOverTile(floor, cellPosition))
                    {
                        if (!is_battle) is_battle = true;
                        GameObject newobj = Instantiate(unitDataOS.unitData[UnitBattle.instance.char_select].Prefab, spawn_pos, Quaternion.identity);
                        newobj.GetComponent<UnitStats>().stat = Units.instance.def_unit[unitName];
                        Units.instance.units[unitName].ready--;
                        UnitBattle.instance.battle_unit[unitName].GetComponentInChildren<Text>().text = "x" + Units.instance.units[unitName].ready;
                        Packet packet = new Packet();
                        packet.Write(12); // UPDATE UNIT
                        packet.Write(unitName);
                        Sender.TCP_Send(packet);
                    }
                }
            }
            else
            {
                UnitBattle.instance.char_select = -1;
            }
        }
    }

    private bool IsMouseOverUI()
    {
        return EventSystem.current.IsPointerOverGameObject(); // ktra xem chuột đang trỏ đến đối tượng UI
    }

    private bool IsMouseOverTile(GameObject floor,Vector3Int cellPosition)
    {
        TileBase tile = floor.GetComponent<Tilemap>().GetTile(cellPosition); // lấy ra 1 ô từ vị trí cellPos đã có
        return tile != null; // trả về true nếu có tile
    }

    public void LoadScene(int scene_index)
    {
        RealtimeNetworking.OnPacketReceived -= Battle.instance.ReceivedPacket;
        scene.GetComponent<Animator>().SetTrigger("transition");
        StartCoroutine(loadLevel(scene_index));
    }

    IEnumerator loadLevel(int scene_index)
    {
        yield return new WaitForSeconds(1.5f);
        SceneManager.LoadScene(scene_index);
    }
}
