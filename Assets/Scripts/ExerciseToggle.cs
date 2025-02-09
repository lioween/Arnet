using UnityEngine;
using UnityEngine.UI;

public class ToggleGameObject : MonoBehaviour
{
    [Header("GameObject to Toggle")]
    public GameObject pnlExercise;
    public GameObject blur;
    public GameObject entireBlur;
    public GameObject imgDown;
    public GameObject imgUp;




    /// <summary>
    /// Activates the target GameObject.
    /// </summary>
    public void Set()
    {

        if (!pnlExercise.activeSelf)
            {
           
            pnlExercise.SetActive(true);
            blur.SetActive(true);
            entireBlur.SetActive(true);
            imgUp.SetActive(true);
            imgDown.SetActive(false);



        }
        else
        {
           
            pnlExercise.SetActive(false);
            blur.SetActive(false);
            entireBlur.SetActive(false);
            imgUp.SetActive(false);
            imgDown.SetActive(true);
        }


    }

    /// <summary>
    /// Toggles the active state of the target GameObject.
    /// </summary>

}
