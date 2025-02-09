using UnityEngine;

public class FirebaseWebView : MonoBehaviour
{
    public RectTransform panel; // Assign your UI Panel here
    public string videoURL; // Set this in the Unity Inspector
    WebViewObject webViewObject;

    void Start()
    {
        if (string.IsNullOrEmpty(videoURL))
        {
            Debug.LogError("Video URL is not set in the Inspector!");
            return;
        }

        webViewObject = (new GameObject("WebViewObject")).AddComponent<WebViewObject>();
        webViewObject.Init();
        webViewObject.LoadURL(videoURL);

        // Make sure WebView is resized to fit the UI Panel
        AdjustWebViewToPanel();
    }

    void Update()
    {
        AdjustWebViewToPanel(); // Update position dynamically
    }

    void AdjustWebViewToPanel()
    {
        if (panel == null)
        {
            Debug.LogError("UI Panel is not assigned!");
            return;
        }

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
}
