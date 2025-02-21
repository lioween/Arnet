using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class Loadscene : MonoBehaviour
{
    private static Loadscene instance; // Singleton instance
    private static List<string> sceneHistory = new List<string>();

    void Awake()
    {
        // Destroy this object if another instance already exists
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // Set this as the active instance
        instance = this;
        DontDestroyOnLoad(gameObject);

        // Subscribe to the sceneLoaded event to track every scene change
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        // Unsubscribe to prevent memory leaks
        if (instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string currentScene = scene.name;

        // Always add the newly loaded scene to history
        if (sceneHistory.Count == 0 || sceneHistory[sceneHistory.Count - 1] != currentScene)
        {
            sceneHistory.Add(currentScene);

            // Keep only the last 10 scenes
            if (sceneHistory.Count > 10)
            {
                sceneHistory.RemoveAt(0);
            }
        }

        Debug.Log($"[Scene Tracker] Scene Loaded: {currentScene}");
        LogSceneHistory();
    }

    public void LoadSceneByName(string sceneName)
    {
        Debug.Log($"[Scene Tracker] Loading Scene: {sceneName}");
        SceneManager.LoadScene(sceneName);
    }


    public void GoBack(string fallbackScene)
    {
        if (sceneHistory.Count > 1)
        {
            // Remove the current scene from history
            sceneHistory.RemoveAt(sceneHistory.Count - 1);
            string lastScene = sceneHistory[sceneHistory.Count - 1];

            if (lastScene == "Bookmarks" || lastScene == "Exercises")
            {
                Debug.Log($"[Scene Tracker] Going Back to: {lastScene}");
                LogSceneHistory();
                SceneManager.LoadScene(lastScene);
            }
            else
            {
                Debug.LogWarning($"[Scene Tracker] Last scene was '{lastScene}'. Redirecting to fallback scene: {fallbackScene}");
                SceneManager.LoadScene(fallbackScene); // Use the fallback scene from Inspector
            }
        }
        else
        {
            Debug.LogWarning("[Scene Tracker] No previous scene available in history!");
        }
    }

    private void LogSceneHistory()
    {
        Debug.Log("[Scene Tracker] Scene History: " + string.Join(" -> ", sceneHistory));
    }







public void LoadSceneAdditive(string sceneName)
    {
        SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
    }

    public void UnloadScene(string sceneName)
    {
        SceneManager.UnloadSceneAsync(sceneName);
    }

    
}