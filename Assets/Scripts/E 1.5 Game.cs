using System.Collections;
using System.Collections.Generic;
using Firebase.Firestore;
using Firebase.Auth;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text.RegularExpressions;

public class E15Game : MonoBehaviour
{
    // Start is called before the first frame update

    public GameObject PC; // Assign objects in the Inspector
    public Button btnPC; // Assign the button
    public GameObject Smartphone; // Assign objects in the Inspector
    public Button btnSmartphone; // Assign the button

    [Header("Add Devices")]
    public GameObject pnlAddDevice; // Assign objects in the Inspector

    [Header("Connect Devices")]
    public GameObject pnlConnect;
    public Button btnPCConnect;
    public GameObject pnlPCConnected;
    public GameObject PCWifi;
    public Button btnPhoneConnect;
    public GameObject pnlPhoneConnected;
    public GameObject PhoneWifi;

    [Header("Test Connectivity")]
    public GameObject pnlPing;
    public Button btnPCPing;
    public GameObject pnlPCPing;
    public Button btnPhonePing;
    public GameObject pnlPhonePing;

    [Header("Secure Network")]
    public GameObject pnlSecure;
    public Button btnSecure;
    public GameObject pnlPassword;
    public TMP_InputField inpPw;
    public Button btnConfirmPw;
    public GameObject pnlPwNotif;
    public TMP_Text txtNotif;

    [Header("Exercise Panels")]
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

    void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
        auth = FirebaseAuth.DefaultInstance;
        currentUser = auth.CurrentUser;

        PC.SetActive(false);
        Smartphone.SetActive(false);

        if (currentUser == null)
        {
            Debug.LogError("No user is logged in!");
            return;
        }
    }

    public void RevealPC()
    {
        PC.SetActive(true); // Show the next object
        btnPC.interactable = false;

        if (btnSmartphone.interactable == false)
        {
            pnlAddDevice.SetActive(false);
            pnlConnect.SetActive(true);
        }

    }

    public void RevealSmartphone()
    {
        Smartphone.SetActive(true); // Show the next object
        btnSmartphone.interactable = false;

        if (btnPC.interactable == false)
        {
            pnlAddDevice.SetActive(false);
            pnlConnect.SetActive(true);
        }
    }

    public void ShowPCConnected()
    {
        StartCoroutine(PCConnected());
    }

    public IEnumerator PCConnected()
    {
        pnlLoading.SetActive(true);

        yield return new WaitForSeconds(1f);

        pnlLoading.SetActive(false);

        pnlPCConnected.SetActive(true);

        yield return new WaitForSeconds(2f);

        pnlPCConnected.SetActive(false);
        PCWifi.SetActive(true);

        if (PhoneWifi.activeSelf)
        {
            pnlConnect.SetActive(false);
            pnlPing.SetActive(true);
            PCWifi.SetActive(false);
            PhoneWifi.SetActive(false);
        }
    }

    public void ShowPhoneConnected()
    {
        StartCoroutine(PhoneConnected());
    }

    public IEnumerator PhoneConnected()
    {
        pnlLoading.SetActive(true);

        yield return new WaitForSeconds(1f);

        pnlLoading.SetActive(false);

        pnlPhoneConnected.SetActive(true);

        yield return new WaitForSeconds(2f);

        pnlPhoneConnected.SetActive(false);
        PhoneWifi.SetActive(true);

        if (PCWifi.activeSelf)
        {
            pnlConnect.SetActive(false);
            pnlPing.SetActive(true);
            PCWifi.SetActive(false);
            PhoneWifi.SetActive(false);
        }
    }

    public void ShowPCPing()
    {
        StartCoroutine(PCPing());
    }

    public IEnumerator PCPing()
    {
        pnlLoading.SetActive(true);

        yield return new WaitForSeconds(1f);

        pnlLoading.SetActive(false);

        pnlPCPing.SetActive(true);

        yield return new WaitForSeconds(1.5f);

        pnlPCPing.SetActive(false);
        PCWifi.SetActive(true);

        if (PhoneWifi.activeSelf)
        {
            pnlPing.SetActive(false);
            pnlSecure.SetActive(true);
        }
    }

    public void ShowPhonePing()
    {
        StartCoroutine(PhonePing());
    }

    public IEnumerator PhonePing()
    {
        pnlLoading.SetActive(true);

        yield return new WaitForSeconds(1f);

        pnlLoading.SetActive(false);

        pnlPhonePing.SetActive(true);

        yield return new WaitForSeconds(1.5f);

        pnlPhonePing.SetActive(false);
        PhoneWifi.SetActive(true);

        if (PCWifi.activeSelf)
        {
            pnlPing.SetActive(false);
            pnlSecure.SetActive(true);
        }
    }

    public void ShowPw()
    {
        pnlPassword.SetActive(true);
    }

    public void ConfirmPw()
    {
        StartCoroutine(ValidateInput());
    }

    public IEnumerator ValidateInput()
    {
        string input = inpPw.text;

        // ✅ Check if input meets all validation rules
        if (IsValidInput(input))
        {
            txtNotif.text = "Password changed successfully. Network secured.";
            txtNotif.color = Color.green;
            pnlPwNotif.SetActive(true);
            yield return new WaitForSeconds(2f);
            pnlPwNotif.SetActive(false);
            pnlPassword.SetActive(false);


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

            }

            pnlLoading.SetActive(true);
            yield return new WaitForSeconds(1f);
            pnlLoading.SetActive(false);
            pnlPassed.SetActive(true);

        }
        else
        {
            txtNotif.text = "Error: Password must be at least 8 characters long and include numbers and special characters.";
            txtNotif.color = Color.red;
            pnlPwNotif.SetActive(true);
            yield return new WaitForSeconds(2f);
            pnlPwNotif.SetActive(false);
        }
    }

    private bool IsValidInput(string input)
    {
        // ✅ Ensure it's at least 8 characters long
        if (input.Length < 8) return false;

        // ✅ Check for at least one digit
        if (!Regex.IsMatch(input, @"\d")) return false;

        // ✅ Check for at least one special character
        if (!Regex.IsMatch(input, @"[!@#$%^&*()_+{}\[\]:;'<>,.?/~`]")) return false;

        return true; // ✅ Input is valid
    }

}



