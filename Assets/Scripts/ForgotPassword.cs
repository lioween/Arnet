using Firebase.Auth;
using Firebase;
using Firebase.Extensions;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Text.RegularExpressions;
using TMPro;
using System;

public class ForgotPassword : MonoBehaviour
{
    private FirebaseAuth auth;

    // UI Elements
    public TMP_InputField txtemail;
    public TMP_Text txtnotif;
    public GameObject loadingUI;
    public GameObject pnlnotif;
    public TMP_Text pnlemail;
    public TMP_Text timerText;
    public Button sendButton;

    private bool isTimerActive = false;
    private float timerDuration = 60f;
    private float currentTime;

    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;

        // Check if a timer was previously set
        if (PlayerPrefs.HasKey("ForgotPasswordTimerStart"))
        {
            string savedTime = PlayerPrefs.GetString("ForgotPasswordTimerStart", "");
            if (!string.IsNullOrEmpty(savedTime))
            {
                DateTime savedDateTime = DateTime.Parse(savedTime);
                TimeSpan timeElapsed = DateTime.UtcNow - savedDateTime;

                if (timeElapsed.TotalSeconds < timerDuration)
                {
                    isTimerActive = true;
                    currentTime = timerDuration - (float)timeElapsed.TotalSeconds;
                    sendButton.interactable = false;
                    txtemail.interactable = false;
                    StartCoroutine(UpdateTimer());
                }
                else
                {
                    PlayerPrefs.DeleteKey("ForgotPasswordTimerStart");
                }
            }
        }
    }

    public void ResetPassword()
    {
        string email = txtemail.text.Trim();

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

    private IEnumerator ForgotPasswordWithLoading(string email)
    {
        loadingUI.SetActive(true);
        var task = auth.SendPasswordResetEmailAsync(email);
        yield return new WaitUntil(() => task.IsCompleted);

        loadingUI.SetActive(false);

        if (task.IsFaulted)
        {
            if (task.Exception != null)
            {
                foreach (var exception in task.Exception.InnerExceptions)
                {
                    txtnotif.text = exception.Message;
                }
            }
            else
            {
                txtnotif.text = "Failed to send password reset email.";
            }
        }
        else
        {
            pnlemail.text = email;
            pnlnotif.SetActive(true);
            yield return new WaitForSeconds(2.5f);
            pnlnotif.SetActive(false);
        }
    }

    private void DisableResendButtonWithTimer()
    {
        sendButton.interactable = false;
        txtemail.interactable = false;
        isTimerActive = true;

        // Store the exact timestamp when the timer started
        PlayerPrefs.SetString("ForgotPasswordTimerStart", DateTime.UtcNow.ToString());
        PlayerPrefs.Save();

        currentTime = timerDuration;
        StartCoroutine(UpdateTimer());
    }

    private IEnumerator UpdateTimer()
    {
        while (isTimerActive)
        {
            string savedTime = PlayerPrefs.GetString("ForgotPasswordTimerStart", "");
            if (!string.IsNullOrEmpty(savedTime))
            {
                DateTime savedDateTime = DateTime.Parse(savedTime);
                TimeSpan timeElapsed = DateTime.UtcNow - savedDateTime;
                currentTime = timerDuration - (float)timeElapsed.TotalSeconds;

                if (currentTime <= 0)
                {
                    isTimerActive = false;
                    sendButton.interactable = true;
                    txtemail.interactable = true;
                    timerText.text = "";
                    PlayerPrefs.DeleteKey("ForgotPasswordTimerStart");
                }
                else
                {
                    timerText.text = $"Didn't get an email? \n Resend in <b>{Mathf.CeilToInt(currentTime)}s</b>";
                }
            }

            yield return new WaitForSeconds(1f); // Reduce unnecessary calls per frame
        }
    }

    private bool IsValidEmail(string email)
    {
        string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return Regex.IsMatch(email, emailPattern);
    }
}
