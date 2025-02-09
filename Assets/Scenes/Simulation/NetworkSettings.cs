using UnityEngine;
using System.Collections.Generic;

public class NetworkSettings : MonoBehaviour
{
    public string IPAddress = "0.0.0.0"; // Default IP Address
    public string SubnetMask = "255.255.255.0"; // Default Subnet Mask

    [Header("Port Settings")]
    public int totalPorts = 4; // Total number of LAN ports available
    private List<int> occupiedPorts = new List<int>(); // List of occupied ports

    public bool ConnectToPort(GameObject device)
    {
        if (occupiedPorts.Count < totalPorts)
        {
            occupiedPorts.Add(occupiedPorts.Count + 1); // Assign the next available port
            Debug.Log($"Device {device.name} connected to port {occupiedPorts.Count} on {gameObject.name}");
            return true;
        }
        else
        {
            Debug.LogError($"No available ports on {gameObject.name}");
            return false;
        }
    }

    public void DisconnectFromPort(GameObject device)
    {
        if (occupiedPorts.Count > 0)
        {
            occupiedPorts.RemoveAt(occupiedPorts.Count - 1); // Disconnect the last connected port
            Debug.Log($"Device {device.name} disconnected from {gameObject.name}");
        }
    }

    public int GetAvailablePorts()
    {
        return totalPorts - occupiedPorts.Count; // Return the number of free ports
    }
}
