using Firebase.Firestore;
using Firebase.Auth;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.EventSystems;
using System;

public class E22 : MonoBehaviour
{

    public GameObject pnlTopology;  // Panel containing topology buttons
    public GameObject pnlStar;     // Panel for Star Topology
    public GameObject pnlRing;     // Panel for Ring Topology
    public GameObject pnlHybrid;   // Panel for Hybrid Topology
    public Button nextButton;    // Finish button

    // Buttons for each topology
    public Button btnStar;
    public Button btnRing;
    public Button btnHybrid;

    private Button selectedButton = null; // Stores the selected button

    [Header("Add Devices")]
    public GameObject pnlDevices; // Assign objects in the Inspector

    [Header("Connect Devices")]
    public GameObject pnlConnect;
    public GameObject pnlConnected;

    [Header("Configure Router")]
    public GameObject pnlConfigure;
    public GameObject pnlConfigureRouter;
    public TMP_InputField inpSSID;
    public TMP_InputField inpPw;
    public TMP_Text txtSSID;

    [Header("Assign IP")]
    public GameObject pnlAssign;
    public GameObject pnlAssignNotif;
    public GameObject pnlIP;

    [Header("Ping Devices")]
    public GameObject pnlPing;
    public GameObject pnlPingNotif;

    [Header("Game Settings")]
    public GameObject[] RouterToShow; // Assign objects in the Inspector
    public Button btnRouter; // Assign the button
    public TMP_Text Routernumber; // Assign the TMP text inside the button
    private int RoutercurrentIndex = 0; // Tracks which object to reveal
    public int Routercount; // Default start number

    public GameObject[] PCToShow; // Assign objects in the Inspector
    public Button btnPC; // Assign the button
    public TMP_Text PCnumber; // Assign the TMP text inside the button
    private int PCcurrentIndex = 0; // Tracks which object to reveal
    public int PCcount; // Default start number

    public GameObject[] PrinterToShow; // Assign objects in the Inspector
    public Button btnPrinter; // Assign the button
    public TMP_Text Printernumber; // Assign the TMP text inside the button
    private int PrintercurrentIndex = 0; // Tracks which object to reveal
    public int Printercount; // Default start number

    public GameObject pnlLines; // First panel to appear if correct
    public GameObject pnlPassed; // Second panel to appear after 1.5 sec if correct
    public GameObject pnlFailed; // Panel to appear if the answer is wrong
    public GameObject pnlLoading; // Loading panel (NEW)

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
        foreach (GameObject obj in RouterToShow)
        {
            if (obj != null) obj.SetActive(false);
        }

        foreach (GameObject obj in PCToShow)
        {
            if (obj != null) obj.SetActive(false);
        }

        foreach (GameObject obj in PrinterToShow)
        {
            if (obj != null) obj.SetActive(false);
        }

        // Hide all panels initially
        if (pnlLines != null) pnlLines.SetActive(false);
        if (pnlPassed != null) pnlPassed.SetActive(false);
        if (pnlFailed != null) pnlFailed.SetActive(false);
        if (pnlLoading != null) pnlLoading.SetActive(false);
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
    public void OnNextClicked()
    {
        if (selectedButton == null) return; // Prevents errors

        // Show loading panel
        pnlLoading.SetActive(true);

        // Start coroutine to display the correct panel after a short delay
        StartCoroutine(ShowSelectedPanel());
    }

    private IEnumerator ShowSelectedPanel()
    {
        yield return new WaitForSeconds(1f); // Simulating loading time

        // Hide the loading panel
        pnlLoading.SetActive(false);

        // Show the correct panel based on the selected button
        if (selectedButton == btnStar && pnlStar != null) pnlStar.SetActive(true);
        else if (selectedButton == btnRing && pnlRing != null) pnlRing.SetActive(true);
        else if (selectedButton == btnHybrid && pnlHybrid != null) pnlHybrid.SetActive(true);
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

    public void RevealPrinter()
    {
        if (PrintercurrentIndex < PrinterToShow.Length)
        {
            PrinterToShow[PrintercurrentIndex].SetActive(true); // Show the next object
            PrintercurrentIndex++; // Move to the next object
        }

        // Reduce the number and update TMP text
        Printercount--;
        if (Printernumber != null)
        {
            Printernumber.text = Printercount.ToString();
        }

        // Disable button when count reaches 0
        if (Printercount <= 0)
        {
            btnPrinter.interactable = false;
        }

        // Check if all devices are revealed
        CheckAllDevicesRevealed();
    }

    private void CheckAllDevicesRevealed()
    {
        bool allRoutersRevealed = (Routercount == 0) || (RoutercurrentIndex >= RouterToShow.Length);
        bool allPCsRevealed = (PCcount == 0) || (PCcurrentIndex >= PCToShow.Length);
        bool allPrintersRevealed = (Printercount == 0) || (PrintercurrentIndex >= PrinterToShow.Length);


        // If all valid devices (PCs and Routers) are revealed, show the topology options panel
        if (allPCsRevealed && allRoutersRevealed && allPrintersRevealed)
        {
            if (pnlDevices != null)
            {
                pnlDevices.SetActive(false);
                pnlConnect.SetActive(true);

            }
        }
    }

    public void ShowConnection()
    {
        StartCoroutine(ShowConnectionCoroutine());
    }

    public IEnumerator ShowConnectionCoroutine()
    {
        pnlLoading.SetActive(true);
        yield return new WaitForSeconds(1f);
        pnlLoading.SetActive(false);
        pnlLines.SetActive(true);
        pnlConnected.SetActive(true);
        yield return new WaitForSeconds(1.5f);
        pnlConnected.SetActive(false);
        pnlConnect.SetActive(false);
        pnlConfigure.SetActive(true);
    }

    public void ConfigureRouter()
    {
        pnlConfigureRouter.SetActive(true);
    }

    public void SaveRouter()
    {
        txtSSID.text = inpSSID.text;
        pnlConfigureRouter.SetActive(false);
        pnlConfigure.SetActive(false);
        pnlAssign.SetActive(true);
    }

    public void AssignIP()
    {
        StartCoroutine(AssignIPCoroutine());
    }

    public IEnumerator AssignIPCoroutine()
    {
        pnlLoading.SetActive(true);
        yield return new WaitForSeconds(1.5f);
        pnlLoading.SetActive(false);
        pnlAssignNotif.SetActive(true);
        pnlIP.SetActive(true);
        yield return new WaitForSeconds(1.5f);
        pnlAssignNotif.SetActive(false);
        pnlAssign.SetActive(false);
        pnlPing.SetActive(true);
    }

    public void PingDevices()
    {
        StartCoroutine(PingDeviceCoroutine());
    }

    public IEnumerator PingDeviceCoroutine()
    {
        pnlLoading.SetActive(true);
        yield return new WaitForSeconds(2.5f);
        pnlLoading.SetActive(false);
        pnlPingNotif.SetActive(true);
        yield return new WaitForSeconds(1.5f);
        pnlPingNotif.SetActive(false);

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

        pnlLoading.SetActive(true);
        yield return new WaitForSeconds(1f);
        pnlLoading.SetActive(false);
        pnlPassed.SetActive(true);
    }

    public void RestartGame()
    {
        Debug.Log("🔄 Restarting game...");

        // Reset selection
        selectedButton = null;

        // Reset counters
        PCcurrentIndex = 0;
        RoutercurrentIndex = 0;
        PrintercurrentIndex = 0;
        PCcount = PCToShow.Length;
        Printercount = PrinterToShow.Length;
        Routercount = RouterToShow.Length;

        // Reset UI text
        if (PCnumber != null) PCnumber.text = PCcount.ToString();
        if (Routernumber != null) Routernumber.text = Routercount.ToString();
        if (Printernumber != null) Printernumber.text = Printercount.ToString();

        // Enable buttons
        if (btnPC != null) btnPC.interactable = true;
        if (btnRouter != null) btnRouter.interactable = true;
        if (btnPrinter != null) btnPrinter.interactable = true;
        if (nextButton != null) nextButton.interactable = false;

        // Hide all panels
        pnlTopology?.SetActive(true);
        pnlDevices?.SetActive(true);
        pnlConnect?.SetActive(false);
        pnlConnected?.SetActive(false);
        pnlConfigure?.SetActive(false);
        pnlConfigureRouter?.SetActive(false);
        txtSSID.text = "";
        inpSSID.text = "";
        inpPw.text = "";
        pnlAssign?.SetActive(false);
        pnlAssignNotif?.SetActive(false);
        pnlIP?.SetActive(false);
        pnlPing?.SetActive(false);
        pnlPingNotif?.SetActive(false);
        pnlLines?.SetActive(false);
        pnlPassed?.SetActive(false);
        pnlFailed?.SetActive(false);
        pnlLoading?.SetActive(false);

        // Reset topology selection panels
        pnlStar?.SetActive(false);
        pnlRing?.SetActive(false);
        pnlHybrid?.SetActive(false);

        // Hide all devices
        foreach (GameObject obj in PCToShow) if (obj != null) obj.SetActive(false);
        foreach (GameObject obj in RouterToShow) if (obj != null) obj.SetActive(false);
        foreach (GameObject obj in PrinterToShow) if (obj != null) obj.SetActive(false);

        Debug.Log("✅ Game Reset Complete!");
    }


}
