using System.Collections.Generic;
using UnityEngine;

public class ConnectionManager : MonoBehaviour
{
    public GameObject connectionPrefab; // Prefab for visualizing connections
    private List<Connection> connections = new List<Connection>();

    public void CreateConnection(GameObject source, GameObject target)
    {
        if (source != null && target != null)
        {
            if (ConnectionExists(source, target))
            {
                Debug.LogWarning($"⚠️ Connection already exists between {source.name} and {target.name}.");
                return;
            }

            // Create a visual connection (cable)
            GameObject connectionObject = Instantiate(connectionPrefab);
            LineRenderer lineRenderer = connectionObject.GetComponent<LineRenderer>();
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, source.transform.position);
            lineRenderer.SetPosition(1, target.transform.position);

            connections.Add(new Connection(source, target, connectionObject, lineRenderer));
            Debug.Log($"✅ Connection created: {source.name} <--> {target.name}");
        }
        else
        {
            Debug.LogError("❌ Error: Source or target device is null.");
        }
    }

    public bool ConnectionExists(GameObject source, GameObject target)
    {
        return connections.Exists(c => (c.Source == source && c.Target == target) || (c.Source == target && c.Target == source));
    }

    public Connection GetConnection(GameObject source, GameObject target)
    {
        return connections.Find(c => (c.Source == source && c.Target == target) || (c.Source == target && c.Target == source));
    }

    public List<GameObject> GetPath(GameObject source, GameObject destination)
    {
        if (source == null || destination == null)
        {
            Debug.LogError("❌ Error: Source or destination is null.");
            return null;
        }

        List<GameObject> path = new List<GameObject>();
        HashSet<GameObject> visited = new HashSet<GameObject>();

        if (FindPathRecursive(source, destination, visited, path))
        {
            Debug.Log($"🔗 Path Found: {string.Join(" -> ", path.ConvertAll(obj => obj.name))}");
            return path;
        }

        Debug.LogError($"❌ No path found between {source.name} and {destination.name}.");
        return null;
    }

    private bool FindPathRecursive(GameObject current, GameObject destination, HashSet<GameObject> visited, List<GameObject> path)
    {
        if (visited.Contains(current)) return false;

        visited.Add(current);
        path.Add(current);

        if (current == destination) return true;

        foreach (var connection in connections)
        {
            GameObject neighbor = null;

            if (connection.Source == current) neighbor = connection.Target;
            else if (connection.Target == current) neighbor = connection.Source;

            if (neighbor != null && FindPathRecursive(neighbor, destination, visited, path))
            {
                return true;
            }
        }

        path.Remove(current);
        return false;
    }

    public GameObject GetNextHop(GameObject currentDevice, string targetIP)
    {
        GameObject targetDevice = FindDeviceByIP(targetIP);
        if (targetDevice == null)
        {
            Debug.LogError($"❌ No device found with IP {targetIP}");
            return null;
        }

        var path = GetPath(currentDevice, targetDevice);
        if (path != null && path.Count > 1)
        {
            return path[1]; // Return the next hop in the path
        }
        return null;
    }

    public GameObject FindDeviceByIP(string ip)
    {
        NetworkDevice[] allDevices = FindObjectsOfType<NetworkDevice>();
        foreach (var device in allDevices)
        {
            if (device.IPAddress == ip)
            {
                return device.gameObject;
            }
        }
        return null;
    }
}

[System.Serializable]
public class Connection
{
    public GameObject Source;
    public GameObject Target;
    public GameObject ConnectionObject;
    public LineRenderer Line;

    public Connection(GameObject source, GameObject target, GameObject connectionObject, LineRenderer line)
    {
        Source = source;
        Target = target;
        ConnectionObject = connectionObject;
        Line = line;
    }
}
