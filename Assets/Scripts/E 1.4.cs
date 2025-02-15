using Firebase.Firestore;
using Firebase.Auth;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.EventSystems;
using System;

public class E14 : MonoBehaviour
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

    public GameObject[] PCToShow; // Assign objects in the Inspector
    public Button btnPC; // Assign the button
    public TMP_Text PCnumber; // Assign the TMP text inside the button
    private int PCcurrentIndex = 0; // Tracks which object to reveal
    private int PCcount = 5; // Default start number

    public GameObject[] RouterToShow; // Assign objects in the Inspector
    public Button btnRouter; // Assign the button
    public TMP_Text Routernumber; // Assign the TMP text inside the button
    private int RoutercurrentIndex = 0; // Tracks which object to reveal
    private int Routercount = 1; // Default start number

    public RectTransform panel; // Assign the panel containing draggable objects
    public LineRenderer linePrefab; // Assign a LineRenderer prefab in the Inspector
    private RectTransform draggedObject;
    private Vector3 dragStartPosition;
    private LineRenderer currentLine;
    private List<Connection> connections = new List<Connection>(); // Stores connected objects



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

        CheckIfUserHasScore();

        // Hide all objects at the start
        foreach (GameObject obj in PCToShow)
        {
            if (obj != null) obj.SetActive(false);
        }

        foreach (GameObject obj in RouterToShow)
        {
            if (obj != null) obj.SetActive(false);
        }
    }

    public void CheckIfUserHasScore()
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
    }


    public void OnBeginDrag(PointerEventData eventData)
    {
        draggedObject = eventData.pointerDrag.GetComponent<RectTransform>();
        dragStartPosition = draggedObject.position;

        // Create a new line when dragging starts
        currentLine = Instantiate(linePrefab, panel);
        currentLine.positionCount = 2;
        currentLine.SetPosition(0, dragStartPosition);
        currentLine.SetPosition(1, dragStartPosition);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (draggedObject == null || currentLine == null) return;

        // Update line endpoint as object is dragged
        currentLine.SetPosition(1, Input.mousePosition);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (draggedObject == null || currentLine == null) return;

        // Check if dropped on another object inside the panel
        GameObject droppedOn = GetObjectUnderMouse(eventData);

        if (droppedOn != null && droppedOn != draggedObject.gameObject)
        {
            // Connect dragged object to the dropped-on object
            RectTransform targetObject = droppedOn.GetComponent<RectTransform>();
            currentLine.SetPosition(1, targetObject.position);

            // Save connection
            connections.Add(new Connection(draggedObject, targetObject, currentLine));
        }
        else
        {
            // If not dropped on an object, destroy the line
            Destroy(currentLine.gameObject);
        }

        draggedObject = null;
        currentLine = null;
    }

    private GameObject GetObjectUnderMouse(PointerEventData eventData)
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = eventData.position
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (RaycastResult result in results)
        {
            if (result.gameObject != eventData.pointerDrag) // Ignore self
            {
                return result.gameObject;
            }
        }

        return null;
    }

    // Class to store object connections
    private class Connection
    {
        public RectTransform objectA;
        public RectTransform objectB;
        public LineRenderer line;

        public Connection(RectTransform a, RectTransform b, LineRenderer l)
        {
            objectA = a;
            objectB = b;
            line = l;
        }
    }

}
