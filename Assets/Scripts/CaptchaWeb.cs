using UnityEngine;
using UnityEngine.EventSystems;

public class ReCaptchaWebView : MonoBehaviour
{
    private WebViewObject webView;
    public Login loginScript;
    public CaptchaVerifier captchaVerifier;
    public GameObject overlay; // 🔹 UI overlay to block scene clicks

    private string siteKey = "6LfTWcYqAAAAAO720aDEISXBcISrHYRopT3Hj26V"; // Replace with your real site key
    private string captchaURL = "https://arnet-qcu-a0f45.web.app/recaptcha.html?cache_bypass=1";

    public void ShowCaptcha()
    {
        // Ensure WebView is properly reset
        if (webView != null)
        {
            Destroy(webView.gameObject);
        }

        // Activate overlay to block scene clicks
        if (overlay != null)
        {
            overlay.SetActive(true);
        }

        // Initialize WebView
        webView = (new GameObject("WebViewObject")).AddComponent<WebViewObject>();
        webView.Init(OnMessageReceived, enableWKWebView: true);

        // Enable rounded corners via CSS
        webView.EvaluateJS(@"
            document.body.style.borderRadius = '15px';
            document.body.style.overflow = 'hidden';
            document.body.style.boxShadow = '0px 4px 10px rgba(0, 0, 0, 0.2)';
        ");

        // Load CAPTCHA URL
        string fullUrl = $"{captchaURL}?k={siteKey}";
        Debug.Log("🔗 Loading CAPTCHA URL: " + fullUrl);
        webView.LoadURL(fullUrl);

        // Set WebView margins (centered)
        int popupWidth = Mathf.RoundToInt(Screen.width * 0.9f);
        int popupHeight = Mathf.RoundToInt(Screen.height * 0.6f);
        int leftMargin = (Screen.width - popupWidth) / 2;
        int topMargin = (Screen.height - popupHeight) / 2;
        webView.SetMargins(leftMargin, topMargin, leftMargin, topMargin);

        webView.SetVisibility(true);
    }

    private void OnMessageReceived(string msg)
    {
        Debug.Log("📩 Received message from WebView: " + msg);

        string responseToken = msg.Trim();

        if (!string.IsNullOrEmpty(responseToken) && responseToken.Length > 100)
        {
            Debug.Log("✅ CAPTCHA Token Processed: " + responseToken);

            if (captchaVerifier != null)
            {
                captchaVerifier.VerifyCaptcha(responseToken, (success) =>
                {
                    OnCaptchaVerificationComplete(success, responseToken);
                });
            }
            else
            {
                Debug.LogError("❌ CaptchaVerifier script is missing!");
            }

            CloseWebView();
        }
        else
        {
            Debug.LogError("❌ Unexpected WebView message: " + msg);
        }
    }

    private void OnCaptchaVerificationComplete(bool success, string recaptchaToken)
    {
        if (success)
        {
            Debug.Log("✅ CAPTCHA Verified Successfully!");

            if (loginScript != null)
            {
                loginScript.OnCaptchaVerified(recaptchaToken);
                Debug.Log("✅ Login Script Notified of CAPTCHA Verification.");
            }
            else
            {
                Debug.LogError("❌ Login script is not assigned in ReCaptchaWebView!");
            }
        }
        else
        {
            Debug.LogError("❌ CAPTCHA verification failed. User cannot proceed.");
        }
    }

    public void CloseWebView()
    {
        if (webView != null)
        {
            webView.SetVisibility(false);
            Destroy(webView.gameObject);
            webView = null;
            Debug.Log("❌ WebView Closed and Destroyed.");
        }

        // Deactivate overlay to re-enable scene interaction
        if (overlay != null)
        {
            overlay.SetActive(false);
        }
    }
}
