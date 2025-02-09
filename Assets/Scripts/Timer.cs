using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TimerWithAction : MonoBehaviour
{
    public float timeRemaining = 30f; // Set the timer duration to 30 seconds
    public bool timerIsRunning = false;
    public TMP_Text timerText; // Assign a Text UI element in the Inspector

    private void Start()
    {
        timerIsRunning = true; // Start the timer
    }

    void Update()
    {
        if (timerIsRunning)
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime; // Decrease the time
                UpdateTimerDisplay(timeRemaining); // Update the timer display
            }
            else
            {
                Debug.Log("Time's up!");
                timeRemaining = 0;
                timerIsRunning = false;

                PerformAction(); // Call the action when the timer ends
            }
        }
    }

    void UpdateTimerDisplay(float timeToDisplay)
    {
        timeToDisplay = Mathf.Clamp(timeToDisplay, 0, Mathf.Infinity); // Ensure time is not negative
        int seconds = Mathf.FloorToInt(timeToDisplay); // Convert float to int for seconds
        timerText.text = $"{seconds}"; // Update the UI text
    }

    void PerformAction()
    {
        // Replace this with your custom action
        Debug.Log("Performing action after the timer!");
        // Example: Change scene, show a message, or enable a GameObject
        // Example: SceneManager.LoadScene("NextScene");
    }
}
