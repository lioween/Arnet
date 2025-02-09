using Firebase.Auth;
using Firebase;
using Firebase.Extensions;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Text.RegularExpressions;
using TMPro;

public class ForgotPassword : MonoBehaviour
{
    private FirebaseAuth auth;

    // UI Elements
    public TMP_InputField txtemail;  // For user to enter email
    public TMP_Text txtnotif;       // To display success/error messages
    public GameObject loadingUI;
    public GameObject pnlnotif;     // Notification panel
    public TMP_Text pnlemail;       // Email text on the notification panel
    public TMP_Text timerText;      // Timer countdown text
    public Button sendButton; // The resend button (disable during the timer)

    private bool isTimerActive = false;
    private float timerDuration = 60f; // Timer duration in seconds
    private float currentTime;

    
    
    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;

        if (PlayerPrefs.HasKey("ForgotPasswordTimerStart"))
        {
            float startTime = PlayerPrefs.GetFloat("ForgotPasswordTimerStart");
            float timeElapsed = Time.time - startTime;
            if (timeElapsed < timerDuration)
            {
                isTimerActive = true;
                currentTime = timerDuration - timeElapsed;
                sendButton.interactable = false;
                txtemail.interactable = false;
                StartCoroutine(UpdateTimer());
            }
            else
            {
                PlayerPrefs.DeleteKey("ForgotPasswordTimerStart"); // Remove timer if expired
            }
        }
    }

    // Method to trigger password reset
   

    public void ResetPassword()
    {
        string email = txtemail.text;

        if (string.IsNullOrEmpty(email))
        {
            txtnotif.text = "Please enter an email address.";
            return;
        }
        else if (!IsValidEmail(email))
        {
            txtnotif.text = "Please enter a valid email address.";
            return;
        }

        StartCoroutine(ForgotPasswordWithLoading(email));
        DisableResendButtonWithTimer();
    }


    IEnumerator ForgotPasswordWithLoading(string email)
    {
        loadingUI.SetActive(true);
        var task = auth.SendPasswordResetEmailAsync(email);

        // Wait until the task is completed
        yield return new WaitUntil(() => task.IsCompleted);

        loadingUI.SetActive(false);

        if (task.IsCompleted && !task.IsFaulted)
        {
            pnlemail.text = email;
            pnlnotif.SetActive(true); // Show the notification panel
            yield return new WaitForSeconds(2.5f);
            pnlnotif.SetActive(false); // Hide the notification panel
            
        }
        else if (task.Exception != null)
        {
            txtnotif.text = task.Exception.InnerExceptions[0].Message;
        }
        else
        {
            txtnotif.text = "Failed to send password reset email.";
        }
    }

    private void DisableResendButtonWithTimer()
    {
        sendButton.interactable = false;
        txtemail.interactable = false;
        isTimerActive = true;

        // Store the time when the timer started
        PlayerPrefs.SetFloat("ForgotPasswordTimerStart", Time.time);
        PlayerPrefs.Save();

        currentTime = timerDuration;
        StartCoroutine(UpdateTimer());
    }

    private IEnumerator UpdateTimer()
    {
        while (isTimerActive)
        {
            // Calculate remaining time
            float startTime = PlayerPrefs.GetFloat("ForgotPasswordTimerStart", 0);
            currentTime = timerDuration - (Time.time - startTime);

            if (currentTime <= 0)
            {
                isTimerActive = false;
                sendButton.interactable = true;
                txtemail.interactable = true;
                timerText.text = "";
                PlayerPrefs.DeleteKey("ForgotPasswordTimerStart"); // Clear stored time
            }
            else
            {
                timerText.text = $"Didn't get an email? \n Resend in <b>{Mathf.CeilToInt(currentTime)}s</b>";
            }

            yield return null;
        }
    }

    // Call this in Start() to Resume Timer When Scene Reloads
    


    private bool IsValidEmail(string email)
    {
        string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return Regex.IsMatch(email, emailPattern);
    }
}
