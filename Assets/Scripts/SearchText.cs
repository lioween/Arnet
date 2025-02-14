using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TextSearchHighlighter : MonoBehaviour
{
    public TMP_Text[] textComponents; // Array of TMP_Text components for multiple texts
    public TMP_InputField searchField; // Input field for entering the search term
    public Button btnSearch;

    private Dictionary<TMP_Text, string> originalTexts = new Dictionary<TMP_Text, string>();
    private string searchTerm;
    private Dictionary<TMP_Text, List<int>> searchIndices = new Dictionary<TMP_Text, List<int>>();

    void Start()
    {
        foreach (TMP_Text text in textComponents)
        {
            originalTexts[text] = text.text; // Store the original text
            searchIndices[text] = new List<int>();
        }

        
    }

    public void SearchClick()
    {
        if (CompareColors(btnSearch.image.color, Color.white))
        {
            btnSearch.image.color = Color.cyan;
            searchField.text = "";
            searchField.gameObject.SetActive(true);
        }
        else if (CompareColors(btnSearch.image.color, Color.cyan))
        {
            btnSearch.image.color = Color.white;
            searchField.text = "";
            searchField.gameObject.SetActive(false);
        }
    }

    public void SearchReset()
    {
        btnSearch.image.color = Color.white;
        searchField.text = "";
        searchField.gameObject.SetActive(false);

    }

    private bool CompareColors(Color a, Color b, float tolerance = 0.01f)
    {
        return Mathf.Abs(a.r - b.r) < tolerance &&
               Mathf.Abs(a.g - b.g) < tolerance &&
               Mathf.Abs(a.b - b.b) < tolerance;
    }

    // Called when the search term is updated
    public void OnSearchTermChanged()
    {
        searchTerm = searchField.text.ToLower().Trim();

        if (searchTerm.Length < 2) // Prevents applying color on single characters
        {
            ResetAllTexts();
            return;
        }

        foreach (TMP_Text text in textComponents)
        {
            FindAllOccurrences(text);
            HighlightAllOccurrences(text);
        }
    }

    // Resets all TMP texts to their original state
    private void ResetAllTexts()
    {
        foreach (TMP_Text text in textComponents)
        {
            text.text = originalTexts[text];
        }
    }

    // Finds all occurrences of the search term in a given text component
    private void FindAllOccurrences(TMP_Text textComponent)
    {
        searchIndices[textComponent].Clear();

        if (!string.IsNullOrEmpty(searchTerm))
        {
            string original = originalTexts[textComponent];
            string lowerText = original.ToLower();

            int index = lowerText.IndexOf(searchTerm, 0);
            while (index != -1)
            {
                searchIndices[textComponent].Add(index);
                index = lowerText.IndexOf(searchTerm, index + searchTerm.Length);
            }
        }
    }

    // Highlights all found occurrences of the search term in a given text component
    private void HighlightAllOccurrences(TMP_Text textComponent)
    {
        string modifiedText = originalTexts[textComponent];
        string lowerText = modifiedText.ToLower();

        int offset = 0; // Adjusts for added <color> tags length

        foreach (int index in searchIndices[textComponent])
        {
            int adjustedIndex = index + offset;
            string coloredText = $"<color=#FFFF00>{modifiedText.Substring(adjustedIndex, searchTerm.Length)}</color>";

            modifiedText = modifiedText.Substring(0, adjustedIndex) +
                           coloredText +
                           modifiedText.Substring(adjustedIndex + searchTerm.Length);

            offset += "<color=#FFFF00></color>".Length; // Adjust offset due to added markup
        }

        textComponent.text = modifiedText;
    }
}
