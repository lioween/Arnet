using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using TMPro;

[System.Serializable]
public class RecaptchaResponse
{
    public bool success;
}

public class CaptchaVerifier : MonoBehaviour
{
    private string recaptchaSecretKey = "6LfTWcYqAAAAAEv-Ky8yMb5uHe2TuyDGx0GOpSbq"; // Replace with your actual Secret Key
    public TMP_Text captchaResultText;

    public void VerifyCaptcha(string userResponseToken, System.Action<bool> callback)
    {
        StartCoroutine(VerifyCaptchaCoroutine(userResponseToken, callback));
    }

    private IEnumerator VerifyCaptchaCoroutine(string userResponseToken, System.Action<bool> callback)
    {
        string url = "https://www.google.com/recaptcha/api/siteverify";
        WWWForm form = new WWWForm();
        form.AddField("secret", recaptchaSecretKey);
        form.AddField("response", userResponseToken);

        using (UnityWebRequest request = UnityWebRequest.Post(url, form))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("❌ CAPTCHA verification request failed: " + request.error);
                captchaResultText.text = "CAPTCHA Verification Error.";
                callback?.Invoke(false);
                yield break;
            }

            string responseJson = request.downloadHandler.text;
            Debug.Log("📩 Google CAPTCHA Response: " + responseJson);

            RecaptchaResponse response = JsonUtility.FromJson<RecaptchaResponse>(responseJson);

            if (response != null && response.success)
            {
                captchaResultText.text = "CAPTCHA Verified!";
                Debug.Log("🎉 CAPTCHA verification successful!");
                callback?.Invoke(true); // ✅ Calls login
            }
            else
            {
                captchaResultText.text = "CAPTCHA Failed.";
                Debug.LogError("❌ CAPTCHA verification failed!");
                callback?.Invoke(false);
            }
        }
    }
}
