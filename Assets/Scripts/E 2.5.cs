using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using System.Text.RegularExpressions;
using Firebase.Firestore;
using Firebase.Auth;

public class E25 : MonoBehaviour
{
    [Header("Configure Router")]
    public GameObject pnlConfigure;
    public TMP_Text txtSSID;
    public TMP_InputField inpSSID;
    public TMP_InputField inpPw;
    public Dropdown ddProtocol;
    public Button btnSave;
    public GameObject pnlConfigureNotif;
    public TMP_Text txtConfigureNotif;

    [Header("Connect to Wifi")]
    public GameObject pnlWifi;
    public Button btnConnect;
    public Button btnWifi;
    public TMP_Text txtWifiSSID;

    [Header("Input Password")]
    public GameObject pnlConnectWifi;
    public TMP_InputField inpWifiPw;
    public Button btnConnectWifi;
    public GameObject pnlWifiNotif;
    public TMP_Text txtWifiNotif;
    public TMP_Text txtConnectSSID;
    public GameObject imgWifi;

    public GameObject pnlPassed;
    public GameObject pnlFailed;
    public GameObject pnlLoading;

    // Firestore variables
    private FirebaseFirestore db;
    private FirebaseAuth auth;
    private FirebaseUser currentUser;

    [Header("Firestore Settings")]
    [SerializeField] private string firestoreCollectionName; // Collection name specified in Inspector
    [SerializeField] private string firestoreDocumentName; // Document name specified in Inspector
    [SerializeField] private string AddCollection; // Document name specified in Inspector
    [SerializeField] private string AddDocument; // Document name specified in Inspector

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

        btnSave.onClick.AddListener(SaveConfiguration);
    }

    public void ConfigureRouter()
    {
        pnlConfigure.SetActive(true);
    }

    public void SaveConfiguration()
    {
        StartCoroutine(SaveConfigurationCoroutine());
    }

    private bool IsValidPassword(string password)
    {
        if (password.Length < 8)
            return false;

        if (!Regex.IsMatch(password, @"\d")) // Check for a number
            return false;

        if (!Regex.IsMatch(password, @"[!@#$%^&*(),.?\:{ }|<>\\[\]\/]")) // ✅ Properly escaped special characters
            return false;

        return true;
    }

    public IEnumerator SaveConfigurationCoroutine()
    {
        string password = inpPw.text;

        if (string.IsNullOrEmpty(inpSSID.text) || string.IsNullOrEmpty(inpPw.text))
        {
            pnlLoading.SetActive(true);
            yield return new WaitForSeconds(1f);
            pnlLoading.SetActive(false);
            txtConfigureNotif.text = "All fields are required.";
            txtConfigureNotif.color = Color.red;
            pnlConfigureNotif.SetActive(true);
            yield return new WaitForSeconds(1.5f);
            pnlConfigureNotif.SetActive(false);
        }
        else if (!IsValidPassword(password))
        {
            pnlLoading.SetActive(true);
            yield return new WaitForSeconds(1f);
            pnlLoading.SetActive(false);
            txtConfigureNotif.text = "Password must be at least 8 characters long, contain a number, and a special character.";
            txtConfigureNotif.color = Color.red;
            pnlConfigureNotif.SetActive(true);
            yield return new WaitForSeconds(2.5f);
            pnlConfigureNotif.SetActive(false);
        }
        else
        {
            txtSSID.text = inpSSID.text;
            pnlLoading.SetActive(true);
            yield return new WaitForSeconds(1f);
            pnlLoading.SetActive(false);
            txtConfigureNotif.text = "Configuration Saved!";
            txtConfigureNotif.color = Color.green;
            pnlConfigureNotif.SetActive(true);
            yield return new WaitForSeconds(1.5f);
            pnlConfigure.SetActive(false);
            pnlConfigureNotif.SetActive(false);
            btnConnect.gameObject.SetActive(true);
        }
    }

    public void ShowWifiPanel()
    {
        txtWifiSSID.text = txtSSID.text;
        pnlWifi.SetActive(true);
    }

    public void ClickWifi()
    {
        txtConnectSSID.text = txtWifiSSID.text;
        pnlConnectWifi.SetActive(true);
    }

    public void ConnectWifi()
    {
        StartCoroutine(ConnectWifiCoroutine());
    }

    public IEnumerator ConnectWifiCoroutine()
    {
        if (inpWifiPw.text == "")
        {
            pnlLoading.SetActive(true);
            yield return new WaitForSeconds(1f);
            pnlLoading.SetActive(false);
            txtWifiNotif.text = "Password is required.";
            txtWifiNotif.color = Color.red;
            pnlWifiNotif.SetActive(true);
            yield return new WaitForSeconds(1.5f);
            pnlWifiNotif.SetActive(false);
        }
        if (inpWifiPw.text == inpPw.text)
        {
            string userId = currentUser.UserId; // Get current user's ID

            pnlLoading.SetActive(true);
            yield return new WaitForSeconds(1f);
            pnlLoading.SetActive(false);
            txtWifiNotif.text = "Device successfully connected to " + txtSSID.text + " Secure connection established.";
            txtWifiNotif.color = Color.green;
            pnlWifiNotif.SetActive(true);
            yield return new WaitForSeconds(2f);
            pnlWifi.SetActive(false);
            pnlConnectWifi.SetActive(false);
            pnlWifiNotif.SetActive(false);
            imgWifi.SetActive(true);

            if (currentUser != null)
            {

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

                // Reference to the "profile" collection for the current user
                DocumentReference userProfileRef = db.Collection("profile").Document(userId);

                // Check if the specified collection already exists
                var task = userProfileRef.Collection(AddCollection).GetSnapshotAsync();
                yield return new WaitUntil(() => task.IsCompleted); // Wait for the task to complete


                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.LogError("Failed to check for existing collection: " + task.Exception);
                    yield break;
                }

                QuerySnapshot querySnapshot = task.Result;

                if (querySnapshot.Count == 0) // If the collection is empty
                {
                    // Add an empty document to create the collection
                    var innerTask = userProfileRef.Collection(AddCollection).Document(AddDocument).SetAsync(new { });
                    yield return new WaitUntil(() => innerTask.IsCompleted); // Wait for the inner task to complete

                    if (innerTask.IsFaulted || innerTask.IsCanceled)
                    {
                        Debug.LogError("Failed to add collection: " + innerTask.Exception);
                        yield break;
                    }

                    Debug.Log("Collection with name '" + AddCollection + "' and Document '" + AddDocument + "' has been added.");
                }
                else
                {
                    Debug.Log("Collection '" + AddCollection + "' already exists.");
                }
            }


            pnlLoading.SetActive(true);
            yield return new WaitForSeconds(1f);
            pnlLoading.SetActive(false);
            pnlPassed.SetActive(true);
        }
        else
        {
            pnlLoading.SetActive(true);
            yield return new WaitForSeconds(1f);
            pnlLoading.SetActive(false);
            txtWifiNotif.text = "Incorrect password. Please try again.";
            txtWifiNotif.color = Color.red;
            pnlWifiNotif.SetActive(true);
            yield return new WaitForSeconds(1.5f);
            pnlWifiNotif.SetActive(false);
        }
    }

    public void RestartGame()
    {
        Debug.Log("🔄 Restarting game...");

        // Reset input fields
        inpSSID.text = "";
        inpPw.text = "";
        inpWifiPw.text = "";
        ddProtocol.value = 0; // Reset dropdown selection

        // Reset SSID display
        txtSSID.text = "SSID";
        txtWifiSSID.text = "SSID";
        txtConnectSSID.text = "SSID";

        // Reset notification messages
        txtConfigureNotif.text = "";
        txtWifiNotif.text = "";

        // Hide UI panels
        pnlConfigure.SetActive(false);
        pnlWifi.SetActive(false);
        pnlConnectWifi.SetActive(false);
        pnlConfigureNotif.SetActive(false);
        pnlWifiNotif.SetActive(false);
        pnlPassed.SetActive(false);
        pnlFailed.SetActive(false);
        pnlLoading.SetActive(false);

        // Reset button states
        btnConnect.gameObject.SetActive(false);
        imgWifi.SetActive(false);

        Debug.Log("✅ Game Reset Complete!");
    }


}
