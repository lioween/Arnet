using UnityEngine;
using UnityEngine.UI;

public class VideoLink : MonoBehaviour
{
    public RectTransform panel; // The panel containing the WebView
    public ScrollRect scrollView; // Assign your ScrollView here
    public string videoURL; // Set this in the Unity Inspector
    private WebViewObject webViewObject;

    public void StartVideo()
    {
        if (string.IsNullOrEmpty(videoURL))
        {
            Debug.LogError("Video URL is not set in the Inspector!");
            return;
        }

        if (webViewObject != null) // If already running, stop first
        {
            StopVideo();
        }

        webViewObject = (new GameObject("WebViewObject")).AddComponent<WebViewObject>();
        webViewObject.Init(transparent: true);
        webViewObject.LoadURL(videoURL);

        AdjustWebViewToPanel();
        HidePanelGraphics();
    }

    public void StopVideo()
    {
        if (webViewObject != null)
        {
            webViewObject.SetVisibility(false);
            Destroy(webViewObject.gameObject);
            webViewObject = null;
            Debug.Log("✅ WebView Stopped and Destroyed.");
        }

        ShowPanelGraphics();
        panel.gameObject.SetActive(true); // Show panel again
    }

    void Update()
    {
        AdjustWebViewToPanel();

        if (IsPanelOutOfScrollViewBounds())
        {
            Debug.Log("🚨 Panel is out of ScrollView bounds. Destroying WebView.");
            StopVideo();
        }
    }

    void AdjustWebViewToPanel()
    {
        if (panel == null || webViewObject == null) return;

        // Convert panel's position & size into screen coordinates
        Vector3[] panelCorners = new Vector3[4];
        panel.GetWorldCorners(panelCorners);

        int left = (int)panelCorners[0].x;
        int top = Screen.height - (int)panelCorners[1].y;
        int right = Screen.width - (int)panelCorners[2].x;
        int bottom = (int)panelCorners[0].y;

        webViewObject.SetMargins(left, top, right, bottom);
        webViewObject.SetVisibility(true);
    }

    bool IsPanelOutOfScrollViewBounds()
    {
        if (panel == null || scrollView == null) return true;

        RectTransform viewport = scrollView.viewport; // Get the viewport RectTransform

        // Get panel's position relative to the viewport
        Vector3[] panelCorners = new Vector3[4];
        panel.GetWorldCorners(panelCorners);

        Vector3[] viewportCorners = new Vector3[4];
        viewport.GetWorldCorners(viewportCorners);

        // Convert world corners to local positions relative to the viewport
        for (int i = 0; i < 4; i++)
        {
            panelCorners[i] = viewport.InverseTransformPoint(panelCorners[i]);
            viewportCorners[i] = viewport.InverseTransformPoint(viewportCorners[i]);
        }

        // Calculate the viewport bounds
        float viewportTop = viewportCorners[2].y;

        // Calculate the panel bounds
        float panelTop = panelCorners[2].y;

        // Destroy WebView only if the panel's top is **above** the viewport's top
        return panelTop > viewportTop;
    }
    
    void HidePanelGraphics()
    {
        if (panel == null) return;

        Image panelImage = panel.GetComponent<Image>();
        if (panelImage != null) panelImage.color = new Color(panelImage.color.r, panelImage.color.g, panelImage.color.b, 0f);

        Button panelButton = panel.GetComponentInChildren<Button>();
        if (panelButton != null && panelButton.image != null)
        {
            panelButton.image.color = new Color(panelButton.image.color.r, panelButton.image.color.g, panelButton.image.color.b, 0f);
        }
    }

    void ShowPanelGraphics()
    {
        if (panel == null) return;

        Image panelImage = panel.GetComponent<Image>();
        if (panelImage != null) panelImage.color = new Color(panelImage.color.r, panelImage.color.g, panelImage.color.b, 1f);

        Button panelButton = panel.GetComponentInChildren<Button>();
        if (panelButton != null && panelButton.image != null)
        {
            panelButton.image.color = new Color(panelButton.image.color.r, panelButton.image.color.g, panelButton.image.color.b, 1f);
        }
    }
}
