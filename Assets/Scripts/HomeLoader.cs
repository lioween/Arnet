using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase.Extensions;
using Firebase.Auth;
using Firebase.Firestore;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

public class DisplayFirstName : MonoBehaviour
{
    [SerializeField] private TMP_Text firstNameText; // Assign TMP Text object in the inspector

    private FirebaseAuth auth;
    private FirebaseFirestore firestore;
    private FirebaseUser user;
    public GameObject pnlTutorial;
    public GameObject loadingUI;

    public Slider progressSlider; // Assign the slider in the Unity Inspector
    public TMP_Text progressText;     // Assign a Text UI element to show the percentage

    [Header("Firestore Settings")]
    public string profileCollectionName = "profile";  // Main collection (e.g., "profile")
    public string nestedCollectionName = "1.0"; // Replace with your nested collection name


    [Header("UI Settings")]
    [SerializeField] private ScrollRect scrollView; // Reference to the ScrollView component
    public ScrollRect scrollProgress;

    void Start()
    {
        // Initialize Firebase
        auth = FirebaseAuth.DefaultInstance;
        firestore = FirebaseFirestore.DefaultInstance;

        // Check if the user is logged in
        user = auth.CurrentUser;
        if (user != null)
        {
            string userId = user.UserId;
            StartCoroutine(FetchAndDisplayFirstName(userId));
            StartCoroutine(CheckButtonsVisibility());
            StartCoroutine(LoadTutorial(userId));

        }
        else
        {
            Debug.LogWarning("No user is currently logged in.");
            firstNameText.text = "Guest"; // Default text for guests
        }

        // Disable all buttons initially
        if (scrollView != null)
        {
            Button[] buttons = scrollView.content.GetComponentsInChildren<Button>(true);

            foreach (Button button in buttons)
            {
                button.interactable = false;
                CheckCollectionExists(button);
            }
        }
        else
        {
            Debug.LogError("ScrollView is not assigned in the Inspector.");
        }
    }

    private IEnumerator LoadTutorial(string userId)
    {

        loadingUI.SetActive(true);
        CollectionReference collectionRef = firestore.Collection("profile").Document(userId).Collection("1.0");
        var queryTask = collectionRef.Limit(1).GetSnapshotAsync();

        yield return new WaitUntil(() => queryTask.IsCompleted);
        loadingUI.SetActive(false);

        if (queryTask.Exception != null)
        {
            Debug.LogError($"❌ Error checking Firestore collection: {queryTask.Exception}");
            yield break;
        }

        QuerySnapshot querySnapshot = queryTask.Result;

        if (querySnapshot.Count == 0)
        {
                pnlTutorial.SetActive(true);
        }
        else
        {
                pnlTutorial.SetActive(false);
        }
    }

    private IEnumerator FetchAndDisplayFirstName(string userId)
    {
        if (loadingUI != null) loadingUI.SetActive(true);

        // Firestore collection reference
        DocumentReference docRef = firestore.Collection("profile").Document(userId);

        Task<DocumentSnapshot> task = docRef.GetSnapshotAsync();
        yield return new WaitUntil(() => task.IsCompleted); // Wait until the task completes

        if (loadingUI != null) loadingUI.SetActive(false);

        if (task.IsFaulted)
        {
            Debug.LogError("Error fetching Firestore document: " + task.Exception?.Message);
            firstNameText.text = "Error"; // Display an error message
            yield break;
        }

        if (task.Result != null && task.Result.Exists)
        {
            // Retrieve the first name
            if (task.Result.TryGetValue("firstname", out string firstName))
            {
                firstNameText.text = firstName + "!";
            }
            else
            {
                Debug.LogWarning("First name field not found in Firestore.");
                firstNameText.text = "User";
            }
        }
        else
        {
            Debug.LogWarning("Firestore document does not exist.");
            firstNameText.text = "Guest"; // Default message if the document doesn't exist
        }
    }

    private void CheckCollectionExists(Button button)
    {
        string buttonName = button.name; // Get the button's name

        loadingUI.SetActive(true);
        // Ensure currentUser is not null (safety check)
        if (user == null)
        {
            Debug.LogError("No user is logged in! Cannot check Firestore.");
            return;
        }

        string userId = user.UserId; // Get the current user's ID

        // Reference to the Firestore collection for the current user
        CollectionReference collectionRef = firestore.Collection("profile").Document(userId).Collection(buttonName);

        // Start the coroutine to check if the collection exists
        StartCoroutine(CheckCollectionExistsCoroutine(collectionRef, button));
    }

    private IEnumerator CheckCollectionExistsCoroutine(CollectionReference collectionRef, Button button)
    {
        // Query the collection to check if it contains any documents
        var queryTask = collectionRef.Limit(1).GetSnapshotAsync();

        // Wait until the task is completed
        yield return new WaitUntil(() => queryTask.IsCompleted);
        loadingUI.SetActive(false);

        if (queryTask.IsFaulted)
        {
            Debug.LogError($"Error checking collection existence: {queryTask.Exception}");
            loadingUI.SetActive(false); // Ensure loading UI is turned off
            yield break;
        }

        QuerySnapshot querySnapshot = queryTask.Result;

        // Find the Image child of the button
        Transform childTransform = button.transform.Find("imgLock"); // Replace "imgLock" with the actual child name
        if (childTransform != null)
        {
            GameObject childImage = childTransform.gameObject;

            // Update the button interactability and child image based on the collection's existence
            if (querySnapshot.Count > 0)
            {
                button.interactable = true; // Collection exists
                if (childImage != null)
                {
                    childImage.SetActive(false); // Hide the child image
                }
                Debug.Log($"Button '{button.name}' is now interactable, and the child image has been deactivated.");

                // Activate the other button if the collection exists
                ActivateOtherButton("d" + button.name, false);
            }
            else
            {
                button.interactable = false; // Collection does not exist
                if (childImage != null)
                {
                    childImage.SetActive(true); // Show the child image
                }
                Debug.Log($"Button '{button.name}' is not interactable, and the child image has been activated.");

                // Activate the other button as a fallback
                ActivateOtherButton("d" + button.name, true);
            }
        }
        else
        {
            Debug.LogWarning($"Child image not found under button '{button.name}'. Ensure the child is named correctly.");
        }

        // Ensure loading UI is turned off after all updates
    }
    private void ActivateOtherButton(string otherButtonName, bool setActive)
    {
        Button otherButton = GameObject.Find(otherButtonName)?.GetComponent<Button>();
        if (otherButton != null)
        {
            otherButton.gameObject.SetActive(setActive);
            otherButton.interactable = true;
            Debug.Log($"Other button '{otherButton.name}' has been set to active: {setActive}.");
        }
        else
        {
            Debug.LogWarning($"Other button '{otherButtonName}' not found.");
        }
    }

    private IEnumerator CheckButtonsVisibility()
    {
        // Show the loading UI
        loadingUI.SetActive(true);

        // Ensure the user is authenticated
        FirebaseUser user = auth.CurrentUser;

        if (user == null)
        {
            Debug.LogError("No user is currently signed in.");
            loadingUI.SetActive(false); // Hide the loading UI
            yield break;
        }

        string userId = user.UserId; // Get the authenticated user's ID

        // Reference the user's document in the profile collection
        DocumentReference userDocRef = firestore.Collection(profileCollectionName).Document(userId);

        // Get all the child buttons in the ScrollView
        Button[] buttons = scrollProgress.content.GetComponentsInChildren<Button>();

        // Create a list to hold all the Firebase tasks
        List<Task<QuerySnapshot>> tasks = new List<Task<QuerySnapshot>>();

        // Start tasks for each button
        foreach (Button button in buttons)
        {
            string buttonName = button.name;

            // Start a Firebase query for the button and add it to the task list
            var task = userDocRef.Collection(buttonName).GetSnapshotAsync();
            tasks.Add(task);
        }

        // Wait until all tasks are completed
        foreach (var task in tasks)
        {
            yield return new WaitUntil(() => task.IsCompleted);
        }

        // Process the results
        for (int i = 0; i < buttons.Length; i++)
        {
            Button button = buttons[i];
            Task<QuerySnapshot> task = tasks[i];

            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError($"Failed to check collection for button '{button.name}': {task.Exception}");
                continue;
            }

            // Handle the result
            QuerySnapshot querySnapshot = task.Result;
            if (querySnapshot.Count > 0)
            {
                // Collection exists, ensure the button is visible
                button.gameObject.SetActive(true);
            }
            else
            {
                // Collection does not exist, hide the button
                button.gameObject.SetActive(false);
            }
        }

        // Hide the loading UI after all tasks are completed
        scrollProgress.gameObject.SetActive(true);
        StartCoroutine(UpdateProgressForAllCollections());
        loadingUI.SetActive(false);
    }


    private IEnumerator UpdateProgressForAllCollections()
    {
        // Show the loading UI
        loadingUI.SetActive(true);

        // Ensure the user is authenticated
        FirebaseUser user = auth.CurrentUser;

        if (user == null)
        {
            Debug.LogError("No user is currently signed in.");
            loadingUI.SetActive(false); // Hide the loading UI
            yield break;
        }

        string userId = user.UserId; // Get the authenticated user's ID

        for (float i = 1.0f; i <= 10.0f; i += 1.0f)
        {
            string collectionName = i.ToString("0.0");
            CollectionReference nestedCollectionRef = firestore
                .Collection("profile")
                .Document(userId)
                .Collection(collectionName);

            var task = nestedCollectionRef.GetSnapshotAsync();
            yield return new WaitUntil(() => task.IsCompleted); // Wait for task to complete

            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("Failed to retrieve documents for collection " + collectionName + ": " + task.Exception);
                continue; // Skip to the next collection
            }

            int countPassed = 0;

            foreach (DocumentSnapshot document in task.Result.Documents)
            {
                if (document.Exists && document.TryGetValue("status", out string status) && status == "passed")
                {
                    countPassed++;
                }
            }

            float percentage = Mathf.Clamp((countPassed / 5f) * 100f, 0, 100);
            Debug.Log("Collection " + collectionName + " - Passed Count: " + countPassed + " - Percentage: " + percentage);

            // Find the corresponding UI elements under svProgress
            Transform contentTransform = GameObject.Find("svProgress/Viewport/Content")?.transform;
            if (contentTransform != null)
            {
                Transform collectionTransform = contentTransform.Find(collectionName);
                if (collectionTransform != null)
                {
                    Slider progressSlider = collectionTransform.Find("Slider")?.GetComponent<Slider>();
                    TMP_Text progressText = collectionTransform.Find("txtPercent")?.GetComponent<TMP_Text>();

                    if (progressSlider != null || progressText != null)
                    {
                        UpdateUI(progressSlider, progressText, percentage);
                    }
                    else
                    {
                        Debug.LogWarning("UI elements missing in " + collectionName);
                    }
                }
                else
                {
                    Debug.LogWarning("Collection " + collectionName + " not found under svProgress.");
                }
            }
            else
            {
                Debug.LogError("Scroll View 'svProgress/Viewport/Content' not found.");
            }
        }

        // Hide the loading UI after completion
        loadingUI.SetActive(false);
    }

    private void UpdateUI(Slider progressSlider, TMP_Text progressText, float percentage)
    {
        if (progressSlider != null)
        {
            progressSlider.value = percentage / 100f;
        }

        if (progressText != null)
        {
            progressText.gameObject.SetActive(false); // Force UI refresh
            progressText.text = percentage.ToString("0") + "%";
            progressText.gameObject.SetActive(true);
            Debug.Log("Updated UI - Progress: " + progressText.text + " (" + progressText.gameObject.name + ")");
        }
    }



}
