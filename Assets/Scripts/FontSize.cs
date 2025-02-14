using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FontAdjuster : MonoBehaviour
{
    [Header("UI Elements")]
    public Slider fontSizeSlider;  // Assign the slider in the Inspector
    public TMP_Text[] texts;       // Assign the five TMP texts in the Inspector

    [Header("Font Size Settings")]
    public int minFontSize = 10;   // Set your minimum font size
    public int maxFontSize = 50;   // Set your maximum font size

    void Start()
    {
        if (fontSizeSlider == null || texts.Length == 0)
        {
            Debug.LogError("⚠️ Assign the slider and TMP texts in the Inspector!");
            return;
        }

        // Set slider range
        fontSizeSlider.minValue = minFontSize;
        fontSizeSlider.maxValue = maxFontSize;

        // Set default value
        fontSizeSlider.value = minFontSize;
        UpdateFontSize(fontSizeSlider.value);

        // Listen for slider value changes
        fontSizeSlider.onValueChanged.AddListener(UpdateFontSize);
    }

    void UpdateFontSize(float newSize)
    {
        foreach (TMP_Text text in texts)
        {
            if (text != null)
            {
                text.fontSize = newSize;
            }
        }
    }
}
