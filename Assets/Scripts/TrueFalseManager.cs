using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Firebase.Firestore;
using Firebase.Auth;

public class TrueFalseManager : MonoBehaviour
{
    public TrueFalseQuestion[] allQuestions; // List of questions
    private List<TrueFalseQuestion> currentQuestions; // Selected questions
    private int currentQuestionIndex = 0;
    private int score = 0;
    private int selectedAnswerIndex = -1;

    public Button nextQuestionButton;
    public TMP_Text nextButtonText;

    public GameObject questionPanel;
    public TMP_Text questionText;
    public TMP_Text txtnumber;  // Displays the current question number
    public TMP_Text txtFalse;   // Displays correct statement if answer is false

    public Button[] optionButtons; // 2 Buttons: True & False
    private Color[] originalButtonColors; // Stores the original colors from Canvas

    public TMP_Text timerText;
    public Color correctColor;
    public Color wrongColor;
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
    [SerializeField] private string firestoreCollectionName;
    [SerializeField] private string firestoreDocumentName;
    [SerializeField] private string AddCollection; // Document name specified in Inspector
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

        // Store the original button colors from Canvas
        originalButtonColors = new Color[optionButtons.Length];
        for (int i = 0; i < optionButtons.Length; i++)
        {
            originalButtonColors[i] = optionButtons[i].GetComponent<Image>().color;
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

    void ShuffleQuestions()
    {
        currentQuestions = new List<TrueFalseQuestion>(allQuestions);
        for (int i = 0; i < currentQuestions.Count; i++)
        {
            TrueFalseQuestion temp = currentQuestions[i];
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
            StartCoroutine(EndQuiz());
            return;
        }

        timeRemaining = 30f;
        isTimerRunning = true;
        nextQuestionButton.interactable = true;
        txtFalse.text = ""; // Hide correct statement until evaluation

        // Reset button colors to original (from Canvas)
        for (int i = 0; i < optionButtons.Length; i++)
        {
            optionButtons[i].GetComponent<Image>().color = originalButtonColors[i];
            optionButtons[i].interactable = true;
        }

        selectedAnswerIndex = -1;

        TrueFalseQuestion currentQuestion = currentQuestions[currentQuestionIndex];
        questionText.text = currentQuestion.questionText;
        txtnumber.text = $"{currentQuestionIndex + 1}";

        optionButtons[0].GetComponentInChildren<TMP_Text>().text = "TRUE";
        optionButtons[1].GetComponentInChildren<TMP_Text>().text = "FALSE";

        optionButtons[0].onClick.RemoveAllListeners();
        optionButtons[1].onClick.RemoveAllListeners();
        optionButtons[0].onClick.AddListener(() => OnOptionSelected(0));
        optionButtons[1].onClick.AddListener(() => OnOptionSelected(1));

        if (currentQuestionIndex == currentQuestions.Count - 1)
        {
            nextButtonText.text = "Finish";
        }
        else
        {
            nextButtonText.text = "Next";
        }
    }

    void OnOptionSelected(int selectedIndex)
    {
        selectedAnswerIndex = selectedIndex;

        // Reset all buttons to their original color first
        for (int i = 0; i < optionButtons.Length; i++)
        {
            optionButtons[i].GetComponent<Image>().color = originalButtonColors[i];
        }

        // Change only the selected button to selectedColor
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

        if (isCorrect)
        {
            optionButtons[selectedAnswerIndex].GetComponent<Image>().color = correctColor;
            score++;

            if (currentQuestions[currentQuestionIndex].correctAnswerIndex == 1) // False is the correct answer
            {
                txtFalse.text = $"{currentQuestions[currentQuestionIndex].correctStatement}";
            }

            Debug.Log($"Correct! Current Score: {score}");
        }
        else
        {
            optionButtons[selectedAnswerIndex].GetComponent<Image>().color = wrongColor;

            // Highlight the correct button
            optionButtons[currentQuestions[currentQuestionIndex].correctAnswerIndex]
                .GetComponent<Image>().color = correctColor;

            // Display correct statement if answer is false
            if (currentQuestions[currentQuestionIndex].correctAnswerIndex == 1) // False is the correct answer
            {
                txtFalse.text = $"{currentQuestions[currentQuestionIndex].correctStatement}";
            }

            Debug.Log($"Wrong! Current Score: {score}");
        }

        foreach (Button button in optionButtons)
        {
            button.interactable = false;
        }

        currentQuestionIndex++;
        Invoke(nameof(LoadNextQuestion), 1.5f);
    }


    public void TryAgain()
    {
        // Reset quiz data
        score = 0;
        currentQuestionIndex = 0;
        selectedAnswerIndex = -1;
        timeRemaining = 30f;
        isTimerRunning = false;

        // Hide results panels & show start panel
        pnlFailed.SetActive(false);
        pnlPassed.SetActive(false);
        pnlStart.SetActive(true);

        // Reset text values
        txtFalse.text = "";
        txtnumber.text = "";

        // Reset button colors to their original state
        for (int i = 0; i < optionButtons.Length; i++)
        {
            optionButtons[i].GetComponent<Image>().color = originalButtonColors[i];
            optionButtons[i].interactable = true;
        }

        // Shuffle questions and restart quiz
        ShuffleQuestions();
        LoadNextQuestion();
    }

    IEnumerator EndQuiz()
    {
        Debug.Log($"Quiz Finished! Final Score: {score}/{currentQuestions.Count}");

        if (score > 6)
        {
            scorePassed.text = $"{score}/{currentQuestions.Count}!";
            pnlPassed.SetActive(true);
            pnlFailed.SetActive(false);

            if (currentUser != null)
            {
                string userId = currentUser.UserId;

                // First document reference
                DocumentReference quizDocRef = db.Collection("profile").Document(userId).Collection(firestoreCollectionName).Document(firestoreDocumentName);

                var quizTask = quizDocRef.SetAsync(new
                {
                    score = score,
                    status = "passed",
                    timestamp = FieldValue.ServerTimestamp
                });

                yield return new WaitUntil(() => quizTask.IsCompleted);

                if (quizTask.IsFaulted || quizTask.IsCanceled)
                {
                    Debug.LogError("Failed to save quiz results: " + quizTask.Exception);
                    yield break;
                }
                Debug.Log($"Quiz results saved in '{firestoreCollectionName}' with document '{firestoreDocumentName}'.");

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
        }
        else
        {
            // User failed case
            scoreFailed.text = $"{score}/{currentQuestions.Count}!";
            pnlPassed.SetActive(false);
            pnlFailed.SetActive(true);

            if (currentUser != null)
            {
                string userId = currentUser.UserId;

                DocumentReference quizDocRef = db.Collection("profile").Document(userId).Collection(firestoreCollectionName).Document(firestoreDocumentName);

                var failedQuizTask = quizDocRef.SetAsync(new
                {
                    score = score,
                    status = "failed",
                    timestamp = FieldValue.ServerTimestamp
                });

                yield return new WaitUntil(() => failedQuizTask.IsCompleted);

                if (failedQuizTask.IsFaulted || failedQuizTask.IsCanceled)
                {
                    Debug.LogError("Failed to save quiz results: " + failedQuizTask.Exception);
                    yield break;
                }
                Debug.Log($"Quiz results saved as 'failed' in '{firestoreCollectionName}' with document '{firestoreDocumentName}'.");
            }
            else
            {
                Debug.LogError("No user is logged in! Cannot save results.");
            }
        }
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

        // No score increment because it's marked wrong
        Debug.Log($"Time's up! Current Score: {score}");

        // Move to the next question
        currentQuestionIndex++;
        Invoke(nameof(LoadNextQuestion), 2f); // Proceed to the next question after 2 seconds
    }

    


    [System.Serializable]
    public class TrueFalseQuestion
    {
        public string questionText;
        public int correctAnswerIndex; // 0 for True, 1 for False
        public string correctStatement; // Correct explanation if answer is False
    }
}
