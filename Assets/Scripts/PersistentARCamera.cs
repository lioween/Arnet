using UnityEngine;

public class PersistentARCamera : MonoBehaviour
{
    private static PersistentARCamera instance;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
