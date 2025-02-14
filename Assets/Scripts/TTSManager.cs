using UnityEngine;
using TMPro;
using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine.UI; // Required for ScrollRect

public class TTSManager : MonoBehaviour
{
    public TMP_Text txtTitle;
    public TMP_Text txtText; // Reference to the TMP text object
    public TMP_Text txtExtra1;
    public TMP_Text txtExtra2;
    public TMP_Text txtExtra3;
    public TMP_Text txtExtra4;

    private TextToSpeech tts; // Replace with your TTS class
    private string originalText;

    public GameObject btnPlay;
    public GameObject btnStop;
    public ScrollRect scrollRect; // Reference to the ScrollRect for smooth scrolling

    private string[] paragraphs;
    private int currentParagraphIndex = 0;
    private bool isSpeaking = false;

    void Start()
    {
        tts = gameObject.AddComponent<TextToSpeech>();

        if (txtText != null)
        {
            originalText = txtText.text;
            paragraphs = Regex.Split(txtText.text, "\n+", RegexOptions.Compiled);
        }
    }

    public void SpeakTMPText()
    {
        if (txtText == null)
        {
            Debug.LogError("TMP Text is not assigned.");
            return;
        }

        if (paragraphs == null || paragraphs.Length == 0)
        {
            Debug.LogWarning("TextMeshPro text is empty or not split correctly.");
            return;
        }

        btnPlay.SetActive(false);
        btnStop.SetActive(true);
        isSpeaking = true;
        StartCoroutine(ReadTitleAndText());
    }

    private IEnumerator ReadTitleAndText()
    {
        tts.Speak(StripRichTextTags(txtTitle.text));
        while (tts.IsSpeaking())
        {
            if (!isSpeaking) yield break;
            yield return null;
        }

        yield return new WaitForSeconds(1f);
        yield return StartCoroutine(ReadTextParagraphByParagraph(paragraphs, txtText));
        yield return StartCoroutine(ReadExtraTexts());
    }

    private IEnumerator ReadTextParagraphByParagraph(string[] textParagraphs, TMP_Text textComponent)
    {
        for (currentParagraphIndex = 0; currentParagraphIndex < textParagraphs.Length; currentParagraphIndex++)
        {
            if (!isSpeaking) break;

            string paragraph = textParagraphs[currentParagraphIndex];
            tts.Speak(StripRichTextTags(paragraph));
            HighlightParagraph(textParagraphs, textComponent, currentParagraphIndex);

            while (tts.IsSpeaking())
            {
                if (!isSpeaking) break;
                yield return null;
            }
            yield return new WaitForSeconds(1f);
        }

        RemoveHighlight(textComponent, textParagraphs);
    }

    private IEnumerator ReadExtraTexts()
    {
        TMP_Text[] extraTexts = { txtExtra1, txtExtra2, txtExtra3, txtExtra4 };
        foreach (TMP_Text extraText in extraTexts)
        {
            if (extraText != null && !string.IsNullOrWhiteSpace(extraText.text))
            {
                string[] extraParagraphs = Regex.Split(extraText.text, "\n+", RegexOptions.Compiled);
                yield return StartCoroutine(ReadTextParagraphByParagraph(extraParagraphs, extraText));
            }
        }

        btnPlay.SetActive(true);
        btnStop.SetActive(false);
        isSpeaking = false;
    }

    public void StopSpeaking()
    {
        if (tts != null)
        {
            tts.Stop();
            isSpeaking = false;
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

    private void HighlightParagraph(string[] textParagraphs, TMP_Text textComponent, int index)
    {
        textComponent.ForceMeshUpdate();
        string highlightedText = "";
        for (int i = 0; i < textParagraphs.Length; i++)
        {
            highlightedText += (i == index) ? $"<color=yellow>{textParagraphs[i]}</color>\n" : $"{textParagraphs[i]}\n";
        }
        textComponent.text = highlightedText.TrimEnd('\n');
    }

    private void RemoveHighlight(TMP_Text textComponent, string[] textParagraphs)
    {
        textComponent.text = string.Join("\n", textParagraphs);
    }

    private string StripRichTextTags(string text)
    {
        return Regex.Replace(text, "<.*?>", string.Empty);
    }
}
