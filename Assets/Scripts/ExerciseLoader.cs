using UnityEngine;
using UnityEngine.UI;
using Firebase.Auth;
using Firebase.Firestore;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class ExerciseLoader : MonoBehaviour
{
    private FirebaseAuth auth;
    private FirebaseFirestore firestore;
    private FirebaseUser user;
    public GameObject loadingUI;

    public RectTransform contentRectTransform;
    public GameObject lastPnlexercise;

    public float heightIfActive = 2191.954f;
    public float heightIfInactive = 2880.225f;

    [Header("UI Settings")]
    [SerializeField] private ScrollRect scrollView; // Reference to the ScrollView component

    void Start()
    {
        // Initialize Firebase
        auth = FirebaseAuth.DefaultInstance;
        firestore = FirebaseFirestore.DefaultInstance;

        // Check if the user is logged in
        user = auth.CurrentUser;
        if (user == null)
        {
            Debug.LogWarning("No user is currently logged in.");
            return;
        }

        // Disable all buttons initially and check Firestore for each button
        if (scrollView != null)
        {
            Button[] buttons = scrollView.content.GetComponentsInChildren<Button>(true);

            foreach (Button button in buttons)
            {
                button.interactable = false; // Disable buttons by default
                CheckCollectionOrDocumentExists(button);
            }
        }
        else
        {
            Debug.LogError("ScrollView is not assigned in the Inspector.");
        }
    }

    private void CheckCollectionOrDocumentExists(Button button)
    {
        loadingUI.SetActive(true);

        if (user == null)
        {
            Debug.LogError("No user is logged in! Cannot check Firestore.");
            return;
        }

        string userId = user.UserId;
        string buttonName = button.name; // Use button's name as the reference

        // Collection Reference
        CollectionReference collectionRef = firestore.Collection("profile").Document(userId).Collection(buttonName);

        // List of document references for different versions (1.0 - 5.0)
        List<DocumentReference> documentRefs = new List<DocumentReference>
    {
        firestore.Collection("profile").Document(userId).Collection("1.0").Document(buttonName),
        firestore.Collection("profile").Document(userId).Collection("2.0").Document(buttonName),
        firestore.Collection("profile").Document(userId).Collection("3.0").Document(buttonName),
        firestore.Collection("profile").Document(userId).Collection("4.0").Document(buttonName),
        firestore.Collection("profile").Document(userId).Collection("5.0").Document(buttonName),
        firestore.Collection("profile").Document(userId).Collection("6.0").Document(buttonName),
        firestore.Collection("profile").Document(userId).Collection("7.0").Document(buttonName),
        firestore.Collection("profile").Document(userId).Collection("8.0").Document(buttonName),
        firestore.Collection("profile").Document(userId).Collection("9.0").Document(buttonName),
        firestore.Collection("profile").Document(userId).Collection("10.0").Document(buttonName)
    };

        StartCoroutine(CheckFirestoreData(collectionRef, documentRefs, button));
    }

    private IEnumerator CheckFirestoreData(CollectionReference collectionRef, List<DocumentReference> documentRefs, Button button)
    {
        // Show the loading panel at the start

        // Step 1: Check if the collection exists (has any documents)
        var collectionTask = collectionRef.Limit(1).GetSnapshotAsync();
        yield return new WaitUntil(() => collectionTask.IsCompleted);

        if (!collectionTask.IsFaulted && !collectionTask.IsCanceled)
        {
            QuerySnapshot collectionSnapshot = collectionTask.Result;
            if (collectionSnapshot.Count > 0)
            {
                button.interactable = true;
                Debug.Log($"✅ Collection '{collectionRef.Id}' exists for button '{button.name}'.");
            }
        }
        else
        {
            Debug.Log($"❌ Collection '{collectionRef.Id}' does not exist for button '{button.name}'.");
        }

        Transform txtResultTransform = button.transform.Find("txtResult");
        TMP_Text txtResult = txtResultTransform != null ? txtResultTransform.GetComponent<TMP_Text>() : null;

        bool allDocumentsChecked = true;  // Flag to track if all documents have been processed

        // Step 2: Check if any of the documents exist
        foreach (var documentRef in documentRefs)
        {
            var documentTask = documentRef.GetSnapshotAsync();
            yield return new WaitUntil(() => documentTask.IsCompleted);

            if (!documentTask.IsFaulted && !documentTask.IsCanceled)
            {
                DocumentSnapshot documentSnapshot = documentTask.Result;
                if (documentSnapshot.Exists)
                {
                    button.interactable = true;
                    Debug.Log($"✅ Document '{documentRef.Id}' exists for button '{button.name}'.");

                    if (documentSnapshot.ContainsField("status"))
                    {
                        string status = documentSnapshot.GetValue<string>("status");
                        Debug.Log($"📜 Status field found: {status}");

                        if (status == "passed" && txtResult != null)
                        {
                            txtResult.text = "Passed"; // Update UI inside button
                            Debug.Log($"🎉 Button '{button.name}' has status PASSED.");
                        }
                        else if (status == "failed" && txtResult != null)
                        {
                            txtResult.text = "<color=red>Failed</color>"; // Update UI inside button
                            Debug.Log($"🎉 Button '{button.name}' has status PASSED.");
                        }
                    }
                    else
                    {
                        txtResult.text = "";
                    }
                }
            }
            else
            {
                allDocumentsChecked = false;  // If any document has a fault or error, set this to false
                Debug.LogWarning($"Error checking document: {documentRef.Id}");
            }
        }

        // Hide the loading panel after everything is done or if error occurred
        if (allDocumentsChecked)
        {
            Debug.Log("✅ All documents have been checked.");
        }
        else
        {
            Debug.LogWarning("⚠️ Some documents couldn't be checked.");
        }

        loadingUI.SetActive(false);  // Hide the loading panel when everything is done
    }




    public void lastButton()
    {
        if (contentRectTransform != null)
        {
            // Get the current width of the content
            float currentWidth = contentRectTransform.sizeDelta.x;

            // Check if the last panel is active or not in the scene (even if it's disabled in the inspector)
            bool isPanelActive = lastPnlexercise.activeInHierarchy;

            // Decide the new height based on the active state of the panel
            float newHeight = isPanelActive ? heightIfInactive : heightIfActive;

            // Set the new size (width remains the same, adjust height)
            contentRectTransform.sizeDelta = new Vector2(currentWidth, newHeight);
            // Auto-scroll to the bottom after resizing
            StartCoroutine(ScrollToBottom());
        }
    }

    IEnumerator ScrollToBottom()
    {
        // Wait for the next frame so that Unity updates the layout
        yield return new WaitForEndOfFrame();

        // Set the vertical scroll position to 0 (bottom)
        scrollView.verticalNormalizedPosition = 0f;
    }
}

