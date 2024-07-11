using UnityEngine;
using UnityEngine.UI;
using DevelopersHub.RealtimeNetworking.Client;

public class ConfirmUpgradeUI : MonoBehaviour
{
    public int id = 0;
    public Text goldtxt, woodtxt;

    public void UpgradeRequest()
    {
        Packet packet = new Packet();
        packet.Write(6);
        packet.Write(id);
        Sender.TCP_Send(packet);
    }
}
