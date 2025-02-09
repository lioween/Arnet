using UnityEngine;
using UnityEngine.UI;

public class NetworkDevice : MonoBehaviour
{
    public string IPAddress = "0.0.0.0";
    public string SubnetMask = "255.255.255.0";
    public bool IsRouter;
    public bool IsSwitch;
    public bool IsPC;
    public int availablePorts = 4;
    public Text monitorScreen; // ✅ Add UI reference for the monitor

    public int GetAvailablePorts()
    {
        return availablePorts;
    }

    public void DisplayOnMonitor(string message)
    {
        if (monitorScreen != null)
        {
            monitorScreen.text += "\n" + message;
        }
    }

    public void ProcessMessage(NetworkMessage message, ConnectionManager connectionManager)
    {
        Debug.Log($"{gameObject.name} received message: {message.Content}");

        if (message.DestinationIP == IPAddress)
        {
            Debug.Log($"📩 {gameObject.name} received message from {message.SenderIP}: {message.Content}");
            DisplayOnMonitor($"📩 Received from {message.SenderIP}: {message.Content}"); // ✅ Display on monitor
        }
        else
        {
            if (IsRouter || IsSwitch)
            {
                Debug.Log($"{gameObject.name} is forwarding message...");
                GameObject nextDevice = connectionManager.GetNextHop(gameObject, message.DestinationIP);
                if (nextDevice != null)
                {
                    nextDevice.GetComponent<NetworkDevice>()?.ProcessMessage(message, connectionManager);
                }
                else
                {
                    Debug.LogError($"{gameObject.name} cannot find next hop for {message.DestinationIP}.");
                }
            }
            else
            {
                Debug.Log($"{gameObject.name} is a PC and cannot forward messages.");
            }
        }
    }
}
