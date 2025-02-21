using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Firebase.Firestore;
using Firebase.Auth;
using UnityEngine.EventSystems;
using System;

public class DragAndArrange : MonoBehaviour
{
    public GameObject pnlButtons; // 🔹 The panel that contains the buttons
    public List<Button> correctOrder; // Assign actual button references in Inspector
    private List<Button> buttons = new List<Button>();

    private Dictionary<Button, int> buttonIndexMap = new Dictionary<Button, int>();

    public GameObject pnlStart;
    public GameObject pnlPassed;
    public GameObject pnlFailed;
    public GameObject pnlLoading;

    private FirebaseFirestore db;
    private FirebaseAuth auth;
    private FirebaseUser currentUser;

    [Header("Firestore Settings")]
    [SerializeField] private string firestoreCollectionName;
    [SerializeField] private string firestoreDocumentName;
    [SerializeField] private string AddCollection;
    [SerializeField] private string AddDocument;

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

        if (pnlButtons == null)
        {
            Debug.LogError("No pnlButtons assigned! Assign the panel containing buttons.");
            return;
        }

        // 🔹 Find all Buttons inside the specified panel
        foreach (Transform child in pnlButtons.transform)
        {
            Button btn = child.GetComponent<Button>();
            if (btn != null)
            {
                buttons.Add(btn);
                AddDragEvents(btn);
            }
            else
            {
                Debug.LogWarning($"No Button component found on {child.name}! Skipping this object.");
            }
        }

        CheckIfUserHasStatus();
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
        DocumentReference scoreDocRef = db.Collection("profile").Document(userId).Collection(firestoreCollectionName).Document(firestoreDocumentName);

        StartCoroutine(CheckStatusCoroutine(scoreDocRef));
    }

    private IEnumerator CheckStatusCoroutine(DocumentReference statusDocRef)
    {
        pnlLoading.SetActive(true);
        var task = statusDocRef.GetSnapshotAsync();
        yield return new WaitUntil(() => task.IsCompleted);
        pnlLoading.SetActive(false);

        if (task.IsFaulted || task.IsCanceled)
        {
            Debug.LogError("Failed to check for user status: " + task.Exception);
            yield break;
        }

        DocumentSnapshot snapshot = task.Result;

        if (snapshot.Exists && snapshot.TryGetValue("status", out string userStatus))
        {
            pnlStart.SetActive(false);
            pnlPassed.SetActive(userStatus.Equals("passed", StringComparison.OrdinalIgnoreCase));
            pnlFailed.SetActive(userStatus.Equals("failed", StringComparison.OrdinalIgnoreCase));
        }
        else
        {
            pnlStart.SetActive(true);
        }
    }

    public void StartQuiz()
    {
        pnlStart.SetActive(false);
        ShuffleButtons();
    }

    void ShuffleButtons()
    {
        for (int i = buttons.Count - 1; i > 0; i--)
        {
            int randomIndex = UnityEngine.Random.Range(0, i + 1);
            (buttons[i], buttons[randomIndex]) = (buttons[randomIndex], buttons[i]); // Swap
        }

        for (int i = 0; i < buttons.Count; i++)
        {
            buttons[i].transform.SetSiblingIndex(i);
        }
    }

    void AddDragEvents(Button btn)
    {
        EventTrigger trigger = btn.gameObject.GetComponent<EventTrigger>() ?? btn.gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry dragStart = new EventTrigger.Entry { eventID = EventTriggerType.BeginDrag };
        dragStart.callback.AddListener((data) => OnDragStart((PointerEventData)data));
        trigger.triggers.Add(dragStart);

        EventTrigger.Entry dragging = new EventTrigger.Entry { eventID = EventTriggerType.Drag };
        dragging.callback.AddListener((data) => OnDragging((PointerEventData)data));
        trigger.triggers.Add(dragging);

        EventTrigger.Entry dragEnd = new EventTrigger.Entry { eventID = EventTriggerType.EndDrag };
        dragEnd.callback.AddListener((data) => OnDragEnd((PointerEventData)data));
        trigger.triggers.Add(dragEnd);
    }

    void OnDragStart(PointerEventData eventData)
    {
        eventData.pointerDrag.transform.SetAsLastSibling();
    }

    void OnDragging(PointerEventData eventData)
    {
        eventData.pointerDrag.transform.position = eventData.position;
    }

    void OnDragEnd(PointerEventData eventData)
    {
        RectTransform draggedButton = eventData.pointerDrag.GetComponent<RectTransform>();

        Button closestButton = null;
        float minDistance = float.MaxValue;

        foreach (Button btn in buttons)
        {
            if (btn.gameObject == eventData.pointerDrag) continue;

            float distance = Vector3.Distance(btn.transform.position, draggedButton.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestButton = btn;
            }
        }

        if (closestButton != null)
        {
            int indexA = buttons.IndexOf(closestButton);
            int indexB = buttons.IndexOf(eventData.pointerDrag.GetComponent<Button>());

            (buttons[indexA], buttons[indexB]) = (buttons[indexB], buttons[indexA]); // Swap

            buttons[indexA].transform.SetSiblingIndex(indexA);
            buttons[indexB].transform.SetSiblingIndex(indexB);
        }
    }

    public void ValidateOrder()
    {
        StartCoroutine(ValidateOrderCoroutine());
    }

    public IEnumerator ValidateOrderCoroutine()
    {
        bool isCorrect = true;

        for (int i = 0; i < buttons.Count; i++)
        {
            if (buttons[i] != correctOrder[i]) // Compare actual buttons, not text
            {
                buttons[i].GetComponent<Image>().color = new Color32(170, 11, 0, 255);
                isCorrect = false;
            }
            else
            {
                buttons[i].GetComponent<Image>().color = new Color32(24, 115, 115, 255);
            }
        }

        if (isCorrect)
        {
            Debug.Log("Correct Order!");

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

                if (!firestoreDocumentName.Contains(".5"))
                {
                    // Additional document (if no ".5" in AddDocument)
                    DocumentReference additionalDocRef = db.Collection("profile").Document(userId).Collection(firestoreCollectionName).Document(AddDocument);

                    var additionalTask = additionalDocRef.SetAsync(new { });
                    yield return new WaitUntil(() => additionalTask.IsCompleted);

                    if (additionalTask.IsFaulted || additionalTask.IsCanceled)
                    {
                        Debug.LogError("Failed to save additional empty document: " + additionalTask.Exception);
                        yield break;
                    }
                    Debug.Log("Additional empty document saved to Firestore.");
                }
                else
                {
                    // If AddDocument contains ".5", check and create a collection if it doesn't exist
                    DocumentReference userProfileRef = db.Collection("profile").Document(userId);
                    QuerySnapshot querySnapshot = null;

                    var checkCollectionTask = userProfileRef.Collection(AddCollection).GetSnapshotAsync();
                    yield return new WaitUntil(() => checkCollectionTask.IsCompleted);

                    if (checkCollectionTask.IsFaulted || checkCollectionTask.IsCanceled)
                    {
                        Debug.LogError("Failed to check for existing collection: " + checkCollectionTask.Exception);
                        yield break;
                    }

                    querySnapshot = checkCollectionTask.Result;

                    if (querySnapshot.Count == 0) // If collection does not exist, create it
                    {
                        DocumentReference newCollectionDoc = userProfileRef.Collection(AddCollection).Document(AddDocument);
                        var createCollectionTask = newCollectionDoc.SetAsync(new { });

                        yield return new WaitUntil(() => createCollectionTask.IsCompleted);

                        if (createCollectionTask.IsFaulted || createCollectionTask.IsCanceled)
                        {
                            Debug.LogError("Failed to create new collection and document: " + createCollectionTask.Exception);
                            yield break;
                        }
                        Debug.Log($"Created collection '{AddCollection}' and document '{AddDocument}'.");
                    }
                    else
                    {
                        Debug.Log($"Collection '{AddCollection}' already exists, no changes made.");
                    }
                }
            }
            else
            {
                Debug.LogError("No user is logged in! Cannot save results.");
            }

            pnlLoading.SetActive(true);
            yield return new WaitForSeconds(1.5f);
            pnlLoading.SetActive(false);
            pnlPassed.SetActive(true);
        }
        else
        {
            Debug.Log("Incorrect Order. Try Again.");
        }
    }
}