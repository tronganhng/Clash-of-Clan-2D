using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Proj2.clashofclan_2d;
using DevelopersHub.RealtimeNetworking.Client;
using System.Collections;

public class LoadScene : MonoBehaviour
{
    public Text nofication;
    public GameObject loadscene;
    public void LoadBattleScene()
    {
        int ready_amout = 0;
        foreach(KeyValuePair<string, Data.Unit> kvp in Units.instance.units)
        {
            ready_amout += kvp.Value.ready;
        }
        if(ready_amout > 0)
        {
            RealtimeNetworking.OnPacketReceived -= Player.instance.ReceivedPacket;
            loadscene.GetComponent<Animator>().SetTrigger("transition");
            StartCoroutine(loadLevel(1));
        }
        else
        {
            nofication.text = "There is no troop!";
            nofication.gameObject.SetActive(true);
            StartCoroutine(TurnOffNofication());
        }
    }

    IEnumerator TurnOffNofication()
    {
        yield return new WaitForSeconds(1.4f);
        nofication.gameObject.SetActive(false);
    }

    IEnumerator loadLevel(int scene_index)
    {
        yield return new WaitForSeconds(1.5f);
        SceneManager.LoadScene(scene_index);
    }
}
