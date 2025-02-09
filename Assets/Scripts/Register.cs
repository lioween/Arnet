using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using TMPro;
using System.Collections;
using System.Text.RegularExpressions;

public class FirebaseRegistration : MonoBehaviour
{
    private FirebaseAuth auth;
    private FirebaseFirestore firestore;

    // UI Elements
    public TMP_InputField firstNameInput;
    public TMP_InputField lastNameInput;
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public TMP_Text txtFirstNameError; // Error message for first name
    public TMP_Text txtLastNameError;  // Error message for last name
    public TMP_Text txtEmailError;     // Error message for email
    public TMP_Text txtPasswordError;  // Error message for password
    public GameObject loadingUI;
    public GameObject pnlnotif;
    public TMP_Text pnlemail;

    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        firestore = FirebaseFirestore.DefaultInstance;

        // Attach real-time validation to input fields
        firstNameInput.onValueChanged.AddListener(ValidateFirstNameOnChange);
        lastNameInput.onValueChanged.AddListener(ValidateLastNameOnChange);
        emailInput.onValueChanged.AddListener(ValidateEmailOnChange);
        passwordInput.onValueChanged.AddListener(ValidatePasswordOnChange);

        ClearErrorMessages();
    }

    public void RegisterUser()
    {
        string firstName = firstNameInput.text.Trim();
        string lastName = lastNameInput.text.Trim();
        string email = emailInput.text.Trim();
        string password = passwordInput.text;

        ClearErrorMessages();

        bool hasError = false;

        // Final validation for all fields
        if (!ValidateField(firstName, txtFirstNameError, "This field is required."))
            hasError = true;

        if (!ValidateField(lastName, txtLastNameError, "This field is required."))
            hasError = true;

        if (!ValidateEmail(email, txtEmailError))
            hasError = true;

        if (!ValidatePassword(password, out string passwordErrorMessage))
        {
            txtPasswordError.text = passwordErrorMessage;
            hasError = true;
        }

        if (hasError) return;

        StartCoroutine(RegisterWithLoading(email, password));
    }

    private IEnumerator RegisterWithLoading(string email, string password)
    {
        string firstName = firstNameInput.text.Trim();
        string lastName = lastNameInput.text.Trim();

        loadingUI.SetActive(true);

        var task = auth.CreateUserWithEmailAndPasswordAsync(email, password);
        yield return new WaitUntil(() => task.IsCompleted);

        loadingUI.SetActive(false);

        if (task.IsFaulted || task.IsCanceled)
        {
            foreach (var innerException in task.Exception.Flatten().InnerExceptions)
            {
                if (innerException is FirebaseException firebaseEx)
                {
                    HandleAuthError((AuthError)firebaseEx.ErrorCode);
                }
            }
            yield break;
        }

        if (task.IsCompletedSuccessfully)
        {
            AuthResult result = task.Result;
            FirebaseUser newUser = result.User;
            SaveUserDetails(newUser.UserId, firstName, lastName);
            SendVerificationEmail(newUser);

            // Clear input fields
            firstNameInput.text = "";
            lastNameInput.text = "";
            emailInput.text = "";
            passwordInput.text = "";
            ClearErrorMessages();

            // Show notification panel
            pnlemail.text = email;
            pnlnotif.SetActive(true);

            auth.SignOut();
        }
    }

    // Real-time validation methods
    private void ValidateFirstNameOnChange(string firstName)
    {
        ValidateField(firstName, txtFirstNameError, "This field is required.");
    }

    private void ValidateLastNameOnChange(string lastName)
    {
        ValidateField(lastName, txtLastNameError, "This field is required.");
    }

    private void ValidateEmailOnChange(string email)
    {
        ValidateEmail(email, txtEmailError);
    }

    private void ValidatePasswordOnChange(string password)
    {
        if (!ValidatePassword(password, out string passwordErrorMessage))
        {
            txtPasswordError.text = passwordErrorMessage;
        }
        else
        {
            txtPasswordError.text = "";
        }
    }

    // General field validation
    private bool ValidateField(string value, TMP_Text errorText, string errorMessage)
    {
        if (string.IsNullOrEmpty(value))
        {
            errorText.text = errorMessage;
            return false;
        }

        errorText.text = ""; // Clear error
        return true;
    }

    // Email validation
    private bool ValidateEmail(string email, TMP_Text errorText)
    {
        if (string.IsNullOrEmpty(email))
        {
            errorText.text = "This field is required.";
            return false;
        }

        if (!IsValidEmail(email))
        {
            errorText.text = "Invalid email format.";
            return false;
        }

        errorText.text = ""; // Clear error
        return true;
    }

    // Password validation
    private bool ValidatePassword(string password, out string errorMessage)
    {
        errorMessage = "";

        if (password.Length < 8)
        {
            errorMessage = "Must be at least 8 characters long.";
            return false;
        }

        if (!Regex.IsMatch(password, @"[A-Z]"))
        {
            errorMessage = "Must include at least one uppercase letter.";
            return false;
        }

        if (!Regex.IsMatch(password, @"[a-z]"))
        {
            errorMessage = "Must include at least one lowercase letter.";
            return false;
        }

        if (!Regex.IsMatch(password, @"\d"))
        {
            errorMessage = "Must include at least one number.";
            return false;
        }

        if (!Regex.IsMatch(password, @"[@$!%*?&#+]"))
        {
            errorMessage = "Must include at least one special character.";
            return false;
        }

        return true;
    }

    private bool IsValidEmail(string email)
    {
        string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return Regex.IsMatch(email, emailPattern);
    }

    private void ClearErrorMessages()
    {
        txtFirstNameError.text = "";
        txtLastNameError.text = "";
        txtEmailError.text = "";
        txtPasswordError.text = "";
    }

    private void SaveUserDetails(string userId, string firstName, string lastName)
    {
        var userDoc = firestore.Collection("profile").Document(userId);
        var userDetails = new
        {
            firstname = firstName,
            lastname = lastName,
            createdat = Timestamp.GetCurrentTimestamp()
        };

        userDoc.SetAsync(userDetails).ContinueWith(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                Debug.Log("User details saved successfully.");
            }
            else
            {
                Debug.LogError($"Failed to save user details: {task.Exception}");
            }
        });
    }

    private void SendVerificationEmail(FirebaseUser user)
    {
        user.SendEmailVerificationAsync().ContinueWith(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                Debug.Log("Verification email sent.");
            }
            else
            {
                Debug.LogError($"Failed to send verification email: {task.Exception}");
            }
        });
    }

    private void HandleAuthError(AuthError errorCode)
    {
        string emailMessage = "";
        string passwordMessage = "";

        switch (errorCode)
        {
            case AuthError.EmailAlreadyInUse:
                emailMessage = "The email is already in use.";
                break;
            case AuthError.InvalidEmail:
                emailMessage = "The email address is invalid.";
                break;
            case AuthError.WeakPassword:
                passwordMessage = "The password is too weak.";
                break;
            default:
                emailMessage = "Registration failed. Please try again.";
                break;
        }

        if (!string.IsNullOrEmpty(emailMessage))
            txtEmailError.text = emailMessage;

        if (!string.IsNullOrEmpty(passwordMessage))
            txtPasswordError.text = passwordMessage;
    }
}
