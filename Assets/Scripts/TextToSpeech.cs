using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class TextToSpeech : MonoBehaviour
{
    private AndroidJavaObject ttsObject;
    private Queue<string> textQueue = new Queue<string>();
    private string pausedChunk = null;
    private bool isPaused = false;
    private bool isSpeaking = false;

    void Start()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            try
            {
                AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

                ttsObject = new AndroidJavaObject("android.speech.tts.TextToSpeech", currentActivity, new TextToSpeechCallback(this));
                Debug.Log("TTS initialized successfully.");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"TTS initialization failed: {ex.Message}");
            }
        }
        else
        {
            Debug.LogWarning("TTS is only supported on Android.");
        }
    }

    public void Speak(string text)
    {
        if (Application.platform == RuntimePlatform.Android && ttsObject != null)
        {
            if (isPaused)
            {
                Debug.LogWarning("Cannot start new speech while paused. Resume first.");
                return;
            }

            int chunkSize = 4000;
            for (int i = 0; i < text.Length; i += chunkSize)
            {
                string chunk = text.Substring(i, Mathf.Min(chunkSize, text.Length - i));
                textQueue.Enqueue(chunk);
            }

            if (!isSpeaking)
            {
                SpeakNextChunk();
            }
        }
        else
        {
            Debug.LogWarning("TTS is not available or not on an Android platform.");
        }
    }

    private void SpeakNextChunk()
    {
        if (textQueue.Count > 0)
        {
            string chunk = textQueue.Dequeue();
            pausedChunk = chunk; // Save the chunk in case of pause
            isSpeaking = true;

            if (ttsObject != null)
            {
                Debug.Log("Applying speech rate inside the speech request...");


                // ✅ Call speak() using the bundle
                int result = ttsObject.Call<int>("speak", chunk, 0, null);

                if (result < 0)
                {
                    Debug.LogError("TTS speak call failed.");
                    isSpeaking = false;
                }
                else
                {
                    Debug.Log($"Speaking chunk with adjusted rate: {chunk}");
                }
            }
        }
        else
        {
            isSpeaking = false;
            Debug.Log("Finished speaking all queued text.");
        }
    }

   

    public void Pause()
    {
        if (Application.platform == RuntimePlatform.Android && ttsObject != null)
        {
            if (isSpeaking)
            {
                ttsObject.Call<int>("stop"); // Stops the current speech
                isPaused = true;
                isSpeaking = false;
                Debug.Log("TTS paused.");
            }
            else
            {
                Debug.LogWarning("Cannot pause, TTS is not speaking.");
            }
        }
    }

    public void Resume()
    {
        if (isPaused)
        {
            Debug.Log("Resuming TTS...");
            isPaused = false;

            if (!string.IsNullOrEmpty(pausedChunk))
            {
                textQueue.Enqueue(pausedChunk); // Re-enqueue the paused chunk
                pausedChunk = null;
            }

            SpeakNextChunk();
        }
        else
        {
            Debug.LogWarning("TTS is not paused, cannot resume.");
        }
    }

    public void Stop()
    {
        if (Application.platform == RuntimePlatform.Android && ttsObject != null)
        {
            try
            {
                ttsObject.Call<int>("stop");
                textQueue.Clear();
                isSpeaking = false;
                isPaused = false;
                pausedChunk = null;
                Debug.Log("TTS stopped successfully.");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to stop TTS: {ex.Message}");
            }
        }
        else
        {
            Debug.LogWarning("TTS is not available or not on an Android platform.");
        }
    }

    public bool IsSpeaking()
    {
        if (Application.platform == RuntimePlatform.Android && ttsObject != null)
        {
            return ttsObject.Call<bool>("isSpeaking");
        }
        return false;
    }

    private class TextToSpeechCallback : AndroidJavaProxy
    {
        private TextToSpeech parent;

        public TextToSpeechCallback(TextToSpeech parent) : base("android.speech.tts.TextToSpeech$OnInitListener")
        {
            this.parent = parent;
        }

        public void onInit(int status)
        {
            if (status == 0) // SUCCESS
            {
                Debug.Log("TTS initialized successfully.");


                Debug.Log("TTS is ready with Google TTS and adjusted speech rate.");
            }
            else
            {
                Debug.LogError("TTS initialization failed.");
            }
        }

    }

    void Update()
    {
        if (!IsSpeaking() && isSpeaking && !isPaused)
        {
            SpeakNextChunk();
        }
    }
}
