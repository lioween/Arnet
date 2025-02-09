using UnityEngine;
using TMPro;
using Firebase.Extensions;
using Firebase.Auth;
using Firebase.Firestore;
using Firebase;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Threading.Tasks;

public class ProfileLoader : MonoBehaviour
{
    [SerializeField]
    public TMP_Text nameText;
    public TMP_Text emailText;
    public Button profilePicture;
    public GameObject loadingUI;

    private FirebaseAuth auth;
    private FirebaseFirestore firestore;

    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        firestore = FirebaseFirestore.DefaultInstance;

        // Check if user is logged in
        FirebaseUser user = auth.CurrentUser;
        if (user != null)
        {
            string userId = user.UserId;
            StartCoroutine(FetchAndDisplayUserProfile(userId));
        }
        else
        {
            Debug.LogWarning("No user is currently logged in.");
            nameText.text = "Guest"; // Default text for guests
        }
    }

    private IEnumerator FetchAndDisplayUserProfile(string userId)
    {
        loadingUI.SetActive(true);
        // Firestore collection reference
        DocumentReference docRef = firestore.Collection("profile").Document(userId);

        // Fetch document
        Task<DocumentSnapshot> task = docRef.GetSnapshotAsync();
        yield return new WaitUntil(() => task.IsCompleted); // Wait until the task completes

        loadingUI.SetActive(false);

        if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("Failed to fetch user data: " + task.Exception?.Message);
                nameText.text = "Error"; // Default text if fetch fails
            }

            var documentSnapshot = task.Result;
            if (documentSnapshot.Exists)
            {
                // Retrieve user data from Firestore document
                string firstName = documentSnapshot.GetValue<string>("firstname");
                string lastName = documentSnapshot.GetValue<string>("lastname");

                // Update UI with user data
                nameText.text = firstName + " " + lastName;
                emailText.text = auth.CurrentUser.Email;

                string base64Image = documentSnapshot.GetValue<string>("profilePicture");

                if (!string.IsNullOrEmpty(base64Image))
                {
                    // Load and display the profile picture
                    StartCoroutine(LoadProfilePicture(base64Image));
                }
            }
            else
            {
                Debug.LogWarning("User document does not exist in Firestore.");
                nameText.text = "User"; // Default text if no document is found
            }
        
    }

    private IEnumerator LoadProfilePicture(string base64Image)
    {
        // Convert Base64 string to a Texture2D
        Texture2D texture = ImageHelper.ConvertFromBase64(base64Image);

        if (texture != null)
        {
            // Determine the shorter dimension to center-crop the texture
            int cropSize = Mathf.Min(texture.width, texture.height);
            int offsetX = (texture.width - cropSize) / 2;
            int offsetY = (texture.height - cropSize) / 2;

            // Crop the texture to a 1:1 ratio
            Texture2D croppedTexture = new Texture2D(cropSize, cropSize);
            croppedTexture.SetPixels(texture.GetPixels(offsetX, offsetY, cropSize, cropSize));
            croppedTexture.Apply();

            // Create a sprite from the cropped texture and apply it to the UI
            Sprite profileSprite = Sprite.Create(croppedTexture, new Rect(0, 0, cropSize, cropSize), new Vector2(0.5f, 0.5f));
            profilePicture.image.sprite = profileSprite;

            // Ensure the aspect ratio is preserved
            profilePicture.image.preserveAspect = true;
        }
        else
        {
            Debug.LogError("Failed to convert Base64 string to texture.");
        }

        yield return null;
    }



}
