using UnityEngine;
using UnityEngine.UI;
using Firebase.Firestore;
using Firebase.Auth;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class MatchingQuizManager : MonoBehaviour
{
    [Header("Settings")]
    public int numberOfDescriptions = 5; // Number of description buttons to generate

    [Header("UI Elements")]
    public Transform scrollViewContent; // Content panel inside Scroll View (for descriptions)
    public GameObject buttonPrefab; // Prefab for dynamically created description buttons

    [Header("Device Buttons (Assign in Inspector)")]
    public List<Button> deviceButtons = new List<Button>(); // Manually assign device buttons in the Inspector

    [Header("Finish & Score Panels")]
    public Button finishButton;
    public GameObject failedPanel;
    public GameObject passedPanel;
    public GameObject startPanel;
    public TextMeshProUGUI failedScoreTextTMP;
    public TextMeshProUGUI passedScoreTextTMP;
    public GameObject loadingUI;


    [Header("Descriptions")]
    public List<string> descriptions;

    private Dictionary<Button, Button> selectedPairs = new Dictionary<Button, Button>();
    private Dictionary<Button, Color> originalColors = new Dictionary<Button, Color>();
    private Dictionary<Button, string> buttonDescriptions = new Dictionary<Button, string>();
    private Button selectedDevice = null;
    private int score = 0;

    private FirebaseFirestore db;
    private FirebaseAuth auth;
    private FirebaseUser currentUser;

    [Header("Firestore Settings")]
    [SerializeField] private string firestoreCollectionName; // Collection name specified in Inspector
    [SerializeField] private string firestoreDocumentName; // Document name specified in Inspector
    [SerializeField] private string firestoreAddDocument; // Document name specified in Inspector

    public void Start()
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

        StartCoroutine(CheckScoreCoroutine(scoreDocRef));
    }

    private IEnumerator CheckScoreCoroutine(DocumentReference scoreDocRef)
    {
        loadingUI.SetActive(true);
        // Fetch the user's score document
        var task = scoreDocRef.GetSnapshotAsync();

        // Wait for the task to complete
        yield return new WaitUntil(() => task.IsCompleted);

        loadingUI.SetActive(false);

        if (task.IsFaulted || task.IsCanceled)
        {
            Debug.LogError("Failed to check for user score: " + task.Exception);
            yield break;
        }

        DocumentSnapshot snapshot = task.Result;

        if (snapshot.Exists)
        {
            // If the score document exists, retrieve the score
            if (snapshot.TryGetValue("score", out long userScoreLong))
            {
                int userScore = (int)userScoreLong; // Convert Firestore's long to int

                // Check if the user passed or failed
                if (userScore > 5)
                {
                    // User passed
                    passedScoreTextTMP.text = $"{userScore}/{numberOfDescriptions}!";
                    passedPanel.SetActive(true);
                    failedPanel.SetActive(false);
                    startPanel.SetActive(false);
                }
                else if (userScore >= 0)
                {
                    // User failed
                    failedScoreTextTMP.text = $"{userScore}/{numberOfDescriptions}!";
                    passedPanel.SetActive(false);
                    failedPanel.SetActive(true);
                    startPanel.SetActive(false);
                }
                else
                {
                    Debug.LogWarning("Invalid score value.");
                    passedPanel.SetActive(false);
                    failedPanel.SetActive(false);
                }
            }
            else
            {
                Debug.LogWarning("Score document exists but 'score' field is missing.");
                startPanel.SetActive(true);
                passedPanel.SetActive(false);
                failedPanel.SetActive(false);
            }
        }
        else
        {
            Debug.Log("No score document found for this user. Redirecting to quiz start.");

        }
    }

    public void StartQuiz()
    {
        startPanel.SetActive(false);
        failedPanel.SetActive(false);
        passedPanel.SetActive(false);
        AssignDeviceButtons();
        GenerateDescriptionButtons();
        finishButton.onClick.AddListener(CheckResults);
        finishButton.interactable = false;
    }

    void AssignDeviceButtons()
    {
        if (deviceButtons.Count == 0)
        {
            Debug.LogError("No device buttons assigned! Please assign them in the Inspector.");
            return;
        }

        foreach (Button deviceButton in deviceButtons)
        {
            if (deviceButton != null)
            {
                originalColors[deviceButton] = deviceButton.GetComponent<Image>().color;
                deviceButton.onClick.AddListener(() => SelectDevice(deviceButton));
            }
        }
    }

    void GenerateDescriptionButtons()
    {
        if (deviceButtons.Count < numberOfDescriptions || descriptions.Count < numberOfDescriptions)
        {
            Debug.LogError("Not enough device buttons or descriptions provided! Check Inspector.");
            return;
        }

        // Step 1: Create a mapping of correct device-description pairs before shuffling
        Dictionary<string, string> correctPairs = new Dictionary<string, string>();

        for (int i = 0; i < numberOfDescriptions; i++)
        {
            correctPairs[deviceButtons[i].name] = descriptions[i]; // Match index-based pairs
        }

        // Step 2: Shuffle only the descriptions while keeping track of correct pairs
        List<string> shuffledDescriptions = new List<string>(descriptions.GetRange(0, numberOfDescriptions));
        ShuffleList(shuffledDescriptions); // Shuffle while keeping correct pair tracking

        // Step 3: Generate description buttons with shuffled descriptions
        for (int i = 0; i < numberOfDescriptions; i++)
        {
            string correctDeviceName = deviceButtons[i].name; // The correct device name
            string shuffledDesc = shuffledDescriptions[i]; // Get a shuffled description

            // Step 4: Find the correct device name for this shuffled description
            string correctDeviceForDescription = "";
            foreach (var pair in correctPairs)
            {
                if (pair.Value == shuffledDesc)
                {
                    correctDeviceForDescription = pair.Key;
                    break;
                }
            }

            // Step 5: Instantiate button for description
            GameObject btnObj = Instantiate(buttonPrefab, scrollViewContent);
            btnObj.transform.SetParent(scrollViewContent, false);
            btnObj.transform.localScale = Vector3.one;
            btnObj.transform.localPosition = Vector3.zero;

            LayoutRebuilder.ForceRebuildLayoutImmediate(scrollViewContent.GetComponent<RectTransform>());

            Button btn = btnObj.GetComponent<Button>();
            TextMeshProUGUI btnText = btnObj.GetComponentInChildren<TextMeshProUGUI>();

            btnText.text = shuffledDesc;
            btn.name = correctDeviceForDescription; // Assign the correct device name (even after shuffling)
            btn.onClick.AddListener(() => SelectDescription(btn));

            // Store original color from prefab
            Image btnImage = btn.GetComponent<Image>();
            originalColors[btn] = btnImage.color; // Keep prefab's default color

            // Step 6: Store correct pairing for validation later
            buttonDescriptions[btn] = correctDeviceForDescription;
        }

        Canvas.ForceUpdateCanvases();
    }


    void SelectDevice(Button deviceButton)
    {
        // If clicking the same device again, unselect it and reset its color
        if (selectedDevice == deviceButton)
        {
            selectedDevice.GetComponent<Image>().color = originalColors[selectedDevice]; // Restore original color
            selectedDevice = null;
            return;
        }

        // Reset the previously selected device
        if (selectedDevice != null)
        {
            selectedDevice.GetComponent<Image>().color = originalColors[selectedDevice];
        }

        // Reset the pair of the newly selected device if it was already matched
        if (selectedPairs.ContainsKey(deviceButton))
        {
            ResetPair(deviceButton);
        }

        // Select the new device
        selectedDevice = deviceButton;
        selectedDevice.GetComponent<Image>().color = HexToColor("#187373"); // Highlight selected
    }


    // Helper function to reset a device's pair
    void ResetPair(Button deviceButton)
    {
        if (selectedPairs.ContainsKey(deviceButton))
        {
            Button pairedDescription = selectedPairs[deviceButton];

            // Get the color before resetting it
            Color removedColor = deviceButton.GetComponent<Image>().color;

            // Restore original colors
            deviceButton.GetComponent<Image>().color = originalColors[deviceButton];
            pairedDescription.GetComponent<Image>().color = originalColors[pairedDescription];

            // Reset text color of the description button
            TMP_Text descText = pairedDescription.GetComponentInChildren<TMP_Text>();
            if (descText != null)
            {
                descText.color = Color.black; // Restore original text color
            }

            selectedPairs.Remove(deviceButton);

            // Allow the color to be reused
            activeColors.Remove(removedColor);

            CheckAllMatched();
        }
    }


    // Convert HEX to Color
    public Color HexToColor(string hex)
    {
        Color color;
        if (ColorUtility.TryParseHtmlString(hex, out color))
        {
            return color;
        }
        return Color.white; // Default if invalid hex
    }



    void SelectDescription(Button descriptionButton)
    {
        if (selectedDevice == null)
        {
            Debug.Log("Select a device first!");
            return;
        }

        if (selectedPairs.ContainsValue(descriptionButton))
        {
            Button matchedDevice = null;
            foreach (var pair in selectedPairs)
            {
                if (pair.Value == descriptionButton)
                {
                    matchedDevice = pair.Key;
                    break;
                }
            }

            if (matchedDevice != null)
            {
                selectedPairs.Remove(matchedDevice);
                matchedDevice.GetComponent<Image>().color = originalColors[matchedDevice];
                descriptionButton.GetComponent<Image>().color = originalColors[descriptionButton];
                descriptionButton.GetComponentInChildren<TMP_Text>().color = Color.black;
                CheckAllMatched();
            }
            return;
        }


        Color distinctDarkColor = GetUniqueDarkColor(); // Ensures unique dark colors

        selectedPairs[selectedDevice] = descriptionButton;

        // Set colors
        selectedDevice.GetComponent<Image>().color = distinctDarkColor;
        descriptionButton.GetComponent<Image>().color = distinctDarkColor;

        // Ensure white text remains visible
        TMP_Text descText = descriptionButton.GetComponentInChildren<TMP_Text>();
        if (descText != null)
        {
            descText.color = Color.white; // Always use white text
        }

        selectedDevice = null;
        CheckAllMatched();
    }


    // Generate a dark random color for tertiary colors
    // List to track used colors
    // Track actively used colors
    private HashSet<Color> activeColors = new HashSet<Color>();

    Color GetUniqueDarkColor()
    {
        List<Color> primaryColors = new List<Color>
    {
        new Color(0.4f, 0f, 0f, 1f), // Dark Red
        new Color(0f, 0f, 0.4f, 1f), // Dark Blue
        new Color(0.4f, 0.4f, 0f, 1f) // Dark Yellow
    };

        List<Color> secondaryColors = new List<Color>
    {
        new Color(0f, 0.4f, 0f, 1f), // Dark Green
        new Color(0.4f, 0.2f, 0f, 1f), // Dark Orange
        new Color(0.2f, 0f, 0.4f, 1f) // Dark Purple
    };

        // Step 1: Remove already used colors
        primaryColors.RemoveAll(color => activeColors.Contains(color));
        secondaryColors.RemoveAll(color => activeColors.Contains(color));

        Color selectedColor;

        // Step 2: Use a primary color if available
        if (primaryColors.Count > 0)
        {
            selectedColor = primaryColors[Random.Range(0, primaryColors.Count)];
        }
        // Step 3: Use a secondary color if primary is exhausted
        else if (secondaryColors.Count > 0)
        {
            selectedColor = secondaryColors[Random.Range(0, secondaryColors.Count)];
        }
        // Step 4: If no predefined colors left, generate a random dark tertiary color
        else
        {
            selectedColor = GenerateDarkTertiaryColor();
        }

        // Mark the color as in use
        activeColors.Add(selectedColor);
        return selectedColor;
    }

    // Generate a random dark tertiary color when all primary & secondary colors are used
    Color GenerateDarkTertiaryColor()
    {
        Color newColor;
        do
        {
            float hue = Random.Range(0f, 1f);
            float saturation = Random.Range(0.6f, 1f);
            float brightness = Random.Range(0.1f, 0.3f);
            newColor = Color.HSVToRGB(hue, saturation, brightness);
        } while (activeColors.Contains(newColor));

        activeColors.Add(newColor);
        return newColor;
    }

    void CheckAllMatched()
    {
        finishButton.interactable = selectedPairs.Count == numberOfDescriptions;
    }

    void CheckResults()
    {
        score = 0;

        foreach (var pair in selectedPairs)
        {
            Button deviceButton = pair.Key;
            Button descriptionButton = pair.Value;

            // Get the correct device name stored for the description button
            string correctDeviceName;
            if (!buttonDescriptions.TryGetValue(descriptionButton, out correctDeviceName))
            {
                Debug.LogError("Description button not found in dictionary!");
                continue;
            }

            // Compare the device button's name with the stored correct device name
            if (deviceButton.name == buttonDescriptions[descriptionButton])
            {
                score++;
                deviceButton.interactable = false;
                descriptionButton.interactable = false;
            }
        }

        Debug.Log($"Quiz Finished! Final Score: {score}/{numberOfDescriptions}");

        // Display the result panel


        if (score > 6)
        {
            passedScoreTextTMP.text = $"{score}/{numberOfDescriptions}!";

            passedPanel.SetActive(true);
            failedPanel.SetActive(false);

            // Update the score text

            if (currentUser != null)
            {
                string userId = currentUser.UserId; // Get current user's ID

                // First document reference
                DocumentReference quizDocRef = db.Collection("profile").Document(userId).Collection(firestoreCollectionName).Document(firestoreDocumentName);

                quizDocRef.SetAsync(new
                {
                    score = score,
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
        }
        else
        {

            failedScoreTextTMP.text = $"{score}/{numberOfDescriptions}!";

            passedPanel.SetActive(false);
            failedPanel.SetActive(true);


            if (currentUser != null)
            {
                string userId = currentUser.UserId; // Get current user's ID

                // First document reference
                DocumentReference quizDocRef = db.Collection("profile").Document(userId).Collection(firestoreCollectionName).Document(firestoreDocumentName);

                quizDocRef.SetAsync(new
                {
                    score = score,
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
        }
        finishButton.interactable = false;

    }



    void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int rand = Random.Range(0, i + 1);
            (list[i], list[rand]) = (list[rand], list[i]);
        }
    }

    


    public void ResetGame()
    {
        // Clear selected pairs
        selectedPairs.Clear();
        selectedDevice = null;

        // Reset used colors so they are regenerated in the next round
        activeColors.Clear();

        // Reset device buttons
        foreach (Button deviceButton in deviceButtons)
        {
            
                deviceButton.GetComponent<Image>().color = HexToColor("#334648");
                deviceButton.interactable = true;
           
        }

        // Reset description buttons
        foreach (Transform child in scrollViewContent)
        {
            Destroy(child.gameObject); // Remove old description buttons
        }

        // Hide results
        startPanel.SetActive(true);
        failedPanel.SetActive(false);
        passedPanel.SetActive(false);

        // Reset finish button state
        finishButton.interactable = false;

    }


}
