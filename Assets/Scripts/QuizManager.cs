using Firebase.Firestore;
using Firebase.Auth;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class QuizManager : MonoBehaviour
{
    public Question[] allQuestions; // Add your 30 questions via the Inspector
    private List<Question> currentQuestions; // List of shuffled 10 questions
    private int currentQuestionIndex = 0;
    private int score = 0;
    private string status;
    private int selectedAnswerIndex = -1; // To track the selected option
    public Button nextQuestionButton;
    public TMP_Text nextButtonText;

    public GameObject questionPanel;
    public TMP_Text questionText;
    public Button[] optionButtons;
    public Image[] progressCircles;
    public TMP_Text timerText;
    public Color correctColor;
    public Color wrongColor;
    public Color defaultColor;
    public Color selectedColor;
    public GameObject pnlPassed;
    public GameObject pnlFailed;
    public TMP_Text scorePassed;
    public TMP_Text scoreFailed;
    public TMP_Text txtTitle;
    public GameObject pnlStart;
    public GameObject loadingUI;

    private float timeRemaining = 30f;
    private bool isTimerRunning = false;

    // Firestore variables
    private FirebaseFirestore db;
    private FirebaseAuth auth;
    private FirebaseUser currentUser;

    [Header("Firestore Settings")]
    [SerializeField] private string firestoreCollectionName; // Collection name specified in Inspector
    [SerializeField] private string firestoreDocumentName; // Document name specified in Inspector
    [SerializeField] private string firestoreAddCollection; // Document name specified in Inspector
    [SerializeField] private string firestoreAddDocument; // Document name specified in Inspector


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

                // Fallback to allQuestions.Length if currentQuestions is not initialized
                int totalQuestions = currentQuestions != null && currentQuestions.Count > 0
                    ? currentQuestions.Count
                    : allQuestions.Length;

                // Check if the user passed or failed
                if (userScore >= 7)
                {
                    // User passed
                    scorePassed.text = $"{userScore}/{totalQuestions}!";
                    pnlPassed.SetActive(true);
                    pnlFailed.SetActive(false);
                    pnlStart.SetActive(false);
                }
                else if (userScore >= 0)
                {
                    // User failed
                    scoreFailed.text = $"{userScore}/{totalQuestions}!";
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

    void EndQuiz()
    {
        Debug.Log($"Quiz Finished! Final Score: {score}/{currentQuestions.Count}");

        // Display the result panel
       

        if (score > 6)
        {
            scorePassed.text = $"{score}/{currentQuestions.Count}!";

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

            scoreFailed.text = $"{score}/{currentQuestions.Count}!";

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


    void ShuffleQuestions()
    {
        currentQuestions = new List<Question>(allQuestions);
        for (int i = 0; i < currentQuestions.Count; i++)
        {
            Question temp = currentQuestions[i];
            int randomIndex = Random.Range(i, currentQuestions.Count);
            currentQuestions[i] = currentQuestions[randomIndex];
            currentQuestions[randomIndex] = temp;
        }

        currentQuestions = currentQuestions.GetRange(0, 10);
    }

    void LoadNextQuestion()
    {
        if (currentQuestionIndex >= currentQuestions.Count)
        {
            EndQuiz();
            return;
        }

        timeRemaining = 30f;
        isTimerRunning = true;

        nextQuestionButton.interactable = true;

        foreach (Button button in optionButtons)
        {
            button.GetComponent<Image>().color = defaultColor;
            button.interactable = true;
        }

        selectedAnswerIndex = -1;

        Question currentQuestion = currentQuestions[currentQuestionIndex];
        questionText.text = currentQuestion.questionText;

        for (int i = 0; i < optionButtons.Length; i++)
        {
            if (i < currentQuestion.options.Length)
            {
                optionButtons[i].gameObject.SetActive(true);
                optionButtons[i].GetComponentInChildren<TMP_Text>().text = currentQuestion.options[i];
                int index = i;
                optionButtons[i].onClick.RemoveAllListeners();
                optionButtons[i].onClick.AddListener(() => OnOptionSelected(index));
            }
            else
            {
                optionButtons[i].gameObject.SetActive(false);
            }
        }

        if (currentQuestionIndex == currentQuestions.Count - 1)
        {
            nextButtonText.text = "Finish";
        }
        else
        {
            nextButtonText.text = "Next Question";
        }
    }

    void OnOptionSelected(int selectedIndex)
    {
        selectedAnswerIndex = selectedIndex;

        foreach (Button button in optionButtons)
        {
            button.GetComponent<Image>().color = defaultColor;
        }

        optionButtons[selectedIndex].GetComponent<Image>().color = selectedColor;
    }

    public void EvaluateAnswer()
    {
        if (selectedAnswerIndex == -1)
        {
            Debug.Log("No answer selected!");
            return;
        }

        nextQuestionButton.interactable = false;

        bool isCorrect = selectedAnswerIndex == currentQuestions[currentQuestionIndex].correctAnswerIndex;

        // Update the score and feedback
        if (isCorrect)
        {
            optionButtons[selectedAnswerIndex].GetComponent<Image>().color = correctColor;
            score++; // Increment score
            Debug.Log($"Correct! Current Score: {score}");
        }
        else
        {
            optionButtons[selectedAnswerIndex].GetComponent<Image>().color = wrongColor;

            // Highlight the correct button
            optionButtons[currentQuestions[currentQuestionIndex].correctAnswerIndex]
                .GetComponent<Image>().color = correctColor;
            Debug.Log($"Wrong! Current Score: {score}");
        }

        // Disable all buttons for this question
        foreach (Button button in optionButtons)
        {
            button.interactable = false;
        }

        // Update progress circle
        GameObject currentCircle = progressCircles[currentQuestionIndex].gameObject;

        // Show the correct or wrong icon
        Transform checkmark = currentCircle.transform.Find("imgCorrect");
        Transform xMark = currentCircle.transform.Find("imgWrong");

        if (isCorrect)
        {
            if (checkmark != null) checkmark.gameObject.SetActive(true);
        }
        else
        {
            if (xMark != null) xMark.gameObject.SetActive(true);
        }

        // Prepare for the next question
        currentQuestionIndex++;
        Invoke(nameof(LoadNextQuestion), 1f); // Delay before loading the next question
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
        int correctIndex = currentQuestions[currentQuestionIndex].correctAnswerIndex;
        optionButtons[correctIndex].GetComponent<Image>().color = correctColor;

        // Disable all buttons to prevent interaction
        foreach (Button button in optionButtons)
        {
            button.interactable = false;
        }

        nextQuestionButton.interactable = false;

        // Update progress circle with X mark
        GameObject currentCircle = progressCircles[currentQuestionIndex].gameObject;
        Transform xMark = currentCircle.transform.Find("imgWrong");
        if (xMark != null) xMark.gameObject.SetActive(true); // Show the X mark

        // No score increment because it's marked wrong
        Debug.Log($"Time's up! Current Score: {score}");

        // Move to the next question
        currentQuestionIndex++;
        Invoke(nameof(LoadNextQuestion), 2f); // Proceed to the next question after 2 seconds
    }




    void HighlightCorrectAnswer()
    {
        // Find the correct answer index
        foreach (Button button in optionButtons)
        {
            button.GetComponent<Image>().color = defaultColor;
        }

        int correctIndex = currentQuestions[currentQuestionIndex].correctAnswerIndex;

        // Highlight the correct button
        optionButtons[correctIndex].GetComponent<Image>().color = correctColor;

        // Disable all buttons to prevent interaction
        foreach (Button button in optionButtons)
        {
            button.interactable = false;
        }
    }

    public void TryAgain()
    {
        // Ensure panels are assigned before attempting to toggle
        if (pnlFailed != null) pnlFailed.SetActive(false);
        if (pnlStart != null) pnlStart.SetActive(true);

            // Reset score
            score = 0;

            // Reset question index
            currentQuestionIndex = 0;

            // Reset selected answer index
            selectedAnswerIndex = -1;

            // Reset timer
            timeRemaining = 30f;
            isTimerRunning = false;

            // Clear progress circles
            foreach (Image progressCircle in progressCircles)
            {
                Transform checkmark = progressCircle.transform.Find("imgCorrect");
                Transform xMark = progressCircle.transform.Find("imgWrong");

                if (checkmark != null) checkmark.gameObject.SetActive(false);
                if (xMark != null) xMark.gameObject.SetActive(false);
            }

        // Reset UI for options
        foreach (Button button in optionButtons)
        {
            // Reset button color
            button.GetComponent<Image>().color = defaultColor;

            // Enable the button
            button.interactable = true;

            // Clear the text in the button
            TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>();
            if (buttonText != null)
            {
                buttonText.text = "";
            }
        }


        // Reset the current questions list (reshuffle)

        // Reset panels


        // Reset timer text
        timerText.text = Mathf.CeilToInt(timeRemaining).ToString();

            // Reset next question button text
            nextButtonText.text = "Next Question";

            Debug.Log("Quiz has been reset.");
        

    }





    [System.Serializable]
    public class Question
    {
        public string questionText;
        public string[] options;
        public int correctAnswerIndex;
    }
}
