// Import necessary Unity Firebase libraries
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System;
using System.Threading.Tasks;

public class BookmarkManager : MonoBehaviour
{
    private FirebaseAuth auth;
    private FirebaseFirestore db;

    [SerializeField]
    public TMP_Text txtCategory;
    public TMP_Text txtTitle; // Reference to a TMP input field in your Unity scene
    public Button btnBookmark;
    public Button btnUnbookmark;
    

    void Start()
    {
        // Initialize Firebase Auth and Firestore
        auth = FirebaseAuth.DefaultInstance;
        db = FirebaseFirestore.DefaultInstance;

        CheckIfSceneBookmarked();
    }

    public async void AddBookmark()
    {
        try
        {
            // Get the current user
            FirebaseUser currentUser = auth.CurrentUser;

            btnBookmark.gameObject.SetActive(false);
            btnUnbookmark.gameObject.SetActive(true);

            if (currentUser == null)
            {
                throw new Exception("User not authenticated");
            }

            // Get the current scene name
            string sceneName = SceneManager.GetActiveScene().name;

            // Get the title from the TMP input field

            

            // Define the document path under the "profile" collection
            DocumentReference bookmarkRef = db
                .Collection("profile")
                .Document(currentUser.UserId)
                .Collection("bookmark")
                .Document(sceneName);

            // Create the bookmark data
            var bookmarkData = new
            {
                category = txtCategory.text,
                title = txtTitle.text,
                createdAt = Timestamp.GetCurrentTimestamp() // Optional: Add a timestamp
            };

            // Save the bookmark to Firestore
            await bookmarkRef.SetAsync(bookmarkData);

            
            Debug.Log("Bookmark added successfully!");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error adding bookmark: {ex.Message}");
        }
    }

    public async void RemoveBookmark()
    {
        try
        {
            // Get the current user
            FirebaseUser currentUser = auth.CurrentUser;

            btnBookmark.gameObject.SetActive(true);
            btnUnbookmark.gameObject.SetActive(false);

            if (currentUser == null)
            {
                throw new Exception("User not authenticated");
            }

            // Get the current scene name
            string sceneName = SceneManager.GetActiveScene().name;

            // Define the document path under the "profile" collection
            DocumentReference bookmarkRef = db
                .Collection("profile")
                .Document(currentUser.UserId)
                .Collection("bookmark")
                .Document(sceneName);

            // Delete the bookmark document
            await bookmarkRef.DeleteAsync();
            
            Debug.Log("Bookmark removed successfully!");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error removing bookmark: {ex.Message}");
        }
    }

    private async void CheckIfSceneBookmarked()
    {
        try
        {
            // Get the current user
            FirebaseUser currentUser = auth.CurrentUser;

            if (currentUser == null)
            {
                throw new Exception("User not authenticated");
            }

            // Get the current scene name
            string sceneName = SceneManager.GetActiveScene().name;

            // Check if the scene is bookmarked
            DocumentReference bookmarkRef = db
                .Collection("profile")
                .Document(currentUser.UserId)
                .Collection("bookmark")
                .Document(sceneName);

            DocumentSnapshot documentSnapshot = await bookmarkRef.GetSnapshotAsync();

            if (documentSnapshot.Exists)
            {
                btnBookmark.gameObject.SetActive(false);
                btnUnbookmark.gameObject.SetActive(true);
                Debug.Log("The current scene is bookmarked.");
                // Perform an action if bookmarked, e.g., enable/disable UI elements
            }
            else
            {
                btnBookmark.gameObject.SetActive(true);
                btnUnbookmark.gameObject.SetActive(false);
                Debug.Log("The current scene is not bookmarked.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error checking bookmark: {ex.Message}");
        }
    }


}
