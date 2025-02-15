using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;

public class PacketCreator : MonoBehaviour
{
    // UI Elements for Step 1
    public InputField messageInput;
    public InputField sourceIPInput;
    public InputField destinationIPInput;
    public InputField sequenceNumberInput;
    public Text feedbackText;
    public GameObject step1UI; // Panel para sa Step 1

    // UI Elements for Step 2
    public Text routeResultText;
    public GameObject routeSelectionUI; // Panel para sa Step 2

    // UI Elements for Step 3
    public Text reassemblyResultText;
    public GameObject reassemblyUI; // Panel para sa Step 3

    // UI Elements for Results Screen
    public GameObject resultsScreenUI;
    public Text successRateText;
    public Text totalTimeText;
    public Text bonusPointsText;
    public Text badgeText;
    public Text scoreText; // Text para ipakita ang score (1-100)

    // UI Elements for Result Panels
    public GameObject step1ResultUI; // Result panel para sa Step 1
    public Text step1ResultText; // Text para sa result ng Step 1
    public GameObject step2ResultUI; // Result panel para sa Step 2
    public Text step2ResultText; // Text para sa result ng Step 2
    public GameObject step3ResultUI; // Result panel para sa Step 3
    public Text step3ResultText; // Text para sa result ng Step 3

    // Variables para sa tracking
    private bool isPacketOutOfOrder = true; // Simulate out-of-order packets
    private bool isPacketMissing = false; // Simulate missing packets
    private int correctDecisions = 0; // Bilang ng tamang desisyon
    private int totalDecisions = 3; // Kabuuang bilang ng desisyon (Step 1, Step 2, Step 3)
    private float startTime; // Oras ng simula ng game
    private int bonusPoints = 0; // Bonus points
    private int score = 0; // Total score (1-100)

    // Start function
    private void Start()
    {
        startTime = Time.time; // I-record ang oras ng simula
        step1UI.SetActive(true); // Ipakita ang Step 1 UI sa simula
        routeSelectionUI.SetActive(false); // Itago ang Step 2 UI
        reassemblyUI.SetActive(false); // Itago ang Step 3 UI
        resultsScreenUI.SetActive(false); // Itago ang Results Screen UI
        step1ResultUI.SetActive(false); // Itago ang Step 1 Result UI
        step2ResultUI.SetActive(false); // Itago ang Step 2 Result UI
        step3ResultUI.SetActive(false); // Itago ang Step 3 Result UI
    }

    // Step 1: Packet Creation
    public void OnCreatePacketButtonClick()
    {
        // Kunin ang mga input mula sa UI
        string message = messageInput.text;
        string sourceIP = sourceIPInput.text;
        string destinationIP = destinationIPInput.text;
        string sequenceNumber = sequenceNumberInput.text;

        // I-validate ang mga input
        if (string.IsNullOrEmpty(sourceIP) || string.IsNullOrEmpty(destinationIP))
        {
            feedbackText.text = "Warning: Source IP and Destination IP must be set!";
            feedbackText.color = Color.red;
            return; // Huwag magpatuloy kung kulang ang IP
        }

        // Validate IP address format
        if (!IsValidIP(sourceIP) || !IsValidIP(destinationIP))
        {
            feedbackText.text = "Invalid IP address format! Minus 5 points.";
            feedbackText.color = Color.red;
            bonusPoints -= 5; // Minus 5 points
            return;
        }

        // Kung may Sequence Number, magbigay ng bonus points
        if (!string.IsNullOrEmpty(sequenceNumber))
        {
            bonusPoints += 10; // 10 points bonus
            correctDecisions++; // Tamang desisyon
        }

        // I-format ang packet
        string packet = $"Message: {message}\nSource IP: {sourceIP}\nDestination IP: {destinationIP}\nSequence Number: {sequenceNumber}";
        Debug.Log("Packet Created: " + packet);

        // Ipakita ang feedback sa Step 1 Result Panel
        step1ResultText.text = $"Packet successfully created! Bonus Points: {bonusPoints}\n\nPacket Summary:\n{packet}";
        step1UI.SetActive(false); // Itago ang Step 1 UI
        step1ResultUI.SetActive(true); // Ipakita ang Step 1 Result UI
    }

    // Function to validate IP address
    private bool IsValidIP(string ip)
    {
        string pattern = @"^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$";
        return Regex.IsMatch(ip, pattern);
    }

    // Proceed to Step 2 after Step 1 Result
    public void OnStep1ResultProceed()
    {
        step1ResultUI.SetActive(false); // Itago ang Step 1 Result UI
        ShowRouteSelectionUI(); // Ipakita ang Step 2 UI
    }

    // Step 2: Route Selection
    private void ShowRouteSelectionUI()
    {
        routeSelectionUI.SetActive(true); // Ipakita ang Step 2 UI
        routeResultText.text = "Choose a path for the packet:";
    }

    // Path A: Fast but Crowded
    public void OnPathASelected()
    {
        routeResultText.text = "Path A: Fast but Crowded - Traffic jam delay!";
        Debug.Log("Path A: Traffic jam delay!");

        // Ipakita ang result sa Step 2 Result Panel
        step2ResultText.text = "Path Selected: Path A (Fast but Crowded)";
        routeSelectionUI.SetActive(false); // Itago ang Step 2 UI
        step2ResultUI.SetActive(true); // Ipakita ang Step 2 Result UI
    }

    // Path B: Moderate Traffic
    public void OnPathBSelected()
    {
        routeResultText.text = "Path B: Moderate Traffic - Best choice!";
        Debug.Log("Path B: Best choice!");
        correctDecisions++; // Tamang desisyon

        // Ipakita ang result sa Step 2 Result Panel
        step2ResultText.text = "Path Selected: Path B (Moderate Traffic)";
        routeSelectionUI.SetActive(false); // Itago ang Step 2 UI
        step2ResultUI.SetActive(true); // Ipakita ang Step 2 Result UI
    }

    // Path C: Long Route but Stable
    public void OnPathCSelected()
    {
        routeResultText.text = "Path C: Long Route but Stable - Packet arrived late but safely.";
        Debug.Log("Path C: Packet arrived late but safely.");

        // Ipakita ang result sa Step 2 Result Panel
        step2ResultText.text = "Path Selected: Path C (Long Route but Stable)";
        routeSelectionUI.SetActive(false); // Itago ang Step 2 UI
        step2ResultUI.SetActive(true); // Ipakita ang Step 2 Result UI
    }

    // Proceed to Step 3 after Step 2 Result
    public void OnStep2ResultProceed()
    {
        step2ResultUI.SetActive(false); // Itago ang Step 2 Result UI
        ProceedToStep3(); // Ipakita ang Step 3 UI
    }

    // Step 3: Packet Reassembly
    private void ProceedToStep3()
    {
        reassemblyUI.SetActive(true); // Ipakita ang Step 3 UI
        reassemblyResultText.text = "The packet has arrived at the receiver. Some packets took different paths. What will you do?";
    }

    // Option 1: Reorder Using Sequence Number
    public void OnReorderSelected()
    {
        if (isPacketOutOfOrder)
        {
            reassemblyResultText.text = "Packets reordered using sequence number. Message delivered correctly!";
            Debug.Log("Packets reordered. Message delivered correctly!");
            correctDecisions++; // Tamang desisyon
        }
        else
        {
            reassemblyResultText.text = "No need to reorder. Message delivered correctly!";
            Debug.Log("No need to reorder. Message delivered correctly!");
        }

        // Ipakita ang result sa Step 3 Result Panel
        step3ResultText.text = reassemblyResultText.text;
        reassemblyUI.SetActive(false); // Itago ang Step 3 UI
        step3ResultUI.SetActive(true); // Ipakita ang Step 3 Result UI
    }

    // Option 2: Request Resend
    public void OnRequestResendSelected()
    {
        if (isPacketMissing)
        {
            reassemblyResultText.text = "Requesting resend of missing packets...";
            Debug.Log("Requesting resend of missing packets...");
        }
        else
        {
            reassemblyResultText.text = "No missing packets. Message delivered correctly!";
            Debug.Log("No missing packets. Message delivered correctly!");
        }

        // Ipakita ang result sa Step 3 Result Panel
        step3ResultText.text = reassemblyResultText.text;
        reassemblyUI.SetActive(false); // Itago ang Step 3 UI
        step3ResultUI.SetActive(true); // Ipakita ang Step 3 Result UI
    }

    // Option 3: Send Data Without Checking
    public void OnSendWithoutCheckingSelected()
    {
        reassemblyResultText.text = "Data sent without checking. Data corruption occurred!";
        Debug.Log("Data sent without checking. Data corruption occurred!");

        // Ipakita ang result sa Step 3 Result Panel
        step3ResultText.text = reassemblyResultText.text;
        reassemblyUI.SetActive(false); // Itago ang Step 3 UI
        step3ResultUI.SetActive(true); // Ipakita ang Step 3 Result UI
    }

    // Proceed to Results Screen after Step 3 Result
    public void OnStep3ResultProceed()
    {
        step3ResultUI.SetActive(false); // Itago ang Step 3 Result UI
        ShowResultsScreen(); // Ipakita ang Results Screen
    }

    // Results Screen
    private void ShowResultsScreen()
    {
        // Ipakita ang Results Screen UI
        resultsScreenUI.SetActive(true);

        // Kalkulahin ang Success Rate
        float successRate = ((float)correctDecisions / totalDecisions) * 100;
        successRateText.text = "Success Rate: " + successRate.ToString("F2") + "%";

        // Kalkulahin ang Total Time Taken
        float totalTime = Time.time - startTime;
        totalTimeText.text = "Total Time Taken: " + totalTime.ToString("F2") + " seconds";

        // Ipakita ang Bonus Points
        bonusPointsText.text = "Bonus Points: " + bonusPoints;

        // Kalkulahin ang Total Score (1-100)
        score = Mathf.Clamp((int)(successRate + bonusPoints), 0, 100); // Clamp score between 0 and 100
        scoreText.text = "Score: " + score + "/100";

        // Magbigay ng Badge
        if (correctDecisions == totalDecisions)
        {
            badgeText.text = "Badge: Network Engineer (Perfect Choices)";
        }
        else if (isPacketMissing && correctDecisions >= 2)
        {
            badgeText.text = "Badge: Troubleshooter (Fixed Errors)";
        }
        else if (totalTime < 30) // Halimbawa, kung mabilis ang route selection
        {
            badgeText.text = "Badge: Fastest Router (Best Route Selection)";
        }
        else
        {
            badgeText.text = "Badge: None";
        }
    }

    // Restart Game
    public void RestartGame()
    {
        // I-reset ang lahat ng variables at UI
        correctDecisions = 0;
        bonusPoints = 0;
        score = 0;
        startTime = Time.time;
        resultsScreenUI.SetActive(false);
        reassemblyUI.SetActive(false);
        routeSelectionUI.SetActive(false);
        step1UI.SetActive(true);
        step1ResultUI.SetActive(false);
        step2ResultUI.SetActive(false);
        step3ResultUI.SetActive(false);
        feedbackText.text = "";
        messageInput.text = "";
        sourceIPInput.text = "";
        destinationIPInput.text = "";
        sequenceNumberInput.text = "";

        // I-reload ang scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}