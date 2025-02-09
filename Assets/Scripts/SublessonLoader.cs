using Firebase.Firestore;
using Firebase.Auth;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SublessonLoader : MonoBehaviour
{
    private FirebaseFirestore db;
    private FirebaseAuth auth;
    private FirebaseUser currentUser;

    [Header("Firestore Settings")]
    [SerializeField] private string firestoreCollectionName; // Specify the Firestore collection name

    [Header("UI Settings")]
    [SerializeField] private ScrollRect scrollView; // Reference to the ScrollView component
    [SerializeField] private GameObject loadingUI; // Reference to the loading UI

    private int pendingTasks = 0; // Track the number of Firestore queries

    private void Start()
    {
        // Initialize Firestore and FirebaseAuth
        db = FirebaseFirestore.DefaultInstance;
        auth = FirebaseAuth.DefaultInstance;
        currentUser = auth.CurrentUser;

        if (currentUser == null)
        {
            Debug.LogError("No user is logged in!");
            return;
        }

        if (scrollView == null)
        {
            Debug.LogError("ScrollView is not assigned in the Inspector!");
            return;
        }

        if (loadingUI == null)
        {
            Debug.LogError("Loading UI is not assigned in the Inspector!");
            return;
        }

        // Show the loading UI
        loadingUI.SetActive(true);

        // Find all Button components, even non-interactable ones
        Button[] buttons = scrollView.content.GetComponentsInChildren<Button>(true); // Pass `true` to include inactive buttons

        // Track the number of pending tasks
        pendingTasks = buttons.Length;

        foreach (Button button in buttons)
        {
            button.interactable = false; // Disable interaction by default
            StartCoroutine(CheckDocumentExists(button));
        }
    }

    private IEnumerator CheckDocumentExists(Button button)
    {
        string buttonName = button.name; // Get the button's name

        // Ensure currentUser is not null (safety check)
        if (currentUser == null)
        {
            Debug.LogError("No user is logged in! Cannot check Firestore.");
            yield break;
        }

        string userId = currentUser.UserId; // Get the current user's ID

        // Reference to the Firestore collection for the current user
        CollectionReference collectionRef = db.Collection("profile").Document(userId).Collection(firestoreCollectionName);

        // Query Firestore for the document
        var queryTask = collectionRef.Document(buttonName).GetSnapshotAsync();

        // Wait for the query to complete
        yield return new WaitUntil(() => queryTask.IsCompleted);

        if (queryTask.IsFaulted || queryTask.IsCanceled)
        {
            Debug.LogError($"Failed to check Firestore for document '{buttonName}': {queryTask.Exception}");
        }
        else
        {
            DocumentSnapshot documentSnapshot = queryTask.Result;

            if (documentSnapshot.Exists)
            {
                // If the document exists, make the button interactable
                button.interactable = true;
                Debug.Log($"Button '{buttonName}' is now interactable because a matching document was found for user '{userId}'.");
            }
            else
            {
                // If the document does not exist, disable interaction
                button.interactable = false;
                Debug.Log($"Button '{buttonName}' is not interactable because no matching document was found for user '{userId}'.");
            }
        }

        // Decrement the pending task count
        pendingTasks--;

        // If all tasks are completed, hide the loading UI
        if (pendingTasks <= 0)
        {
            loadingUI.SetActive(false);
            Debug.Log("All Firestore queries completed. Loading UI hidden.");
        }
    }
}
