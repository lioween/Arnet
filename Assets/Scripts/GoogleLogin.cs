using System.Threading.Tasks;
using UnityEngine;
using TMPro;
using Firebase.Auth;
using Firebase.Firestore;
using Google;
using UnityEngine.SceneManagement;
using System.Collections;

public class GoogleLoginManager : MonoBehaviour
{
    private FirebaseAuth auth;
    private FirebaseUser user;
    private GoogleSignInConfiguration configuration;
    private FirebaseFirestore firestore;
    private string GoogleWebAPI = "669840688035-jaj1rh6ou6vr1ifjhatbuffqbud3uvqs.apps.googleusercontent.com";

    [Header("UI Elements")]
    public TMP_Text txtnotif;
    public GameObject loadingUI;

    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        firestore = FirebaseFirestore.DefaultInstance;

        // Google Sign-In Configuration
        configuration = new GoogleSignInConfiguration
        {
            WebClientId = GoogleWebAPI,
            RequestIdToken = true
        };
    }

    public void SignInWithGoogle()
    {

        GoogleSignIn.Configuration = configuration;
        GoogleSignIn.Configuration.UseGameSignIn = false;
        GoogleSignIn.Configuration.RequestIdToken = true;
        GoogleSignIn.Configuration.RequestEmail = true;

        // Start the Google Sign-In process
        StartCoroutine(GoogleSignInWithLoading());
    }

    private IEnumerator GoogleSignInWithLoading()
    {
        if (loadingUI != null) loadingUI.SetActive(true);

        var googleTask = GoogleSignIn.DefaultInstance.SignIn();
        yield return new WaitUntil(() => googleTask.IsCompleted);

        if (loadingUI != null) loadingUI.SetActive(false);

        if (googleTask.IsFaulted || googleTask.IsCanceled)
        {
            Debug.LogError($"Google Sign-In failed: {googleTask.Exception}");
            txtnotif.text = googleTask.IsCanceled ? "Google Sign-In canceled." : "Google Sign-In failed. Please try again.";
            GoogleSignIn.DefaultInstance.SignOut();
            yield break;
        }

        // Successful Google Sign-In
        GoogleSignInUser googleUser = googleTask.Result;
        Credential credential = GoogleAuthProvider.GetCredential(googleUser.IdToken, null);

        StartCoroutine(FirebaseSignInWithLoading(credential, googleUser));
    }

    private IEnumerator FirebaseSignInWithLoading(Credential credential, GoogleSignInUser googleUser)
    {
        if (loadingUI != null) loadingUI.SetActive(true);

        var authTask = auth.SignInWithCredentialAsync(credential);
        yield return new WaitUntil(() => authTask.IsCompleted);

        if (loadingUI != null) loadingUI.SetActive(false);

        if (authTask.IsFaulted || authTask.IsCanceled)
        {
            Debug.LogError($"Firebase Sign-In failed: {authTask.Exception}");
            txtnotif.text = "Sign-In failed. Please try again.";
            GoogleSignIn.DefaultInstance.SignOut();
            yield break;
        }

        user = auth.CurrentUser;
        if (user != null)
        {
            // Debug the retrieved email
            Debug.Log($"Firebase User Email: {user.Email}");
            if (string.IsNullOrEmpty(user.Email))
            {
                Debug.LogError("Firebase user does not have an email linked. Check Google Sign-In configuration.");
            }

            string firstName = googleUser.GivenName;
            string lastName = googleUser.FamilyName;

            // Save user data to Firestore
            var userDocRef = firestore.Collection("profile").Document(user.UserId);
            var getUserTask = userDocRef.GetSnapshotAsync();
            yield return new WaitUntil(() => getUserTask.IsCompleted);

            if (getUserTask.IsFaulted || getUserTask.IsCanceled)
            {
                Debug.LogError($"Failed to check if user document exists: {getUserTask.Exception}");
                txtnotif.text = "Failed to verify account. Please try again.";
                yield break;
            }

            var snapshot = getUserTask.Result;
            if (!snapshot.Exists)
            {
                Debug.Log($"No existing user document found for ID: {user.UserId}. Creating a new document.");
                StoreUserDataInFirestore(user.UserId, firstName, lastName);
            }

            // Load the appropriate scene
            StartCoroutine(CheckCollectionAndLoadScene(user.UserId));
        }
    }


    private void StoreUserDataInFirestore(string userId, string firstName, string lastName)
    {
        var userData = new
        {
            firstname = firstName,
            lastname = lastName,
            createdat = Timestamp.GetCurrentTimestamp()
        };

        firestore.Collection("profile").Document(userId).SetAsync(userData).ContinueWith(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                Debug.Log($"User data saved successfully for {firstName} {lastName}.");
            }
            else
            {
                Debug.LogError($"Failed to save user data: {task.Exception}");
            }
        });
    }

    private IEnumerator CheckCollectionAndLoadScene(string userId)
    {
        var collectionRef = firestore.Collection("profile").Document(userId).Collection("1.0");

        var queryTask = collectionRef.Limit(1).GetSnapshotAsync();
        yield return new WaitUntil(() => queryTask.IsCompleted);

        if (queryTask.IsFaulted || queryTask.IsCanceled)
        {
            Debug.LogError($"Error checking Firestore collection: {queryTask.Exception}");
            txtnotif.text = "Error loading data. Please try again.";
            yield break;
        }

        QuerySnapshot querySnapshot = queryTask.Result;
        if (querySnapshot == null || querySnapshot.Count == 0)
        {
            Debug.Log("Collection '1.0' does not exist or has no documents. Loading scene 'Welcome'.");
            SceneManager.LoadScene("Welcome");
        }
        else
        {
            Debug.Log("Collection '1.0' exists with documents. Loading scene 'Home'.");
            SceneManager.LoadScene("Home");
        }
    }
}
