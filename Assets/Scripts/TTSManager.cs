using UnityEngine;
using TMPro;
using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine.UI; // Required for ScrollRect

public class TTSManager : MonoBehaviour
{
    public TMP_Text txtTitle;
    public TMP_Text txtText; // Reference to the TMP text object
    private TextToSpeech tts; // Replace with your TTS class
    private string originalText;

    public GameObject btnPlay;
    public GameObject btnStop;
    public ScrollRect scrollRect; // Reference to the ScrollRect for smooth scrolling

    private string[] paragraphs; // Array to hold text split into paragraphs
    private int currentParagraphIndex = 0;
    private bool isSpeaking = false; // Flag to track if speaking is ongoing

    void Start()
    {
        // Initialize your TTS class (AndroidTextToSpeech in this case)
        tts = gameObject.AddComponent<TextToSpeech>();

        // Split text into paragraphs
        if (txtText != null)
        {
            originalText = txtText.text; // Store the original text
            paragraphs = Regex.Split(txtText.text, "\n+", RegexOptions.Compiled); // Split by newlines
        }
    }

    public void SpeakTMPText()
    {
        // Check if TMP text is assigned
        if (txtText == null)
        {
            Debug.LogError("TMP Text is not assigned.");
            return;
        }

        // Check if the text is empty
        if (paragraphs == null || paragraphs.Length == 0)
        {
            Debug.LogWarning("TextMeshPro text is empty or not split correctly.");
            return;
        }

        btnPlay.SetActive(false);
        btnStop.SetActive(true);
        isSpeaking = true; // Set the speaking flag to true
        StartCoroutine(ReadTitleAndText());
    }

    private IEnumerator ReadTitleAndText()
    {
        // Speak the title first
        tts.Speak(StripRichTextTags(txtTitle.text));

        // Wait until the TTS is done speaking the title
        while (tts.IsSpeaking())
        {
            if (!isSpeaking) // Stop if interrupted
                yield break;

            yield return null;
        }

        // Optional delay after the title
        yield return new WaitForSeconds(1f);

        // Proceed to read paragraphs
        yield return StartCoroutine(ReadTextParagraphByParagraph());
    }

    private IEnumerator ReadTextParagraphByParagraph()
    {
        for (currentParagraphIndex = 0; currentParagraphIndex < paragraphs.Length; currentParagraphIndex++)
        {
            if (!isSpeaking) // Stop if speaking was interrupted
                break;

            // Get the current paragraph and speak it
            string paragraph = paragraphs[currentParagraphIndex];
            tts.Speak(StripRichTextTags(paragraph));

            // Highlight the current paragraph in the TMP text
            HighlightParagraph(currentParagraphIndex);

            // Smoothly scroll to center the current paragraph only if it is outside the center threshold
            yield return StartCoroutine(SmoothScrollToParagraphIfNeeded(currentParagraphIndex));

            // Wait for TTS to finish speaking the paragraph
            while (tts.IsSpeaking())
            {
                if (!isSpeaking) // Stop if interrupted
                    break;

                yield return null;
            }

            // Small delay before the next paragraph
            if (isSpeaking)
                yield return new WaitForSeconds(1f);
        }

        // Reset button states after all paragraphs are read or stopped
        btnPlay.SetActive(true);
        btnStop.SetActive(false);
        isSpeaking = false;
    }

    private IEnumerator SmoothScrollToParagraphIfNeeded(int index)
    {
        if (scrollRect == null || paragraphs == null || paragraphs.Length == 0)
            yield break;

        // Calculate the target scroll position to center the paragraph
        float targetPosition = 1 - (float)index / (paragraphs.Length - 1); // Inverted because Unity scrolls from top to bottom

        // Adjust to center the paragraph
        targetPosition -= 0.5f / (paragraphs.Length - 1); // Offset to center the highlighted paragraph
        targetPosition = Mathf.Clamp01(targetPosition); // Ensure the position stays within bounds

        // Define a threshold for the center position
        float threshold = 0.1f; // Allowable offset from the center

        // Check if the paragraph is already close to the center
        if (Mathf.Abs(scrollRect.verticalNormalizedPosition - targetPosition) < threshold)
            yield break; // Skip scrolling if already close to the center

        float startPosition = scrollRect.verticalNormalizedPosition;

        // Smoothly interpolate the scroll position
        float duration = 0.5f; // Duration of the smooth scroll
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            scrollRect.verticalNormalizedPosition = Mathf.Lerp(startPosition, targetPosition, elapsedTime / duration);
            yield return null;
        }

        // Ensure the final position is exactly the target
        scrollRect.verticalNormalizedPosition = targetPosition;
    }

    

    public void StopSpeaking()
    {
        if (tts != null)
        {
            tts.Stop(); // Assuming the TTS class has a Stop method
            isSpeaking = false; // Set the speaking flag to false
            btnPlay.SetActive(true);
            btnStop.SetActive(false);
            Debug.Log("Text-to-Speech stopped.");

            if (txtText != null && originalText != null)
            {
                txtText.text = originalText;
            }
        }
        else
        {
            Debug.LogWarning("TTS is not initialized.");
        }
    }

    private void HighlightParagraph(int index)
    {
        // Preserve the original text structure
        TMP_TextInfo textInfo = txtText.textInfo;
        txtText.ForceMeshUpdate(); // Update text mesh data

        string highlightedText = "";
        for (int i = 0; i < paragraphs.Length; i++)
        {
            if (i == index)
            {
                highlightedText += $"<color=yellow>{paragraphs[i]}</color>\n";
            }
            else
            {
                highlightedText += $"{paragraphs[i]}\n";
            }
        }

        txtText.text = highlightedText.TrimEnd('\n');
    }

     private string StripRichTextTags(string text)
        {
        return Regex.Replace(text, "<.*?>", string.Empty); // Remove all tags except <align=center>

    }


}
