using Firebase.Firestore;
using Firebase.Auth;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.EventSystems;
using System;

public class E14game : MonoBehaviour
{
    public GameObject[] PCToShow; // Assign objects in the Inspector
    public Button btnPC; // Assign the button
    public TMP_Text PCnumber; // Assign the TMP text inside the button
    private int PCcurrentIndex = 0; // Tracks which object to reveal
    public int PCcount; // Default start number

    public GameObject[] RouterToShow; // Assign objects in the Inspector
    public Button btnRouter; // Assign the button
    public TMP_Text Routernumber; // Assign the TMP text inside the button
    private int RoutercurrentIndex = 0; // Tracks which object to reveal
    public int Routercount; // Default start number

    public GameObject pnlOptions; // Panel containing the topology options
    public GameObject pnlLines; // First panel to appear if correct
    public GameObject pnlPassed; // Second panel to appear after 1.5 sec if correct
    public GameObject pnlFailed; // Panel to appear if the answer is wrong
    public GameObject pnlLoading; // Loading panel (NEW)
    public Button finishButton; // Finish button

    public Button correctButton; // Assign the correct button in the Inspector

    private Button selectedButton = null; // Stores the selected button

    // Firestore variables
    private FirebaseFirestore db;
    private FirebaseAuth auth;
    private FirebaseUser currentUser;

    [Header("Firestore Settings")]
    [SerializeField] private string firestoreCollectionName; // Collection name specified in Inspector
    [SerializeField] private string firestoreDocumentName; // Document name specified in Inspector
    [SerializeField] private string firestoreAddDocument; // Document name specified in Inspector

    void Start()
    {

        db = FirebaseFirestore.DefaultInstance;
        auth = FirebaseAuth.DefaultInstance;
        currentUser = auth.CurrentUser;

        if (currentUser == null)
        {
            Debug.LogError("No user is logged in!");
            return;
        }

        // Hide all objects at the start
        foreach (GameObject obj in PCToShow)
        {
            if (obj != null) obj.SetActive(false);
        }

        foreach (GameObject obj in RouterToShow)
        {
            if (obj != null) obj.SetActive(false);
        }

        // Hide all panels initially
        if (pnlOptions != null) pnlOptions.SetActive(false);
        if (pnlLines != null) pnlLines.SetActive(false);
        if (pnlPassed != null) pnlPassed.SetActive(false);
        if (pnlFailed != null) pnlFailed.SetActive(false);
        if (pnlLoading != null) pnlLoading.SetActive(false);

        // Disable the Finish button initially
        if (finishButton != null) finishButton.interactable = false;
    }

    public void RevealPC()
    {
        if (PCcurrentIndex < PCToShow.Length)
        {
            PCToShow[PCcurrentIndex].SetActive(true); // Show the next object
            PCcurrentIndex++; // Move to the next object
        }

        // Reduce the number and update TMP text
        PCcount--;
        if (PCnumber != null)
        {
            PCnumber.text = PCcount.ToString();
        }

        // Disable button when count reaches 0
        if (PCcount <= 0)
        {
            btnPC.interactable = false;
        }

        // Check if all devices are revealed
        CheckAllDevicesRevealed();
    }

    public void RevealRouter()
    {
        if (RoutercurrentIndex < RouterToShow.Length)
        {
            RouterToShow[RoutercurrentIndex].SetActive(true); // Show the next object
            RoutercurrentIndex++; // Move to the next object
        }

        // Reduce the number and update TMP text
        Routercount--;
        if (Routernumber != null)
        {
            Routernumber.text = Routercount.ToString();
        }

        // Disable button when count reaches 0
        if (Routercount <= 0)
        {
            btnRouter.interactable = false;
        }

        // Check if all devices are revealed
        CheckAllDevicesRevealed();
    }

    private void CheckAllDevicesRevealed()
    {
        bool allPCsRevealed = (PCcount == 0) || (PCcurrentIndex >= PCToShow.Length);
        bool allRoutersRevealed = (Routercount == 0) || (RoutercurrentIndex >= RouterToShow.Length);

        // If all valid devices (PCs and Routers) are revealed, show the topology options panel
        if (allPCsRevealed && allRoutersRevealed)
        {
            if (pnlOptions != null)
            {
                pnlOptions.SetActive(true);
            }
        }
    }


    // Called when a topology button is selected
    public void SelectButton(Button button)
    {
        selectedButton = button;

        // Debug log to check if the function is being called
        Debug.Log("Selected Button: " + selectedButton.name);

        // Ensure the Finish button gets updated
        if (finishButton != null)
        {
            finishButton.interactable = true;  // Enable button after selecting an answer
        }
    }

    // Called when Finish button is clicked
    public void OnFinishClicked()
    {
       

        // Hide the options panel
        if (pnlOptions != null) pnlOptions.SetActive(false);

        if (selectedButton == correctButton)
        {
            // Show first panel immediately
            if (pnlLines != null)
            {
                pnlLines.SetActive(true);
            }

            if (currentUser != null)
            {
                string userId = currentUser.UserId; // Get current user's ID

                // First document reference
                DocumentReference quizDocRef = db.Collection("profile").Document(userId).Collection(firestoreCollectionName).Document(firestoreDocumentName);

                quizDocRef.SetAsync(new
                {
                    
                    status = "passed",
                    timestamp = FieldValue.ServerTimestamp
                }).ContinueWith(task =>
                {
                    if (task.IsCompleted)
                    {
                        Debug.Log($"Quiz results saved to Firestore in collection '{firestoreCollectionName}' with document name '{firestoreDocumentName}'.");
                    }
                    else
                    {
                        Debug.LogError("Failed to save quiz results: " + task.Exception);
                    }
                });

                // Second document reference (additional document without fields)
                DocumentReference additionalDocRef = db.Collection("profile").Document(userId).Collection(firestoreCollectionName).Document(firestoreAddDocument);

                additionalDocRef.SetAsync(new { }).ContinueWith(task =>
                {
                    if (task.IsCompleted)
                    {
                        Debug.Log("Additional empty document saved to Firestore successfully.");
                    }
                    else
                    {
                        Debug.LogError("Failed to save additional empty document: " + task.Exception);
                    }
                });
            }
            else
            {
                Debug.LogError("No user is logged in! Cannot save results.");
            }
        

        StartCoroutine(ShowPassedPanel());

        }
        else
        {
            // If the answer is wrong, show the failed panel after 1.5 seconds
            if (pnlOptions != null) pnlOptions.SetActive(false);

            if (pnlLoading != null) pnlLoading.SetActive(true);

            if (currentUser != null)
            {
                string userId = currentUser.UserId; // Get current user's ID

                // First document reference
                DocumentReference quizDocRef = db.Collection("profile").Document(userId).Collection(firestoreCollectionName).Document(firestoreDocumentName);

                quizDocRef.SetAsync(new
                {
                    
                    status = "failed",
                    timestamp = FieldValue.ServerTimestamp
                }).ContinueWith(task =>
                {
                    if (task.IsCompleted)
                    {
                        Debug.Log($"Quiz results saved to Firestore in collection '{firestoreCollectionName}' with document name '{firestoreDocumentName}'.");
                    }
                    else
                    {
                        Debug.LogError("Failed to save quiz results: " + task.Exception);
                    }
                });

            }
            else
            {
                Debug.LogError("No user is logged in! Cannot save results.");
            }

            StartCoroutine(ShowFailedPanel());
        }
    }

    private IEnumerator ShowPassedPanel()
    {
        yield return new WaitForSeconds(1f);
        if (pnlLoading != null) pnlLoading.SetActive(true);

        yield return new WaitForSeconds(1f);

        // Hide the first panel
        if (pnlLines != null) pnlLines.SetActive(false);

        // Hide loading panel
        if (pnlLoading != null) pnlLoading.SetActive(false);

        // Show the passed panel
        if (pnlPassed != null) pnlPassed.SetActive(true);
    }

    private IEnumerator ShowFailedPanel()
    {
        yield return new WaitForSeconds(1f);

        // Hide loading panel
        if (pnlLoading != null) pnlLoading.SetActive(false);

        // Show the failed panel
        if (pnlFailed != null) pnlFailed.SetActive(true);
    }
}
