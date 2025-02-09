using UnityEngine;
using UnityEngine.UI;

public class ModelSelectionManager : MonoBehaviour
{
    public GameObject UIPanel; // Reference to the UI Panel
    public InputField ipInputField;
    public InputField subnetInputField;
    private GameObject selectedModel;

    void Start()
    {
        // Ensure the UI Panel is hidden at the start
        UIPanel.SetActive(false);

        // Add listeners to Input Fields to save data on end editing
        ipInputField.onEndEdit.AddListener(OnIPAddressChanged);
        subnetInputField.onEndEdit.AddListener(OnSubnetMaskChanged);
    }

    void Update()
    {
        // Check if the user clicks on the screen
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                selectedModel = hit.collider.gameObject;
                Debug.Log($"Selected model: {selectedModel.name}");

                // Check if the selected model has a NetworkSettings component
                var networkSettings = selectedModel.GetComponent<NetworkSettings>();
                if (networkSettings != null)
                {
                    // Show the UI Panel
                    UIPanel.SetActive(true);

                    // Populate the Input Fields with the current values
                    ipInputField.text = networkSettings.IPAddress;
                    subnetInputField.text = networkSettings.SubnetMask;
                }
                else
                {
                    Debug.LogWarning("Selected object does not have a NetworkSettings component!");
                }
            }
        }
    }

    // Save IP Address when the field editing ends
    private void OnIPAddressChanged(string newIP)
    {
        if (selectedModel != null)
        {
            var networkSettings = selectedModel.GetComponent<NetworkSettings>();
            if (networkSettings != null)
            {
                networkSettings.IPAddress = newIP;
                Debug.Log($"Updated IP Address to: {newIP}");
            }
        }
    }

    // Save Subnet Mask when the field editing ends
    private void OnSubnetMaskChanged(string newSubnet)
    {
        if (selectedModel != null)
        {
            var networkSettings = selectedModel.GetComponent<NetworkSettings>();
            if (networkSettings != null)
            {
                networkSettings.SubnetMask = newSubnet;
                Debug.Log($"Updated Subnet Mask to: {newSubnet}");
            }
        }
    }

    public void ApplyNetworkSettings()
    {
        if (selectedModel != null)
        {
            var networkSettings = selectedModel.GetComponent<NetworkSettings>();
            if (networkSettings != null)
            {
                Debug.Log($"Finalized settings: IP: {networkSettings.IPAddress}, Subnet: {networkSettings.SubnetMask}");

                // Hide the UI Panel after applying settings (optional)
                UIPanel.SetActive(false);
            }
        }
    }

    public void ClosePanel()
    {
        // Close the panel manually
        UIPanel.SetActive(false);
    }
}
