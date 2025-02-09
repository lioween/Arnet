using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TextSearchHighlighter : MonoBehaviour
{
    public TMP_Text textComponent;       // The TMP_Text component for the text to search in
    public TMP_InputField searchField;   // The input field for entering the search term
    public Button btnSearch;
  

    private string originalText;         // Stores the original unformatted text
    private string searchTerm;
    private List<int> searchIndices;
    private int currentSearchIndex;

    void Start()
    {
        searchIndices = new List<int>();
        currentSearchIndex = -1;
        originalText = textComponent.text; // Save the original text

        // Add listeners to buttons
     

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
            textComponent.text = originalText;
            return;
        }

        FindAllOccurrences();
        textComponent.text = originalText; // Reset to the original text

        if (searchIndices.Count > 0)
        {
            HighlightAllOccurrences();
        }
    }


    // Highlights all found occurrences of the search term by changing its color
    private void HighlightAllOccurrences()
{
    string modifiedText = originalText;
    string lowerText = originalText.ToLower();

    int offset = 0; // Adjusts for added <color> tags length

    foreach (int index in searchIndices)
    {
        int adjustedIndex = index + offset;
        string coloredText = $"<color=#FFFF00>{modifiedText.Substring(adjustedIndex, searchTerm.Length)}</color>"; // Red color

        modifiedText = modifiedText.Substring(0, adjustedIndex) + 
                       coloredText + 
                       modifiedText.Substring(adjustedIndex + searchTerm.Length);

        offset += "<color=#FFFF00></color>".Length; // Adjust offset due to added markup
    }

    textComponent.text = modifiedText;
}


    // Finds all occurrences of the search term
    private void FindAllOccurrences()
    {
        searchIndices.Clear();
        currentSearchIndex = -1;

        if (!string.IsNullOrEmpty(searchTerm))
        {
            string lowerText = originalText.ToLower(); // Convert text to lowercase for comparison
            int index = lowerText.IndexOf(searchTerm, 0);

            while (index != -1)
            {
                searchIndices.Add(index);
                index = lowerText.IndexOf(searchTerm, index + searchTerm.Length);
            }
        }
    }

   
   
}
