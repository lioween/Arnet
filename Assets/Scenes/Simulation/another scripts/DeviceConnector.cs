using UnityEngine;

public class DeviceConnector : MonoBehaviour
{
    public ConnectionManager connectionManager; // Reference to the ConnectionManager
    private GameObject sourceDevice; // The device where the connection starts
    private LineRenderer tempLineRenderer; // Temporary visual for the dragging connection

    // List of valid tags for devices
    private readonly string[] validTags = { "Device", "Router", "Switch", "PC" };

    void Update()
    {
        // Start dragging the connection
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider != null && IsValidDevice(hit.collider.gameObject)) // Ensure it's a valid device
                {
                    sourceDevice = hit.collider.gameObject;

                    // Create a temporary line renderer for visual feedback
                    GameObject tempLine = new GameObject("TempLine");
                    tempLineRenderer = tempLine.AddComponent<LineRenderer>();
                    tempLineRenderer.positionCount = 2;
                    tempLineRenderer.startWidth = 0.02f;
                    tempLineRenderer.endWidth = 0.02f;
                    tempLineRenderer.material = new Material(Shader.Find("Unlit/Color")) { color = Color.yellow };

                    tempLineRenderer.SetPosition(0, sourceDevice.transform.position);
                    tempLineRenderer.SetPosition(1, sourceDevice.transform.position); // Start at the same position
                }
            }
        }

        // Update the line position while dragging
        if (Input.GetMouseButton(0) && tempLineRenderer != null)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                tempLineRenderer.SetPosition(1, hit.point); // Update the second point to the current mouse position
            }
            else
            {
                tempLineRenderer.SetPosition(1, ray.GetPoint(10)); // Extend the line outward if no object is hit
            }
        }

        // Complete the connection
        if (Input.GetMouseButtonUp(0) && tempLineRenderer != null)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider != null && IsValidDevice(hit.collider.gameObject) && hit.collider.gameObject != sourceDevice)
                {
                    GameObject targetDevice = hit.collider.gameObject;

                    // Check for a valid connection through ConnectionManager
                    if (connectionManager != null && !connectionManager.ConnectionExists(sourceDevice, targetDevice))
                    {
                        connectionManager.CreateConnection(sourceDevice, targetDevice);
                    }
                    else
                    {
                        Debug.LogWarning("Connection already exists or connection manager is missing.");
                    }
                }
            }

            // Destroy the temporary line
            Destroy(tempLineRenderer.gameObject);
        }
    }

    // Helper method to check if the GameObject has a valid tag
    private bool IsValidDevice(GameObject obj)
    {
        foreach (string tag in validTags)
        {
            if (obj.CompareTag(tag))
            {
                return true;
            }
        }
        return false;
    }
}
