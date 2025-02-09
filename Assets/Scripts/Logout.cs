using Firebase.Auth;
using UnityEngine;
using UnityEngine.SceneManagement;
using Google;
using Firebase;
using Firebase.Extensions;
using System.Collections;
using TMPro;

public class LogoutScript : MonoBehaviour
{
    private FirebaseAuth auth;
    public GameObject loadingUI;

    void Start()
    {
        // Initialize Firebase Authentication
        auth = FirebaseAuth.DefaultInstance;
    }

    // Call this method to log out the current user
    public void LogoutUser()
    {
        StartCoroutine(Logout());
    }
    public IEnumerator Logout()
    {
        loadingUI.SetActive(true);

        // Perform Firebase logout
        if (auth.CurrentUser != null)
        {
            GoogleSignIn.DefaultInstance.SignOut();
            auth.SignOut();
            Debug.Log("Google user logged out successfully.");
        }
        else
        {
            Debug.LogWarning("GoogleSignIn instance is null.");
        }

        // Optional delay to ensure logout is processed properly
        yield return new WaitForSeconds(1.0f);

        loadingUI.SetActive(false);

        // Load the Login scene
        yield return new WaitForSeconds(0.5f);

        SceneManager.LoadScene("SplashScreen 1");
    }

}
