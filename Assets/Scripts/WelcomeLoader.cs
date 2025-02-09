using UnityEngine;
using TMPro;
using Firebase.Extensions;
using Firebase.Auth;
using Firebase.Firestore;
using Firebase;

public class WelcomeLoader : MonoBehaviour
{
    [SerializeField] private TMP_Text firstNameText; // Assign TMP Text object in the inspector

    private FirebaseAuth auth;
    private FirebaseFirestore firestore;

    void Start()
    {
        // Initialize Firebase
        auth = FirebaseAuth.DefaultInstance;
        firestore = FirebaseFirestore.DefaultInstance;

        InitializeFirebase();

        // Check if user is logged in
        FirebaseUser user = auth.CurrentUser;
        if (user != null)
        {
            string userId = user.UserId;
            FetchAndDisplayFirstName(userId);
        }
        else
        {
            Debug.LogWarning("No user is currently logged in.");
            firstNameText.text = "Guest"; // Default text for guests
        }
    }

    public async void InitializeFirebase()
    {
        var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
        if (dependencyStatus == DependencyStatus.Available)
        {
            Debug.Log("Firebase initialized successfully.");
        }
        else
        {
            Debug.LogError($"Could not resolve all Firebase dependencies: {dependencyStatus}");
        }
    }
    private void FetchAndDisplayFirstName(string userId)
    {
        // Firestore collection reference
        DocumentReference docRef = firestore.Collection("profile").Document(userId);

        // Fetch document
        docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && task.Result.Exists)
            {
                // Retrieve the first name from Firestore document
                if (task.Result.TryGetValue("firstname", out string firstName))
                {
                    firstNameText.text = firstName + "!"; // Display the first name
                }
                else
                {
                    Debug.LogWarning("First name field not found in Firestore.");
                    firstNameText.text = "User"; // Default text if no first name is found
                }
            }
            else
            {
                Debug.LogError("Failed to fetch user data: " + task.Exception?.Message);
                firstNameText.text = "Error"; // Default text if fetch fails
            }
        });
    }
}
