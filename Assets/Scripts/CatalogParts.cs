using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UpdateTMPText : MonoBehaviour
{
    public TMP_Text textTitle;
    public TMP_Text textDesc;

    [TextArea(3, 5)]
    public string Title;

    [TextArea(5, 10)]
    public string Desc;

    public ScrollRect scrollView;

    public GameObject assigned3DPart; // Assign the 3D part in the Inspector
    private static Renderer lastHighlightedRenderer;
    private static Color lastOriginalColor;

    public Color highlightColor = new Color(1f, 1f, 0f, 0.5f); // Light yellow selection color

    void Start()
    {
        if (assigned3DPart == null)
        {
            Debug.LogError("No 3D Model assigned for: " + gameObject.name);
        }

        GetComponent<Button>().onClick.AddListener(OnButtonClick);
    }

    void OnButtonClick()
    {
        UpdateTextFields();
        Highlight3DPart();
    }

    public void UpdateTextFields()
    {
        if (textTitle != null) textTitle.text = Title;
        if (textDesc != null) textDesc.text = Desc;
        ResetScrollView();
    }

    void ResetScrollView()
    {
        if (scrollView != null) scrollView.verticalNormalizedPosition = 1f;
    }

    void Highlight3DPart()
    {
        if (assigned3DPart == null) return;

        Renderer renderer = assigned3DPart.GetComponent<Renderer>();

        if (renderer == null)
        {
            Debug.LogWarning("No Renderer found on: " + assigned3DPart.name);
            return;
        }

        // Restore previous object's color
        if (lastHighlightedRenderer != null)
        {
            lastHighlightedRenderer.material.color = lastOriginalColor;
        }

        // Store original color before changing it
        lastOriginalColor = renderer.material.color;
        lastHighlightedRenderer = renderer;

        // Apply highlight color
        renderer.material.color = highlightColor;
    }
}
