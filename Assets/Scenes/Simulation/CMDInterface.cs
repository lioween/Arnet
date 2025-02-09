using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class CMDInterface : MonoBehaviour
{
    public GameObject UIPanel;
    public GameObject optionsPanel;
    public InputField commandInputField;
    public Text outputText;
    public ConnectionManager connectionManager;
    public GameObject pingPrefab;
    public float pingSpeed = 5f;
    private GameObject selectedModel;
    private string ipAddress = "Not Set";
    private string subnetMask = "Not Set";

    void Start()
    {
        UIPanel.SetActive(false);
        optionsPanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                selectedModel = hit.collider.gameObject;
                ShowOptionsPanel();
            }
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            OnSubmitCommand();
        }
    }

    private void ShowOptionsPanel()
    {
        UIPanel.SetActive(false);
        optionsPanel.SetActive(true);
    }

    public void OpenCMDPanel()
    {
        if (selectedModel != null)
        {
            optionsPanel.SetActive(false);
            UIPanel.SetActive(true);

            var networkSettings = selectedModel.GetComponent<NetworkDevice>();
            if (networkSettings != null)
            {
                ipAddress = networkSettings.IPAddress;
                subnetMask = networkSettings.SubnetMask;
                outputText.text = "";
                AppendOutput($"Device: {selectedModel.name}");
                AppendOutput($"IP: {ipAddress}");
                AppendOutput($"Subnet: {subnetMask}");
            }
        }
    }

    public void OnSubmitCommand()
    {
        string command = commandInputField.text.Trim();
        if (!string.IsNullOrEmpty(command))
        {
            ProcessCommand(command);
            commandInputField.text = "";
        }
    }

    private void ProcessCommand(string command)
    {
        string[] parts = command.Split(' ');
        string mainCommand = parts[0].ToLower();

        switch (mainCommand)
        {
            case "help":
                AppendOutput("Available commands:");
                AppendOutput("setip [IP] - Set IP Address");
                AppendOutput("setsubnet [Subnet] - Set Subnet Mask");
                AppendOutput("ping [TargetIP] - Ping another device through network");
                AppendOutput("sendmsg [TargetIP] [Message] - Send a message to another device");
                AppendOutput("clear - Clear the output");
                break;

            case "setip":
                if (parts.Length > 1) SetIPAddress(parts[1]);
                else AppendOutput("Error: Provide an IP Address.");
                break;

            case "setsubnet":
                if (parts.Length > 1) SetSubnetMask(parts[1]);
                else AppendOutput("Error: Provide a Subnet Mask.");
                break;

            case "ping":
                if (parts.Length > 1) PingModel(parts[1]);
                else AppendOutput("Error: Provide a target IP address.");
                break;

            case "sendmsg":
                if (parts.Length > 2)
                {
                    string targetIP = parts[1];
                    string messageContent = string.Join(" ", parts, 2, parts.Length - 2);
                    SendMessageToDevice(targetIP, messageContent);
                }
                else
                {
                    AppendOutput("Error: Provide a target IP and a message.");
                }
                break;

            case "clear":
                outputText.text = "";
                break;

            default:
                AppendOutput($"Unknown command: {mainCommand}");
                break;
        }
    }

    private void SetIPAddress(string newIP)
    {
        if (selectedModel != null)
        {
            var networkSettings = selectedModel.GetComponent<NetworkDevice>();
            if (networkSettings != null)
            {
                networkSettings.IPAddress = newIP;
                ipAddress = newIP;
                AppendOutput($"IP Address set to: {ipAddress}");
            }
        }
        else AppendOutput("Error: No device selected.");
    }

    private void SetSubnetMask(string newSubnet)
    {
        if (selectedModel != null)
        {
            var networkSettings = selectedModel.GetComponent<NetworkDevice>();
            if (networkSettings != null)
            {
                networkSettings.SubnetMask = newSubnet;
                subnetMask = newSubnet;
                AppendOutput($"Subnet Mask set to: {subnetMask}");
            }
        }
        else AppendOutput("Error: No device selected.");
    }

    private void PingModel(string targetIP)
    {
        if (selectedModel == null)
        {
            AppendOutput("Error: No device selected to initiate ping.");
            return;
        }

        var targetDevice = connectionManager.FindDeviceByIP(targetIP);
        if (targetDevice == null)
        {
            AppendOutput($"Ping failed: Device with IP {targetIP} not found.");
            return;
        }

        var path = connectionManager.GetPath(selectedModel, targetDevice);
        if (path != null)
        {
            StartCoroutine(SendPingAlongPath(path));
            AppendOutput($"Reply from {targetIP}: bytes=32 time=32ms TTL=64");
        }
        else
        {
            AppendOutput($"Ping failed: No route to {targetIP}.");
        }
    }

    private IEnumerator SendPingAlongPath(List<GameObject> path)
    {
        for (int i = 0; i < path.Count - 1; i++)
        {
            var connection = connectionManager.GetConnection(path[i], path[i + 1]);
            if (connection != null) yield return StartCoroutine(SendMessageAlongCable(connection.Line));
        }
    }

    private void SendMessageToDevice(string targetIP, string messageContent)
    {
        if (selectedModel == null)
        {
            AppendOutput("Error: No device selected to send the message.");
            return;
        }

        var targetDevice = connectionManager.FindDeviceByIP(targetIP);
        if (targetDevice == null)
        {
            AppendOutput($"Message failed: Device with IP {targetIP} not found.");
            return;
        }

        var path = connectionManager.GetPath(selectedModel, targetDevice);
        if (path != null)
        {
            NetworkMessage message = new NetworkMessage(ipAddress, targetIP, messageContent);

            AppendOutput($"📤 Sent to {targetIP}: \"{messageContent}\"");
            selectedModel.GetComponent<NetworkDevice>()?.DisplayOnMonitor($"📤 Sent to {targetIP}: \"{messageContent}\""); // ✅ Display on sender's PC monitor

            StartCoroutine(SendMessageAlongPath(path, message));
        }
        else
        {
            AppendOutput($"Message failed: No route to {targetIP}.");
        }
    }


    private IEnumerator SendMessageAlongPath(List<GameObject> path, NetworkMessage message)
    {
        for (int i = 0; i < path.Count - 1; i++)
        {
            var connection = connectionManager.GetConnection(path[i], path[i + 1]);
            if (connection != null) yield return StartCoroutine(SendMessageAlongCable(connection.Line));
        }

        GameObject finalDevice = path[path.Count - 1];
        finalDevice.GetComponent<NetworkDevice>()?.ProcessMessage(message, connectionManager);
    }

    private IEnumerator SendMessageAlongCable(LineRenderer cable)
    {
        GameObject messageObject = Instantiate(pingPrefab, cable.GetPosition(0), Quaternion.identity);
        float journey = 0f;

        while (journey < 1f)
        {
            journey += Time.deltaTime * pingSpeed;
            messageObject.transform.position = Vector3.Lerp(cable.GetPosition(0), cable.GetPosition(1), journey);
            yield return null;
        }

        Destroy(messageObject);
    }

    private void AppendOutput(string message)
    {
        outputText.text += message + "\n";

        Canvas.ForceUpdateCanvases();
        var scrollRect = outputText.GetComponentInParent<ScrollRect>();
        if (scrollRect != null) scrollRect.verticalNormalizedPosition = 0f;
    }

    public void CloseCMDPanel()
    {
        UIPanel.SetActive(false);
        optionsPanel.SetActive(true);
    }
}
