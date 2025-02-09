using UnityEngine;

public class PanelController : MonoBehaviour
{
    public RectTransform panel; // Assign your panel's RectTransform
    public float animationDuration = 0.5f; // Time for the animation
    private bool isPanelVisible = false; // Current state of the panel

    private Vector2 hiddenPosition; // Position when hidden
    private Vector2 visiblePosition; // Position when visible

    void Start()
    {
        // Set visible and hidden positions
        if (panel != null)
        {
            visiblePosition = panel.anchoredPosition; // Save the current position as "visible"
            hiddenPosition = new Vector2(visiblePosition.x, visiblePosition.y - panel.rect.height); // Slide below
            panel.anchoredPosition = hiddenPosition; // Start hidden
        }
    }

    public void TogglePanel()
    {
        if (isPanelVisible)
            StartCoroutine(AnimatePanel(hiddenPosition)); // Hide
        else
            StartCoroutine(AnimatePanel(visiblePosition)); // Show

        isPanelVisible = !isPanelVisible; // Toggle state
    }

    private System.Collections.IEnumerator AnimatePanel(Vector2 targetPosition)
    {
        Vector2 startPosition = panel.anchoredPosition;
        float elapsedTime = 0;

        while (elapsedTime < animationDuration)
        {
            panel.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, elapsedTime / animationDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        panel.anchoredPosition = targetPosition; // Finalize position
    }
}
