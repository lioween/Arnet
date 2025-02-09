using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.EventSystems;
using System.Linq;

public class LessonLockedButtonHandler : MonoBehaviour
{
    [Header("UI Settings")]
    public ScrollRect scrollView; // Assign the ScrollView containing the buttons
    public GameObject lockedPanel; // Assign the warning panel
    public TMP_Text txtBefore; // Assign the message text inside the locked panel

    void Start()
    {
        if (lockedPanel != null)
        {
            lockedPanel.SetActive(false); // Ensure the panel is hidden initially
        }

        // Get all buttons inside the ScrollView
        Button[] buttons = scrollView.content.GetComponentsInChildren<Button>(true);

        foreach (Button button in buttons)
        {
            if (button.interactable)
            {
                // ✅ Normal interactable buttons work as expected
                button.onClick.AddListener(() => Debug.Log($"Interactable button {button.name} clicked!"));
            }
            else
            {
                // ✅ Attach event only to locked buttons
                AddLockedButtonListener(button);
            }
        }
    }

    private bool isDragging = false;

    private void AddLockedButtonListener(Button button)
    {
        EventTrigger trigger = button.gameObject.AddComponent<EventTrigger>();

        // ✅ Click event for locked buttons (Prevent click if dragging)
        EventTrigger.Entry clickEntry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerClick
        };
        clickEntry.callback.AddListener((data) =>
        {
            if (!isDragging) // Only trigger click if no drag happened
            {
                OnLockedButtonClicked(button);
            }
        });
        trigger.triggers.Add(clickEntry);

        // ✅ Allow scrolling: Pass drag events to ScrollRect
        EventTrigger.Entry dragEntry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.Drag
        };
        dragEntry.callback.AddListener((data) =>
        {
            isDragging = true; // Mark as dragging
            PassEventToScrollRect((PointerEventData)data, EventTriggerType.Drag);
        });
        trigger.triggers.Add(dragEntry);

        // ✅ Allow beginning drag: Pass to ScrollRect
        EventTrigger.Entry beginDragEntry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.BeginDrag
        };
        beginDragEntry.callback.AddListener((data) =>
        {
            isDragging = true; // Mark as dragging
            PassEventToScrollRect((PointerEventData)data, EventTriggerType.BeginDrag);
        });
        trigger.triggers.Add(beginDragEntry);

        // ✅ Allow end drag: Pass to ScrollRect and reset dragging flag
        EventTrigger.Entry endDragEntry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.EndDrag
        };
        endDragEntry.callback.AddListener((data) =>
        {
            PassEventToScrollRect((PointerEventData)data, EventTriggerType.EndDrag);
            isDragging = false; // Reset dragging flag
        });
        trigger.triggers.Add(endDragEntry);
    }


    // ✅ Pass Drag Events to the ScrollRect
    private void PassEventToScrollRect(PointerEventData eventData, EventTriggerType eventType)
    {
        if (scrollView != null)
        {
            switch (eventType)
            {
                case EventTriggerType.BeginDrag:
                    scrollView.OnBeginDrag(eventData);
                    break;
                case EventTriggerType.Drag:
                    scrollView.OnDrag(eventData);
                    break;
                case EventTriggerType.EndDrag:
                    scrollView.OnEndDrag(eventData);
                    break;
            }
        }
    }



    private void OnLockedButtonClicked(Button button)
    {
        if (button.interactable)
        {
            Debug.Log($"Button {button.name} is interactable, ignoring locked panel.");
            return;
        }

        if (lockedPanel != null)
        {

            Button highestButton = GetHighestInteractableButton();

            if (txtBefore != null && highestButton != null)
            {
                TMP_Text buttonText = highestButton.GetComponentInChildren<TMP_Text>();

                if (buttonText != null)
                {
                    string firstLine = FormatLineAsSentence(buttonText.text, 1); // Get First Line

                    txtBefore.text = $"Before you proceed, make sure to finish <color=#00ffff>{firstLine}</color>";
                    lockedPanel.SetActive(true); // Show the warning panel
                }
                else
                {
                    txtBefore.text = "Complete the previous lesson first.";
                }
            }
            else
            {
                txtBefore.text = "No previous lessons found.";
            }

            Debug.Log($"Locked button {button.name} clicked! Panel displayed with message: {txtBefore.text}");
        }
    }

    // ✅ Function to Extract a Specific Line & Convert to Sentence Case
    private string FormatLineAsSentence(string text, int lineNumber)
    {
        if (string.IsNullOrEmpty(text))
            return null; // Default fallback

        string[] lines = text.Split('\n'); // Split text into lines
        if (lines.Length < lineNumber)
            return null; // Return default if line doesn't exist

        string selectedLine = lines[lineNumber - 1].Trim(); // Get the specific line (1-based index)

        return selectedLine; // Convert to sentence case
    }


    private Button GetHighestInteractableButton()
    {
        // Get all interactable buttons
        Button[] buttons = scrollView.content.GetComponentsInChildren<Button>(true);

        // Filter only interactable buttons and extract numbers correctly
        Button highestButton = buttons
            .Where(button => button.interactable)
            .OrderByDescending(button => ExtractNumberFromString(button.name)) // Extract numbers properly
            .FirstOrDefault();

        return highestButton;
    }

    // ✅ Function to Extract Numbers from String
    private int ExtractNumberFromString(string input)
    {
        string numberString = new string(input.Where(char.IsDigit).ToArray()); // Extracts only numbers
        return int.TryParse(numberString, out int number) ? number : 0; // Convert to int, default to 0 if failed
    }


    public void LoadSceneFromHighestButton()
    {
        Button highestButton = GetHighestInteractableButton();

        if (highestButton != null)
        {
            string sceneName = highestButton.name; // Use the button's name as the scene name
            Debug.Log($"Loading scene: {sceneName}");
            SceneManager.LoadScene(sceneName); // Load the scene
        }
        else
        {
            Debug.LogWarning("No interactable buttons found to load a scene.");
        }
    }

    public void CloseLockedPanel()
    {
        if (lockedPanel != null)
        {
            lockedPanel.SetActive(false); // Hide the panel
        }
    }
}
