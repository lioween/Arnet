using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Firebase.Firestore;
using Firebase.Auth;

public class ImageQuizManager : MonoBehaviour
{
    [System.Serializable]
    public class ImageQuizItem
    {
        public string customImageName; // The name you specify in the Inspector
        public string imageFileName; // The actual file name in Resources/Images/ (without extension)
        public string option1Text; // First answer option
        public string option2Text; // Second answer option
        public int correctAnswerIndex; // 0 for option1, 1 for option2
    }

    public List<ImageQuizItem> quizItems; // List of image quiz items
    private int currentQuestionIndex = 0;
    private int score = 0;
    private int selectedAnswerIndex = -1;

    public Image questionImage; // UI Image to display the question image
    public TMP_Text txtName; // Displays the custom image name
    public TMP_Text option1Text; // Text for first answer option
    public TMP_Text option2Text; // Text for second answer option
    public Button option1Button; // Button for first option
    public Button option2Button; // Button for second option
    public TMP_Text timerText; // Displays remaining time
    public Button nextButton; // Next Question button
    public GameObject pnlStart; // Start panel
    public GameObject pnlPassed; // Passed panel
    public GameObject pnlFailed; // Failed panel
    public TMP_Text scorePassed;
    public TMP_Text scoreFailed;
    public GameObject loadingUI;

    public Color correctColor;
    public Color wrongColor;
    public Color selectedColor;
    private Color[] originalButtonColors;

    private float timeRemaining = 30f;
    private bool isTimerRunning = false;

    // Firestore variables
    private FirebaseFirestore db;
    private FirebaseAuth auth;
    private FirebaseUser currentUser;

    [Header("Firestore Settings")]
    [SerializeField] private string firestoreCollectionName;
    [SerializeField] private string firestoreDocumentName;
    [SerializeField] private string firestoreAddDocument;

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

        originalButtonColors = new Color[2];
        originalButtonColors[0] = option1Button.GetComponent<Image>().color;
        originalButtonColors[1] = option2Button.GetComponent<Image>().color;

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
                if (userScore >= 7)
                {
                    // User passed
                    scorePassed.text = $"{userScore}/{quizItems.Count}!";
                    pnlPassed.SetActive(true);
                    pnlFailed.SetActive(false);
                    pnlStart.SetActive(false);
                }
                else if (userScore >= 0)
                {
                    // User failed
                    scoreFailed.text = $"{userScore}/{quizItems.Count}!";
                    pnlPassed.SetActive(false);
                    pnlFailed.SetActive(true);
                    pnlStart.SetActive(false);
                }
                else
                {
                    Debug.LogWarning("Invalid score value.");
                    pnlPassed.SetActive(false);
                    pnlFailed.SetActive(false);
                }
            }
            else
            {
                Debug.LogWarning("Score document exists but 'score' field is missing.");
                pnlStart.SetActive(true);
                pnlPassed.SetActive(false);
                pnlFailed.SetActive(false);
            }
        }
        else
        {
            Debug.Log("No score document found for this user. Redirecting to quiz start.");

        }
    }

    public void StartQuiz()
    {
        pnlStart.SetActive(false);
        ShuffleQuestions();
        LoadNextQuestion();
    }

    void ShuffleQuestions()
    {
        for (int i = 0; i < quizItems.Count; i++)
        {
            ImageQuizItem temp = quizItems[i];
            int randomIndex = Random.Range(i, quizItems.Count);
            quizItems[i] = quizItems[randomIndex];
            quizItems[randomIndex] = temp;
        }
    }

    void LoadNextQuestion()
    {
        if (currentQuestionIndex >= quizItems.Count)
        {
            EndQuiz();
            return;
        }

        timeRemaining = 30f;
        isTimerRunning = true;
        nextButton.interactable = true;

        // Reset button colors to original (from Canvas)
        option1Button.GetComponent<Image>().color = originalButtonColors[0];
        option2Button.GetComponent<Image>().color = originalButtonColors[1];
        option1Button.interactable = true;
        option2Button.interactable = true;

        selectedAnswerIndex = -1;

        // Load the current question
        ImageQuizItem currentQuestion = quizItems[currentQuestionIndex];
        txtName.text = currentQuestion.customImageName;

        // Load Image from Resources
        Sprite loadedImage = Resources.Load<Sprite>($"1.2/{currentQuestion.imageFileName}");
        if (loadedImage != null)
        {
            questionImage.sprite = loadedImage;
        }
        else
        {
            Debug.LogError($"Image '{currentQuestion.imageFileName}' not found in Resources/Images/");
        }

        // Set answer options
        option1Text.text = currentQuestion.option1Text;
        option2Text.text = currentQuestion.option2Text;

        // Reset button listeners
        option1Button.onClick.RemoveAllListeners();
        option2Button.onClick.RemoveAllListeners();
        option1Button.onClick.AddListener(() => OnOptionSelected(0));
        option2Button.onClick.AddListener(() => OnOptionSelected(1));

        // Change Next button text if it's the last question
        if (currentQuestionIndex == quizItems.Count - 1)
        {
            nextButton.GetComponentInChildren<TMP_Text>().text = "Finish";
        }
        else
        {
            nextButton.GetComponentInChildren<TMP_Text>().text = "Next";
        }
    }


    void EndQuiz()
    {
        Debug.Log($"Quiz Finished! Final Score: {score}/{quizItems.Count}");

        // Display the result panel


        if (score > 6)
        {
            scorePassed.text = $"{score}/{quizItems.Count}!";

            pnlPassed.SetActive(true);
            pnlFailed.SetActive(false);

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

            scoreFailed.text = $"{score}/{quizItems.Count}!";

            pnlPassed.SetActive(false);
            pnlFailed.SetActive(true);


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
    }

    void OnOptionSelected(int selectedIndex)
    {
        selectedAnswerIndex = selectedIndex;

        // Reset all buttons to their original color first
        option1Button.GetComponent<Image>().color = originalButtonColors[0];
        option2Button.GetComponent<Image>().color = originalButtonColors[1];

        // Change only the selected button to selectedColor
        if (selectedIndex == 0)
            option1Button.GetComponent<Image>().color = selectedColor;
        else
            option2Button.GetComponent<Image>().color = selectedColor;
    }


    public void EvaluateAnswer()
    {
        if (selectedAnswerIndex == -1)
        {
            Debug.Log("No answer selected!");
            return;
        }

        nextButton.interactable = false; // Disable next button

        bool isCorrect = selectedAnswerIndex == quizItems[currentQuestionIndex].correctAnswerIndex;

        if (isCorrect)
        {
            option1Button.GetComponent<Image>().color = selectedAnswerIndex == 0 ? correctColor : originalButtonColors[0];
            option2Button.GetComponent<Image>().color = selectedAnswerIndex == 1 ? correctColor : originalButtonColors[1];
            score++;

            Debug.Log($"Correct! Current Score: {score}");
        }
        else
        {
            option1Button.GetComponent<Image>().color = selectedAnswerIndex == 0 ? wrongColor : originalButtonColors[0];
            option2Button.GetComponent<Image>().color = selectedAnswerIndex == 1 ? wrongColor : originalButtonColors[1];

            // Highlight the correct button
            if (quizItems[currentQuestionIndex].correctAnswerIndex == 0)
            {
                option1Button.GetComponent<Image>().color = correctColor;
            }
            else
            {
                option2Button.GetComponent<Image>().color = correctColor;
            }

            Debug.Log($"Wrong! Current Score: {score}");
        }

        // Disable all buttons to prevent multiple clicks
        option1Button.interactable = false;
        option2Button.interactable = false;

        currentQuestionIndex++;
        Invoke(nameof(LoadNextQuestion), 1.5f); // Delay before loading the next question
    }

    void Update()
    {
        if (isTimerRunning && timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
            timerText.text = Mathf.CeilToInt(timeRemaining).ToString();
        }
        else if (isTimerRunning && timeRemaining <= 0)
        {
            // Time's up logic
            isTimerRunning = false;
            timerText.text = "Time's up!"; // Show "Time's up!" on the timer

            HandleTimesUp(); // Record question as wrong, show correct answer, and update UI
        }
    }

    void HandleTimesUp()
    {
        // Highlight the correct button
        int correctIndex = quizItems[currentQuestionIndex].correctAnswerIndex;
        if (correctIndex == 0)
        {
            option1Button.GetComponent<Image>().color = correctColor;
        }
        else
        {
            option2Button.GetComponent<Image>().color = correctColor;
        }

        // Disable all buttons to prevent interaction
        option1Button.interactable = false;
        option2Button.interactable = false;

        nextButton.interactable = false;

        // No score increment because it's marked wrong
        Debug.Log($"Time's up! Current Score: {score}");

        // Move to the next question
        currentQuestionIndex++;
        Invoke(nameof(LoadNextQuestion), 2f); // Proceed to the next question after 2 seconds
    }


    public void TryAgain()
    {
        score = 0;
        currentQuestionIndex = 0;
        selectedAnswerIndex = -1;
        timeRemaining = 30f;
        isTimerRunning = false;

        // Hide result panels and show start panel
        pnlPassed.SetActive(false);
        pnlFailed.SetActive(false);
        pnlStart.SetActive(true);

        // Reset button colors and enable interaction
        option1Button.GetComponent<Image>().color = originalButtonColors[0];
        option2Button.GetComponent<Image>().color = originalButtonColors[1];
        option1Button.interactable = true;
        option2Button.interactable = true;

        // Reset button text
        nextButton.GetComponentInChildren<TMP_Text>().text = "Next";

        // Shuffle questions and restart quiz
        ShuffleQuestions();
        LoadNextQuestion();
    }

}
