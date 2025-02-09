using Firebase.Auth;
using Firebase.Firestore;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProgressManager : MonoBehaviour
{
    private FirebaseFirestore db;
    private FirebaseAuth auth;
    private FirebaseUser currentUser;

    [Header("TMP_Text References")]
    [SerializeField] private TMP_Text collection1TitleTMP; // TMP_Text for "1.0" title
    [SerializeField] private TMP_Text collection2TitleTMP; // TMP_Text for "1.1" title
    [SerializeField] private TMP_Text scoreTMP;            // TMP_Text for score
    [SerializeField] private TMP_Text badgeTMP;           // TMP_Text for badge
    [SerializeField] private Button btnFinish;

    private void Start()
    {
        // Initialize Firestore and FirebaseAuth
        db = FirebaseFirestore.DefaultInstance;
        auth = FirebaseAuth.DefaultInstance;

        // Get the current logged-in user
        currentUser = auth.CurrentUser;

        // Add a listener for the button click
        btnFinish.onClick.AddListener(AddDataToFirestore);
    }

    public void AddNestedCollections(string userId)
    {
        // Get the text from TMP_Text components
        string collection1Title = collection1TitleTMP.text;
        string collection2Title = collection2TitleTMP.text;
        int score;
        string badge = badgeTMP.text;

        // Validate score input to avoid parsing errors
        if (!int.TryParse(scoreTMP.text, out score))
        {
            Debug.LogError("Invalid score input. Please make sure it's a numeric value.");
            return;
        }

        // Reference to the user's document
        DocumentReference userDocRef = db.Collection("profile").Document(userId);

        // Add a document in "1.0" collection with a "title" field
        userDocRef.Collection("1.0").AddAsync(new { title = collection1Title }).ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                // Get the reference of the added document in "1.0" collection
                DocumentReference addedDocRef = task.Result;

                // Add a document in "1.1" collection inside the added document
                addedDocRef.Collection("1.1").AddAsync(new
                {
                    title = collection2Title,
                    score = score,
                    badge = badge
                }).ContinueWith(innerTask =>
                {
                    if (innerTask.IsCompleted)
                    {
                        Debug.Log("Nested collection 1.1 added successfully!");
                    }
                    else
                    {
                        Debug.LogError("Failed to add nested collection 1.1: " + innerTask.Exception);
                    }
                });

                // Add an empty document in "1.2" collection inside the added document
                addedDocRef.Collection("1.2").AddAsync(new { }).ContinueWith(innerTask =>
                {
                    if (innerTask.IsCompleted)
                    {
                        Debug.Log("Nested collection 1.2 added successfully!");
                    }
                    else
                    {
                        Debug.LogError("Failed to add nested collection 1.2: " + innerTask.Exception);
                    }
                });
            }
            else
            {
                Debug.LogError("Failed to add document in '1.0': " + task.Exception);
            }
        });
    }

    public void AddDataToFirestore()
    {
        // Ensure a user is logged in
        if (currentUser == null)
        {
            Debug.LogError("No user is logged in!");
            return;
        }

        // Check if the button's text is "Finish"
        TMP_Text buttonText = btnFinish.GetComponentInChildren<TMP_Text>();

        if (buttonText != null && buttonText.text == "Finish")
        {
            string userId = currentUser.UserId; // Get the current user's ID
            AddNestedCollections(userId);
        }
        else
        {
            Debug.Log("Conditions not met: Either the button text is not 'Finish' or no user is logged in.");
        }
    }
}
