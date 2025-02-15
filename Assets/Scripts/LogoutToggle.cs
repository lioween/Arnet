using UnityEngine;
using UnityEngine.UI;

public class ToggleObject : MonoBehaviour
{
    [Header("GameObject to Toggle")]
    
    public GameObject targetObject;

    /// <summary>
    /// Activates the target GameObject.
    /// </summary>
    public void SetActive()
    {
       
        targetObject.SetActive(true);

    }

    /// <summary>
    /// Deactivates the target GameObject.
    /// </summary>
    public void SetInactive()
    {
        
        targetObject.SetActive(false);

       
    }

    /// <summary>
    /// Toggles the active state of the target GameObject.
    /// </summary>
    
}
