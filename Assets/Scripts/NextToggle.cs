using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NextToggle : MonoBehaviour
{
    [Header("GameObject to Toggle")]

    public GameObject targetObject1;
    public GameObject targetObject2;


    /// <summary>
    /// Activates the target GameObject.
    /// </summary>
    public void NextPanel()
    {

        targetObject2.SetActive(true);
        targetObject1.SetActive(false);

    }

    public void EndPanel()
    {

        targetObject1.SetActive(false);

    }


}
