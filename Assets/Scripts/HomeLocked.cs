using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.EventSystems;
using System.Linq;
using System.Text.RegularExpressions;

public class LockedButtonHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [Header("UI Settings")]
    public ScrollRect scrollView; // Assign the ScrollView containing the buttons
    public GameObject lockedPanel; // Assign the warning panel
    public TMP_Text txtComplete; // Assign the message text inside the locked panel
    public TMP_Text txtBefore; // Assign the message text inside the locked panel

    private bool isDragging = false;
    private Vector2 pointerDownPosition;
    private const float dragThreshold = 10f; // Prevent clicks if movement exceeds this

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
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        pointerDownPosition = eventData.position;
        isDragging = false; // Reset drag state
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isDragging) // Ensure it's not a drag operation
        {
            Button button = eventData.pointerEnter?.GetComponent<Button>();
            if (button != null)
            {
                if (button.interactable == true)
                {
                    button.onClick.Invoke(); // Normal button click
                }
                else
                {
                    OnLockedButtonClicked(button);
                }
            }
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (Vector2.Distance(pointerDownPosition, eventData.position) > dragThreshold)
        {
            isDragging = true; // Mark as drag if movement exceeds threshold
        }
    }


    private void OnLockedButtonClicked(Button button)
    {
       

        if (lockedPanel != null)
        {
            Button highestButton = GetHighestInteractableButton();

            if (txtComplete != null && highestButton != null)
            {
                TMP_Text buttonText = highestButton.GetComponentInChildren<TMP_Text>();

                if (buttonText != null)
                {
                    string firstLine = StripRichTextTags(FormatLineAsSentence(buttonText.text, 1));
                    string thirdLine = FormatLineAsSentence(buttonText.text, 3);
                    string fourthLine = FormatLineAsSentence(buttonText.text, 4);
                    string fifthLine = FormatLineAsSentence(buttonText.text, 5);

                    txtComplete.text = $"Complete <color=#00ffff>{firstLine}</color> First";
                    txtBefore.text = $"Before you proceed, make sure to finish <color=#00ffff>{thirdLine} {fourthLine} {fifthLine}</color>";
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

            lockedPanel.SetActive(true); // Show the warning panel
            Debug.Log($"Locked button {button.name} clicked! Panel displayed with message: {txtComplete.text}");
        }
    }

    private string FormatLineAsSentence(string text, int lineNumber)
    {
        if (string.IsNullOrEmpty(text)) return "";

        string[] lines = text.Split('\n');
        if (lines.Length < lineNumber) return "";

        return lines[lineNumber - 1].Trim();
    }

    private Button GetHighestInteractableButton()
    {
        Button[] buttons = scrollView.content.GetComponentsInChildren<Button>(true);

        return buttons
            .Where(button => button.interactable)
            .OrderByDescending(button => ExtractNumberFromString(button.name))
            .FirstOrDefault();
    }

    private int ExtractNumberFromString(string input)
    {
        string numberString = new string(input.Where(char.IsDigit).ToArray());
        return int.TryParse(numberString, out int number) ? number : 0;
    }

    private string StripRichTextTags(string text)
    {
        return string.IsNullOrEmpty(text) ? text : Regex.Replace(text, "<.*?>", string.Empty);
    }

    public void LoadSceneFromHighestButton()
    {
        Button highestButton = GetHighestInteractableButton();

        if (highestButton != null)
        {
            string sceneName = highestButton.name; // Use the button's name as the scene name
            Debug.Log($"Loading scene: {sceneName}");
            SceneManager.LoadScene(sceneName);
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
            lockedPanel.SetActive(false);
        }
    }
}