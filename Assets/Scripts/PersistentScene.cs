using UnityEngine;

public class PersistScene : MonoBehaviour
{
    private void Awake()
    {
        // Mark all root objects in the scene as persistent
        foreach (GameObject rootObj in gameObject.scene.GetRootGameObjects())
        {
            DontDestroyOnLoad(rootObj);
        }
    }
}