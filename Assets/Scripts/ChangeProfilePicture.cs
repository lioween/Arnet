using Firebase.Firestore;
using Firebase.Auth;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Android;
using System.Collections;

public class ProfilePictureManager : MonoBehaviour
{
    private FirebaseFirestore firestore;
    private FirebaseAuth auth;
    public Button profileImageUI; // UI Image where the profile picture will be displayed
    public GameObject loadingUI; // Optional: To show a loading spinner
    public GameObject pnlSaved;
    public GameObject pnlPhoto;
    public Button targetImage;

    void Start()
    {
        firestore = FirebaseFirestore.DefaultInstance;
        auth = FirebaseAuth.DefaultInstance;
    }

    // Method to save profile picture as Base64 string in Firestore
    public void SaveProfilePicture(Texture2D profilePicture)
    {
        StartCoroutine(SaveProfilePictureCoroutine(profilePicture));
    }

    private IEnumerator SaveProfilePictureCoroutine(Texture2D profilePicture)
    {
        loadingUI.SetActive(true);

        string base64Image = ImageHelper.ConvertToBase64(profilePicture);
        string userId = auth.CurrentUser.UserId;

        DocumentReference userRef = firestore.Collection("profile").Document(userId);

        var task = userRef.UpdateAsync("profilePicture", base64Image);
        yield return new WaitUntil(() => task.IsCompleted);

        loadingUI.SetActive(false);

        pnlSaved.SetActive(true);
        yield return new WaitForSeconds(1f);
        pnlSaved.SetActive(false);

        if (task.IsFaulted)
        {
            Debug.LogError($"Failed to save profile picture: {task.Exception}");
        }
        else
        {
            Debug.Log("Profile picture saved successfully!");
        }
    }


    // Method to open the gallery and pick a picture
    public void OpenGallery()
    {
       
            NativeGallery.GetImageFromGallery((path) =>
            {
                if (!string.IsNullOrEmpty(path))
                {
                    Texture2D texture = NativeGallery.LoadImageAtPath(path, 512, false); // Ensure texture is readable
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
                        profileImageUI.image.sprite = profileSprite;

                        // Ensure the aspect ratio is preserved
                        profileImageUI.image.preserveAspect = true;
                        SaveProfilePicture(texture); // Automatically save after selection
                    }
                    else
                    {
                        Debug.LogError("Failed to load texture from selected image.");
                    }
                }
            }, "Select a Profile Picture");
        
    }

    public void SeePhoto()
    {
        if (profileImageUI.image.sprite != null)
        {
            targetImage.image.sprite = profileImageUI.image.sprite; // Assign the sprite of source to target
            targetImage.image.preserveAspect = true; // Optional: Maintain the aspect ratio
            pnlPhoto.SetActive(true);
            Debug.Log("Image transferred successfully.");

        }
        else
        {
            Debug.LogWarning("Source image has no sprite to transfer.");
        }
    }

    public void ExitPhoto()
    {
        pnlPhoto.SetActive(false);
    }

}
