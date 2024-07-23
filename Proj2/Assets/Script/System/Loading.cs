using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine;
using System.Collections;
using DevelopersHub.RealtimeNetworking.Client;

public class Loading : MonoBehaviour
{
    [SerializeField] GameObject DisconnectUI;
    [SerializeField] Image ProgressFill;
    bool done = false;
    void Start()
    {        
        ProgressFill.fillAmount = 0;
        ConnectToServer();
    }

    public void ConnectToServer()
    {
        RealtimeNetworking.OnConnectingToServerResult += ConnectionResponse;
        RealtimeNetworking.Connect();
    }

    void ConnectionResponse(bool sucessful)
    {
        RealtimeNetworking.OnConnectingToServerResult -= ConnectionResponse;
        // sucessful: kết quả lắng nghe
        if (sucessful)
        {
            StartCoroutine(StartLoading());
        }
        else
        {
            // fail -> reconnect
            Debug.Log("Cant connect to Server!");
            DisconnectUI.SetActive(true);   
        }
        
    }

    /*void DisconnectedFromServer()
    {
        DisconnectUI.SetActive(true);
        Debug.Log("Disconnected!");
        RealtimeNetworking.OnDisconnectedFromServer -= DisconnectedFromServer;
    }*/

    IEnumerator StartLoading()
    {        
        AsyncOperation async = SceneManager.LoadSceneAsync(1);
        while (!async.isDone && !done)
        {
            float progress = Mathf.Clamp01(async.progress / 0.9f);
            ProgressFill.fillAmount = progress;
            if (async.progress >= 0.9f)
            {
                done = true;
            }
            yield return null;
        }
        ProgressFill.fillAmount = 1;
        async.allowSceneActivation = true;
    }

    public void Reconnect()
    {
        DisconnectUI.SetActive(false);
        ProgressFill.fillAmount = 0;
        ConnectToServer();
    }
}
