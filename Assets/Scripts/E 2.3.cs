using Firebase.Firestore;
using Firebase.Auth;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.EventSystems;
using System;

public class E23 : MonoBehaviour
{
    // Firestore variables
    [Header("Choose Technoloy")]
    public GameObject pnlTechnology;
    public GameObject pnlTechnologyNotif;
    public TMP_Text pnlTechnologyTxtNotif;
    private Button btnSelectedTechnology = null;
    public Button btnNextTechnology = null;
    public Button btnLeased;
    public Button btnPublic;
    public Button btnFiber;
    public Button btnSatellite;

    public GameObject pnlTechnologyIcons;
    public Image imgLeased;
    public Image imgPublic;
    public Image imgFiber;
    public Image imgSatellite;

    [Header("Apply Security")]
    public GameObject pnlSecurity;
    public GameObject pnlSecurityIcons;
    public GameObject VPN;
    public GameObject Firewall;
    public GameObject IDPS;
    public GameObject End;
    public Toggle tglVPN;
    public Toggle tglFirewall;
    public Toggle tglIDPS;
    public Toggle tglEnd;

    [Header("Configure Remote")]
    public GameObject pnlRemote;
    public GameObject pnlRemoteIcons;
    public GameObject MFA;
    public GameObject Cloud;
    public GameObject RBAC;
    public Toggle tglMFA;
    public Toggle tglCloud;
    public Toggle tglRBAC;

    [Header("Test Configuration")]
    public GameObject pnlTest;
    public Button btnTest;
    public GameObject pnlTestResult;
    public TMP_Text txtResult;

    public GameObject pnlPassed; // Second panel to appear after 1.5 sec if correct
    public GameObject pnlFailed; // Panel to appear if the answer is wrong
    public GameObject pnlLoading; // Loading panel (NEW)

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
    }

    public void SelectTechnology(Button button)
    {
        btnSelectedTechnology = button;

        if (btnNextTechnology != null)
        {
            btnNextTechnology.interactable = true;
        }
    }

    public void NextTechnology()
    {
        StartCoroutine(NextTechnologyCoroutine());
    }

    public IEnumerator NextTechnologyCoroutine()
    {
        if (btnSelectedTechnology == null) ; // Prevents errors

        if (btnSelectedTechnology == btnLeased)
        {
            pnlTechnologyTxtNotif.text = "You selected Leased Line (High cost, high reliability).";
            pnlTechnologyIcons.SetActive(true);
            imgLeased.gameObject.SetActive(true);
            imgPublic.gameObject.SetActive(false);
            imgFiber.gameObject.SetActive(false);
            imgSatellite.gameObject.SetActive(false);
        }
        else if (btnSelectedTechnology == btnPublic)
        {
            pnlTechnologyTxtNotif.text = "You selected Public Internet with VPN (Affordable, moderate security, and variable speeds).";
            pnlTechnologyIcons.SetActive(true);
            imgLeased.gameObject.SetActive(false);
            imgPublic.gameObject.SetActive(true);
            imgFiber.gameObject.SetActive(false);
            imgSatellite.gameObject.SetActive(false);
        }
        else if (btnSelectedTechnology == btnFiber)
        {
            pnlTechnologyTxtNotif.text = "You selected Fiber Optic (Fast and moderately expensive, but requires infrastructure).";
            pnlTechnologyIcons.SetActive(true);
            imgLeased.gameObject.SetActive(false);
            imgPublic.gameObject.SetActive(false);
            imgFiber.gameObject.SetActive(true);
            imgSatellite.gameObject.SetActive(false);
        }
        else if (btnSelectedTechnology == btnSatellite)
        {
            pnlTechnologyTxtNotif.text = "You selected Satellite Communication (Global coverage but higher latency and costs).";
            pnlTechnologyIcons.SetActive(true);
            imgLeased.gameObject.SetActive(false);
            imgPublic.gameObject.SetActive(false);
            imgFiber.gameObject.SetActive(false);
            imgSatellite.gameObject.SetActive(true);
        }

        pnlLoading.SetActive(true);
        yield return new WaitForSeconds(1f);
        pnlLoading.SetActive(false);

        pnlTechnologyNotif.SetActive(true);
        yield return new WaitForSeconds(1.5f);
        pnlTechnologyNotif.SetActive(false);
        pnlTechnology.SetActive(false);
        pnlSecurity.SetActive(true);
    }

    public void NextSecurity()
    {
        StartCoroutine(NextSecurityCoroutine());
    }

    public IEnumerator NextSecurityCoroutine()
    {
        pnlLoading.SetActive(true);
        yield return new WaitForSeconds(1.5f);
        pnlLoading.SetActive(false);

        pnlSecurityIcons.SetActive(true);

        if (tglVPN.isOn)
        {
            VPN.SetActive(true);
        }

        if (tglFirewall.isOn)
        {
            Firewall.SetActive(true);
        }

        if (tglIDPS.isOn)
        {
            IDPS.SetActive(true);
        }

        if (tglEnd.isOn)
        {
            End.SetActive(true);
        }

        pnlSecurity.SetActive(false);
        pnlRemote.SetActive(true);
    }

    public void NextRemote()
    {
        StartCoroutine(NextRemoteCoroutine());
    }
    public IEnumerator NextRemoteCoroutine()
    {
        pnlLoading.SetActive(true);
        yield return new WaitForSeconds(1.5f);
        pnlLoading.SetActive(false);

        pnlRemoteIcons.SetActive(true);

        if (tglMFA.isOn)
        {
            MFA.SetActive(true);
        }

        if (tglCloud.isOn)
        {
            Cloud.SetActive(true);
        }

        if (tglRBAC.isOn)
        {
            RBAC.SetActive(true);
        }

        pnlRemote.SetActive(false);
        pnlTest.SetActive(true);
    }

    public void TestConfig()
    {
        StartCoroutine(TestConfigCoroutine());
    }

    private IEnumerator TestConfigCoroutine()
    {
        pnlLoading.SetActive(true);
        yield return new WaitForSeconds(1.5f);
        pnlLoading.SetActive(false);


        bool isGoodConfig = CheckWANConfig();

        pnlTest.SetActive(false);
        if (isGoodConfig)
        {
            txtResult.text = "WAN Configuration Passed!\nYour network is secure, fast, and optimized.";
            pnlTestResult.SetActive(true);

            yield return new WaitForSeconds(2.5f);

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
            yield return new WaitForSeconds(1.5f);
            pnlLoading.SetActive(false);
            pnlPassed.SetActive(true);
        }
        else
        {
            txtResult.text = "WAN Configuration Failed!\nConsider upgrading your technology or adding security measures.";

            pnlTestResult.SetActive(true);
            yield return new WaitForSeconds(2.5f);

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

            pnlLoading.SetActive(true);
            yield return new WaitForSeconds(1.5f);
            pnlLoading.SetActive(false);
            pnlFailed.SetActive(true);
        }
    }

    private bool CheckWANConfig()
    {
        bool hasGoodTechnology = imgLeased.gameObject.activeSelf || imgFiber.gameObject.activeSelf;
        bool hasStrongSecurity = VPN.activeSelf && Firewall.activeSelf && IDPS.activeSelf;
        bool hasSecureRemoteAccess = MFA.activeSelf || RBAC.activeSelf;

        return hasGoodTechnology && hasStrongSecurity && hasSecureRemoteAccess;
    }

    public void RestartGame()
    {
        Debug.Log("🔄 Restarting the game...");

        // Reset selected technology
        btnSelectedTechnology = null;
        if (btnNextTechnology != null) btnNextTechnology.interactable = false;

        // Hide technology icons
        imgLeased.gameObject.SetActive(false);
        imgPublic.gameObject.SetActive(false);
        imgFiber.gameObject.SetActive(false);
        imgSatellite.gameObject.SetActive(false);

        // Reset security selections
        tglVPN.isOn = false;
        tglFirewall.isOn = false;
        tglIDPS.isOn = false;
        tglEnd.isOn = false;
        VPN.SetActive(false);
        Firewall.SetActive(false);
        IDPS.SetActive(false);
        End.SetActive(false);

        // Reset remote access selections
        tglMFA.isOn = false;
        tglCloud.isOn = false;
        tglRBAC.isOn = false;
        MFA.SetActive(false);
        Cloud.SetActive(false);
        RBAC.SetActive(false);

        // Reset UI panels
        pnlTechnology.SetActive(true);
        pnlSecurity.SetActive(false);
        pnlRemote.SetActive(false);
        pnlTest.SetActive(false);
        pnlTestResult.SetActive(false);
        pnlPassed.SetActive(false);
        pnlFailed.SetActive(false);
        pnlLoading.SetActive(false);
        pnlTechnologyIcons.SetActive(false);
        pnlSecurityIcons.SetActive(false);
        pnlRemoteIcons.SetActive(false);

        // Reset test result text
        txtResult.text = "";

        Debug.Log("✅ Game has been reset!");
    }
}
