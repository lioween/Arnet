using Firebase.Firestore;
using Firebase.Auth;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.EventSystems;
using System;

public class E14Manager : MonoBehaviour
{
    public GameObject pnlPassed;
    public GameObject pnlFailed;
    public GameObject pnlStart;
    public GameObject loadingUI;

    private FirebaseFirestore db;
    private FirebaseAuth auth;
    private FirebaseUser currentUser;

    [Header("Firestore Settings")]
    [SerializeField] private string firestoreCollectionName; // Collection name specified in Inspector
    [SerializeField] private string firestoreDocumentName; // Document name specified in Inspector
    [SerializeField] private string firestoreAddDocument; // Document name specified in Inspector

    public GameObject pnlOptions;  // Panel containing topology buttons
    public GameObject pnlLoading;  // Loading panel before showing the selected topology
    public GameObject pnlStar;     // Panel for Star Topology
    public GameObject pnlBus;      // Panel for Bus Topology
    public GameObject pnlRing;     // Panel for Ring Topology
    public GameObject pnlMesh;     // Panel for Mesh Topology
    public GameObject pnlHybrid;   // Panel for Hybrid Topology
    public Button nextButton;    // Finish button

    // Buttons for each topology
    public Button btnStar;
    public Button btnBus;
    public Button btnRing;
    public Button btnMesh;
    public Button btnHybrid;

    private Button selectedButton = null; // Stores the selected button


    void Start()
    {
        // Initialize Firebase
        db = FirebaseFirestore.DefaultInstance;
        auth = FirebaseAuth.DefaultInstance;
        currentUser = auth.CurrentUser;

        if (currentUser == null)
        {
            Debug.LogError("No user is logged in!");
            return;
        }

        CheckIfUserHasStatus();

        if (pnlOptions != null) pnlOptions.SetActive(true);
        if (pnlLoading != null) pnlLoading.SetActive(false);
        if (pnlStar != null) pnlStar.SetActive(false);
        if (pnlBus != null) pnlBus.SetActive(false);
        if (pnlRing != null) pnlRing.SetActive(false);
        if (pnlMesh != null) pnlMesh.SetActive(false);
        if (pnlHybrid != null) pnlHybrid.SetActive(false);

        // Disable the Finish button initially
        if (nextButton != null) nextButton.interactable = false;
    }

    public void SelectTopology(Button button)
    {
        selectedButton = button;

        // Debugging
        Debug.Log("Selected Button: " + selectedButton.name);

        // Enable the Finish button
        if (nextButton != null)
        {
            nextButton.interactable = true;
        }
    }

    // Called when Finish button is clicked
    public void OnFinishClicked()
    {
        if (selectedButton == null) return; // Prevents errors

        // Show loading panel
        if (pnlLoading != null) pnlLoading.SetActive(true);

        // Hide options panel
        if (pnlOptions != null) pnlOptions.SetActive(false);

        // Start coroutine to display the correct panel after a short delay
        StartCoroutine(ShowSelectedPanel());
    }

    private IEnumerator ShowSelectedPanel()
    {
        yield return new WaitForSeconds(1f); // Simulating loading time

        // Hide the loading panel
        if (pnlLoading != null) pnlLoading.SetActive(false);

        // Show the correct panel based on the selected button
        if (selectedButton == btnStar && pnlStar != null) pnlStar.SetActive(true);
        else if (selectedButton == btnBus && pnlBus != null) pnlBus.SetActive(true);
        else if (selectedButton == btnRing && pnlRing != null) pnlRing.SetActive(true);
        else if (selectedButton == btnMesh && pnlMesh != null) pnlMesh.SetActive(true);
        else if (selectedButton == btnHybrid && pnlHybrid != null) pnlHybrid.SetActive(true);
    }

    public void CheckIfUserHasStatus()
    {
        FirebaseUser currentUser = auth.CurrentUser;

        if (currentUser == null)
        {
            Debug.LogError("No user is signed in.");
            return;
        }

        string userId = currentUser.UserId;

        // Reference to the Firestore collection where scores are stored
        DocumentReference scoreDocRef = db.Collection("profile").Document(userId).Collection(firestoreCollectionName).Document(firestoreDocumentName);

        StartCoroutine(CheckStatusCoroutine(scoreDocRef));
    }

    private IEnumerator CheckStatusCoroutine(DocumentReference statusDocRef)
    {
        loadingUI.SetActive(true);
        // Fetch the user's status document
        var task = statusDocRef.GetSnapshotAsync();

        // Wait for the task to complete
        yield return new WaitUntil(() => task.IsCompleted);

        loadingUI.SetActive(false);

        if (task.IsFaulted || task.IsCanceled)
        {
            Debug.LogError("Failed to check for user status: " + task.Exception);
            yield break;
        }

        DocumentSnapshot snapshot = task.Result;

        if (snapshot.Exists)
        {
            // If the status document exists, retrieve the status
            if (snapshot.TryGetValue("status", out string userStatus))
            {
                // Check if the user passed or failed based on the status
                if (userStatus.Equals("passed", StringComparison.OrdinalIgnoreCase))
                {
                    pnlPassed.SetActive(true);
                    pnlFailed.SetActive(false);
                    pnlStart.SetActive(false);
                }
                else if (userStatus.Equals("failed", StringComparison.OrdinalIgnoreCase))
                {
                    pnlPassed.SetActive(false);
                    pnlFailed.SetActive(true);
                    pnlStart.SetActive(false);
                }
                else
                {
                    Debug.LogWarning("Invalid status value.");
                    pnlPassed.SetActive(false);
                    pnlFailed.SetActive(false);
                }
            }
            else
            {
                Debug.LogWarning("Status document exists but 'status' field is missing.");
                pnlStart.SetActive(true);
                pnlPassed.SetActive(false);
                pnlFailed.SetActive(false);
            }
        }
        else
        {
            Debug.Log("No status document found for this user. Redirecting to quiz start.");
            pnlStart.SetActive(true);
            pnlPassed.SetActive(false);
            pnlFailed.SetActive(false);
        }
    }
}
