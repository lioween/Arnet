using UnityEngine;
using UnityEngine.UI;

public class FirebaseWebView : MonoBehaviour
{
    public RectTransform panel; // Assign your UI Panel here
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
        webViewObject.Init(
            transparent: true // ✅ Makes WebView background transparent
        );
        webViewObject.LoadURL(videoURL);

        AdjustWebViewToPanel();

        // ✅ Get the Image component
        Image panelImage = panel.GetComponent<Image>();

        if (panelImage != null)
        {
            // ✅ Create a new color with transparency
            Color transparentColor = panelImage.color;
            transparentColor.a = 0f; // Set alpha to 0 (fully transparent)

            // ✅ Reassign the modified color back to the Image component
            panelImage.color = transparentColor;
        }

        Button panelButton = panel.GetComponentInChildren<Button>(); // ✅ Get the Button component
        if (panelButton != null)
        {
            Image buttonImage = panelButton.image; // ✅ Get the Image component of the Button
            if (buttonImage != null)
            {
                Color newColor = buttonImage.color; // ✅ Retrieve the current color
                newColor.a = 0f; // ✅ Modify the alpha value
                buttonImage.color = newColor; // ✅ Apply the modified color
            }
        }


    }

    public void StopVideo()
    {
        if (webViewObject != null)
        {
            webViewObject.SetVisibility(false); // Hide WebView
            Destroy(webViewObject.gameObject); // Destroy WebView
            webViewObject = null;
            Debug.Log("✅ WebView Stopped and Destroyed.");
        }

        Image panelImage = panel.GetComponent<Image>();

        if (panelImage != null)
        {
            // ✅ Create a new color with transparency
            Color transparentColor = panelImage.color;
            transparentColor.a = 255f; // Set alpha to 0 (fully transparent)

            // ✅ Reassign the modified color back to the Image component
            panelImage.color = transparentColor;
        }

        Button panelButton = panel.GetComponentInChildren<Button>(); // ✅ Get the Button component
        if (panelButton != null)
        {
            Image buttonImage = panelButton.image; // ✅ Get the Image component of the Button
            if (buttonImage != null)
            {
                Color newColor = buttonImage.color; // ✅ Retrieve the current color
                newColor.a = 255f; // ✅ Modify the alpha value
                buttonImage.color = newColor; // ✅ Apply the modified color
            }
        }

        panel.gameObject.SetActive(true); // Show panel again
    }

    void Update()
    {
        AdjustWebViewToPanel();
    }

    void AdjustWebViewToPanel()
    {
        if (panel == null)
        {
            Debug.LogError("UI Panel is not assigned!");
            return;
        }

        if (webViewObject == null) return; // ✅ Prevent errors if WebView is destroyed

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
