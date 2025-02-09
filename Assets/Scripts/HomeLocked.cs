using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.EventSystems;
using System.Linq;

public class LockedButtonHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [Header("UI Settings")]
    public ScrollRect scrollView; // Assign the ScrollView containing the buttons
    public GameObject lockedPanel; // Assign the warning panel
    public TMP_Text txtComplete; // Assign the message text inside the locked panel
    public TMP_Text txtBefore; // Assign the message text inside the locked panel

    private bool isDragging = false;
    private Vector2 pointerDownPosition;
    private const float dragThreshold = 10f; // ✅ Prevent clicks if movement exceeds this

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
            button.onClick.AddListener(() => OnLockedButtonClicked(button));
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        pointerDownPosition = eventData.position;
        isDragging = false; // ✅ Reset drag state
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isDragging) // ✅ Only trigger click if no drag occurred
        {
            Button button = eventData.pointerPress?.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.Invoke();
            }
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (Vector2.Distance(pointerDownPosition, eventData.position) > dragThreshold)
        {
            isDragging = true; // ✅ Mark as drag if movement exceeds threshold
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

            if (txtComplete != null && highestButton != null)
            {
                TMP_Text buttonText = highestButton.GetComponentInChildren<TMP_Text>();

                if (buttonText != null)
                {
                    string firstLine = StripRichTextTags(FormatLineAsSentence(buttonText.text, 1)); // Get First Line
                    string thirdLine = FormatLineAsSentence(buttonText.text, 3); // Get Third Line
                    string fourthLine = FormatLineAsSentence(buttonText.text, 4); // Get Third Line
                    string fifthLine = FormatLineAsSentence(buttonText.text, 5); // Get Third Line

                    txtComplete.text = $"Complete <color=#00ffff>{firstLine}</color> First";
                    txtBefore.text = $"Before you proceed, make sure to finish <color=#00ffff>{thirdLine} {fourthLine} {fifthLine}</color>";
                    lockedPanel.SetActive(true); // Show the warning panel
                }
                else
                {
                    txtComplete.text = "Complete the previous lesson first.";
                }
            }
            else
            {
                txtComplete.text = "No previous lessons found.";
            }

            Debug.Log($"Locked button {button.name} clicked! Panel displayed with message: {txtComplete.text}");
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

    private string StripRichTextTags(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        // Remove rich text tags using regex
        return System.Text.RegularExpressions.Regex.Replace(text, "<.*?>", string.Empty);
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
