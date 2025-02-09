using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using TMPro;
using System.Collections;

public class Login : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_InputField inpEmail;
    public TMP_InputField inpPassword;
    public TMP_Text txtNotif;
    public GameObject loadingUI;

    private FirebaseAuth auth;
    private FirebaseFirestore db;
    private FirebaseUser user;

    public CaptchaVerifier captchaVerifier;
    private bool captchaVerified = false;

    int failedLoginAttempts = 0;
    void Start()
    {
        // Initialize Firebase Auth and Firestore
        auth = FirebaseAuth.DefaultInstance;
        db = FirebaseFirestore.DefaultInstance;

        if (captchaVerifier == null)
        {
            captchaVerifier = FindObjectOfType<CaptchaVerifier>();
        }
    }

    public void StartCaptchaVerification()
    {
        captchaVerified = false; // 🔹 Reset before starting verification
        FindObjectOfType<ReCaptchaWebView>().ShowCaptcha(); // Open reCAPTCHA
    }

    public void OnCaptchaVerified(string recaptchaToken)

    {
        FindObjectOfType<ReCaptchaWebView>().CloseWebView();

        Debug.Log("✅ CAPTCHA Verification Successful! Token: " + recaptchaToken);
        inpEmail.text = "";
        inpPassword.text = "";
        captchaVerified = true;
        txtNotif.text = "CAPTCHA Verification Successful!\nYou may now login again.";
        failedLoginAttempts = 0;
    }

    public void LoginUser()
    {
        string email = inpEmail.text.Trim();
        string password = inpPassword.text.Trim();

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            txtNotif.text = "Please enter both email and password.";
            Debug.LogWarning("❌ Login attempt failed: Email or password is empty.");
            return;
        }

        if (!IsConnectedToInternet())
        {
            txtNotif.text = "No internet connection.";
            Debug.LogError("❌ Network issue detected. Cannot proceed with login.");
            return;
        }

        StartCoroutine(LoginWithLoading(email, password));
    }

    IEnumerator LoginWithLoading(string email, string password)
    {
        loadingUI.SetActive(true);
        Debug.Log($"🔄 Attempting to log in with email: {email}");

        var loginTask = auth.SignInWithEmailAndPasswordAsync(email, password);
        yield return new WaitUntil(() => loginTask.IsCompleted);

        loadingUI.SetActive(false);

        if (loginTask.IsFaulted || loginTask.IsCanceled)
        {
            failedLoginAttempts++; // Increase failed attempt count
            txtNotif.text = "Incorrect email address or password.";
            Debug.LogWarning($"⚠️ Login failed. Attempt {failedLoginAttempts}");

            // Show CAPTCHA only after 3 failed attempts
            if (failedLoginAttempts >= 3)
            {
                txtNotif.text = "";
                Debug.LogWarning("⚠️ Too many failed attempts! Opening CAPTCHA verification.");
                StartCaptchaVerification();
            }
        }
        else
        {
            failedLoginAttempts = 0; // Reset counter on successful login

            if (!captchaVerified && failedLoginAttempts > 3)
            {
                txtNotif.text = "";
               
                Debug.LogWarning("⚠️ CAPTCHA not verified! Opening CAPTCHA verification.");
                StartCaptchaVerification();
            }
            else
            {
                // Login successful
                user = auth.CurrentUser;
                Debug.Log("✅ Login successful!");

                if (user != null && user.IsEmailVerified)
                {
                    txtNotif.text = "";
                    StartCoroutine(CheckCollectionAndLoadScene(user.UserId));
                }
                else
                {
                    txtNotif.text = "Email not verified. Check your inbox.";
                    SendVerificationEmail(user);
                    Debug.LogWarning("⚠️ Email not verified.");
                }
            }
        }
    }

    private void SendVerificationEmail(FirebaseUser user)
    {
        user.SendEmailVerificationAsync().ContinueWith(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                Debug.Log("📧 Verification email sent.");
            }
            else
            {
                Debug.LogError($"❌ Failed to send verification email: {task.Exception}");
            }
        });
    }

    private IEnumerator CheckCollectionAndLoadScene(string userId)
    {
        CollectionReference collectionRef = db.Collection("profile").Document(userId).Collection("1.0");
        var queryTask = collectionRef.Limit(1).GetSnapshotAsync();

        yield return new WaitUntil(() => queryTask.IsCompleted);

        if (queryTask.Exception != null)
        {
            Debug.LogError($"❌ Error checking Firestore collection: {queryTask.Exception}");
            yield break;
        }

        QuerySnapshot querySnapshot = queryTask.Result;

        if (querySnapshot.Count == 0)
        {
            SceneManager.LoadScene("Welcome");
        }
        else
        {
            SceneManager.LoadScene("Home");
        }
    }

    public bool IsConnectedToInternet()
    {
        return Application.internetReachability != NetworkReachability.NotReachable;
    }
}
