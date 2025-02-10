using Firebase.Auth;
using Firebase.Firestore;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class LessonAdder : MonoBehaviour
{
    private FirebaseFirestore db;
    private FirebaseAuth auth;
    private FirebaseUser currentUser;

    [Header("Firestore Settings")]
    [SerializeField] private string collectionName = "1.0"; // Specify the collection name explicitly
    [SerializeField] private string documentName = "1.1"; // Editable in the Inspector

    private void Start()
    {
        // Initialize Firebase Firestore and Auth
        db = FirebaseFirestore.DefaultInstance;
        auth = FirebaseAuth.DefaultInstance;

        // Get the current logged-in user
        currentUser = auth.CurrentUser;

        if (currentUser == null)
        {
            Debug.LogError("No user is logged in!");
            return;
        }

        // Add listener to the button click
        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(() => StartCoroutine(AddCollection()));
        }
        else
        {
            Debug.LogError("No Button component found on this GameObject!");
        }
    }

    public void AddLesson()
    {
        StartCoroutine(AddCollection());
    }

    private IEnumerator AddCollection()
    {
        if (currentUser == null)
        {
            Debug.LogError("No user is logged in! Cannot add collection.");
            yield break;
        }

        string userId = currentUser.UserId; // Get the current user's ID

        // Reference to the "profile" collection for the current user
        DocumentReference userProfileRef = db.Collection("profile").Document(userId);

        // Check if the specified collection already exists
        var task = userProfileRef.Collection(collectionName).GetSnapshotAsync();
        yield return new WaitUntil(() => task.IsCompleted); // Wait for the task to complete

        SceneManager.LoadScene("Home");

        if (task.IsFaulted || task.IsCanceled)
        {
            Debug.LogError("Failed to check for existing collection: " + task.Exception);
            yield break;
        }

        QuerySnapshot querySnapshot = task.Result;

        if (querySnapshot.Count == 0) // If the collection is empty
        {
            // Add an empty document to create the collection
            var innerTask = userProfileRef.Collection(collectionName).Document(documentName).SetAsync(new { });
            yield return new WaitUntil(() => innerTask.IsCompleted); // Wait for the inner task to complete

            if (innerTask.IsFaulted || innerTask.IsCanceled)
            {
                Debug.LogError("Failed to add collection: " + innerTask.Exception);
                yield break;
            }

            Debug.Log("Collection with name '" + collectionName + "' has been added.");
        }
        else
        {
            Debug.Log("Collection '" + collectionName + "' already exists.");
        }
    }

}
