using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MessageSender : MonoBehaviour
{
    public ConnectionManager connectionManager;
    public GameObject messagePrefab;
    public float messageSpeed = 5f;

    public void SendMessage(GameObject source, GameObject destination, string content)
    {
        var sourceDevice = source.GetComponent<NetworkDevice>();
        var destinationDevice = destination.GetComponent<NetworkDevice>();

        if (sourceDevice == null || destinationDevice == null)
        {
            Debug.LogError("Source or destination does not have a NetworkDevice component.");
            return;
        }

        NetworkMessage message = new NetworkMessage(sourceDevice.IPAddress, destinationDevice.IPAddress, content);
        var path = connectionManager.GetPath(source, destination);

        if (path != null)
        {
            StartCoroutine(SendMessageAlongPath(message, path));
        }
        else
        {
            Debug.LogError("No valid path found.");
        }
    }

    private IEnumerator SendMessageAlongPath(NetworkMessage message, List<GameObject> path)
    {
        for (int i = 0; i < path.Count - 1; i++)
        {
            GameObject current = path[i];
            GameObject next = path[i + 1];

            var connection = connectionManager.GetConnection(current, next);
            if (connection != null)
            {
                yield return MoveMessageAlongCable(message, connection.Line);
            }
        }

        Debug.Log("Message successfully delivered!");
    }

    private IEnumerator MoveMessageAlongCable(NetworkMessage message, LineRenderer cable)
    {
        GameObject messageObject = Instantiate(messagePrefab, cable.GetPosition(0), Quaternion.identity);

        float journey = 0f;
        while (journey < 1f)
        {
            journey += Time.deltaTime * messageSpeed;
            Vector3 newPosition = Vector3.Lerp(cable.GetPosition(0), cable.GetPosition(1), journey);
            messageObject.transform.position = newPosition;
            yield return null;
        }

        Destroy(messageObject);
    }
}
