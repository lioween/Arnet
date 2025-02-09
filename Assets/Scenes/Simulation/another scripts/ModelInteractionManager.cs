using UnityEngine;
using UnityEngine.UI;

public class ModelInteractionManager : MonoBehaviour
{
    public GameObject optionsPanel; // The options panel UI
    public GameObject cmdPanel; // The CMD panel UI
    private GameObject selectedModel; // Reference to the selected model

    void Start()
    {
        // Ensure both panels are hidden at the start
        optionsPanel.SetActive(false);
        cmdPanel.SetActive(false);
    }

    void Update()
    {
        // Check if the user clicks on the screen
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                // Set the selected model
                selectedModel = hit.collider.gameObject;
                Debug.Log($"Selected model: {selectedModel.name}");

                // Show the options panel
                ShowOptionsPanel();
            }
        }
    }

    private void ShowOptionsPanel()
    {
        // Hide the CMD Panel if it's open
        cmdPanel.SetActive(false);

        // Show the Options Panel
        optionsPanel.SetActive(true);
    }

    public void RotateModel()
    {
        if (selectedModel != null)
        {
            selectedModel.transform.Rotate(0, 45, 0);
            Debug.Log($"Rotated {selectedModel.name}");
        }
    }

    public void ScaleModel()
    {
        if (selectedModel != null)
        {
            selectedModel.transform.localScale *= 1.1f; // Increase scale by 10%
            Debug.Log($"Scaled {selectedModel.name}");
        }
    }

    public void DeleteModel()
    {
        if (selectedModel != null)
        {
            Destroy(selectedModel);
            selectedModel = null;

            // Hide both panels
            optionsPanel.SetActive(false);
            cmdPanel.SetActive(false);

            Debug.Log("Deleted model.");
        }
    }

    public void OpenCMD()
    {
        if (selectedModel != null)
        {
            // Hide the Options Panel
            optionsPanel.SetActive(false);

            // Show the CMD Panel
            cmdPanel.SetActive(true);
            Debug.Log("Opened CMD interface.");
        }
    }

    public void CloseOptionsPanel()
    {
        // Hide the Options Panel
        optionsPanel.SetActive(false);
        Debug.Log("Closed Options Panel.");
    }

    public void CloseCMD()
    {
        // Hide the CMD Panel
        cmdPanel.SetActive(false);

        // Optionally, bring back the Options Panel
        optionsPanel.SetActive(true);

        Debug.Log("Closed CMD interface.");
    }
}
