using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ShowHidePw : MonoBehaviour
{
    public TMP_InputField passwordInputField; // Reference to the password input field
    public Button showButton;                // Button to show the password
    public Button hideButton;                // Button to hide the password

    public void ShowPassword()
    {
        if (passwordInputField != null)
        {
            passwordInputField.contentType = TMP_InputField.ContentType.Standard; // Show plain text
            passwordInputField.ForceLabelUpdate();                               // Apply the change

            showButton.gameObject.SetActive(false);
            hideButton.gameObject.SetActive(true);
        }

    }

    public void HidePassword()
    {
        if (passwordInputField != null)
        {
            passwordInputField.contentType = TMP_InputField.ContentType.Password; // Hide as dots
            passwordInputField.ForceLabelUpdate();                               // Apply the change

            showButton.gameObject.SetActive(true);
            hideButton.gameObject.SetActive(false);
        }

    }

    
}
