using UnityEngine;

public static class ImageHelper
{
    // Convert Texture2D to Base64 string
    public static string ConvertToBase64(Texture2D texture)
    {
        byte[] imageBytes = texture.EncodeToPNG(); // Convert texture to PNG bytes
        return System.Convert.ToBase64String(imageBytes); // Convert to Base64 string
    }

    // Convert Base64 string to Texture2D
    public static Texture2D ConvertFromBase64(string base64)
    {
        byte[] imageBytes = System.Convert.FromBase64String(base64);
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(imageBytes); // Convert Base64 string to Texture2D
        return texture;
    }
}
