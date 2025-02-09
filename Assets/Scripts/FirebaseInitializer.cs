using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using Google;
using UnityEngine.SceneManagement;
using System.Collections;

public class FirebaseInitializer : MonoBehaviour
{
    private static FirebaseInitializer _instance;
    public static FirebaseInitializer Instance => _instance;

    private bool isFirebaseInitialized = false;
    private GoogleSignInConfiguration googleSignInConfig;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeFirebase();
            InitializeGoogleSignIn();

            StartCoroutine(WaitForInitialization());
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeFirebase()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                isFirebaseInitialized = true;
                Debug.Log("Firebase initialized successfully!");
            }
            else
            {
                Debug.LogError($"Could not resolve Firebase dependencies: {task.Result}");
            }
        });
    }

    private void InitializeGoogleSignIn()
    {
        googleSignInConfig = new GoogleSignInConfiguration
        {
            WebClientId = "669840688035-jaj1rh6ou6vr1ifjhatbuffqbud3uvqs.apps.googleusercontent.com", // Replace with your Firebase project's Web client ID
            RequestIdToken = true
        };

        GoogleSignIn.Configuration = googleSignInConfig;
        Debug.Log("Google Sign-In initialized successfully!");
    }

    private IEnumerator WaitForInitialization()
    {
        while (!isFirebaseInitialized)
        {
            yield return null; // Wait for the next frame
        }
        SceneManager.LoadScene("SplashScreen 1");
    }
}
