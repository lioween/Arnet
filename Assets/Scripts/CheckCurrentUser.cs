using UnityEngine;
using Firebase.Auth;
using UnityEngine.SceneManagement;
using System.Collections;

public class CheckCurrentUser : MonoBehaviour
{
    private FirebaseAuth auth;

    

    private void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        StartCoroutine(CheckUserAfterSceneLoad());
    }

    private IEnumerator CheckUserAfterSceneLoad()
    {
        // Optional: Display a loading screen here if needed.

        yield return new WaitForSeconds(0.5f); // Simulate a delay for user experience (e.g., splash screen).

        // Check if the user is authenticated
        if (auth.CurrentUser != null)
        {
            Debug.Log($"User already signed in: {auth.CurrentUser.Email}");
            SceneManager.LoadScene("Home");
        }
        else
        {
            Debug.Log("No user signed in. Redirecting to Login.");
            SceneManager.LoadScene("Login");
        }
    }
}
