using UnityEngine;

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
        Instantiate(Item, Spawn_pos.position, Quaternion.identity);
    }
}
