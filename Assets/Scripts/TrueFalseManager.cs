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
    public Button[] optionButtons; // 2 Buttons: True & False
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

        if (score > 6)
        {
            scorePassed.text = $"{score}/{currentQuestions.Count}!";
            pnlPassed.SetActive(true);
            pnlFailed.SetActive(false);
            SaveQuizResult("passed");
        }
        else
        {
            scoreFailed.text = $"{score}/{currentQuestions.Count}!";
            pnlPassed.SetActive(false);
            pnlFailed.SetActive(true);
            SaveQuizResult("failed");
        }
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

        TrueFalseQuestion currentQuestion = currentQuestions[currentQuestionIndex];
        questionText.text = currentQuestion.questionText;

        optionButtons[0].GetComponentInChildren<TMP_Text>().text = "True";
        optionButtons[1].GetComponentInChildren<TMP_Text>().text = "False";

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
            isTimerRunning = false;
            timerText.text = "Time's up!";
            HandleTimesUp();
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

    public void TryAgain()
    {
        pnlFailed.SetActive(false);
        pnlStart.SetActive(true);
        score = 0;
        currentQuestionIndex = 0;
        selectedAnswerIndex = -1;
        timeRemaining = 30f;
        isTimerRunning = false;

        foreach (Image circle in progressCircles)
        {
            circle.color = defaultColor;
        }

        foreach (Button button in optionButtons)
        {
            button.GetComponent<Image>().color = defaultColor;
            button.interactable = true;
            TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>();
            if (buttonText != null)
            {
                buttonText.text = "";
            }
        }

        timerText.text = Mathf.CeilToInt(timeRemaining).ToString();
        nextButtonText.text = "Next Question";
    }

    private void SaveQuizResult(string status)
    {
        if (currentUser != null)
        {
            string userId = currentUser.UserId;
            DocumentReference quizDocRef = db.Collection("profile").Document(userId).Collection(firestoreCollectionName).Document(firestoreDocumentName);

            quizDocRef.SetAsync(new
            {
                title = txtTitle.text,
                score = score,
                status = status,
                timestamp = FieldValue.ServerTimestamp
            });
        }
    }

    [System.Serializable]
    public class TrueFalseQuestion
    {
        public string questionText;
        public int correctAnswerIndex; // 0 for True, 1 for False
    }
}
