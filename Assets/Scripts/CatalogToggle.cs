using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatalogToggle : MonoBehaviour
{

    public GameObject pnlFull;
    public GameObject model1;
    public GameObject model2;

    public  void Full()
    {
        pnlFull.SetActive(true);
        model1.SetActive(false);
        model2.SetActive(true);
    }

    public void Exit()
    {
        pnlFull.SetActive(false);
        model1.SetActive(true);
        model2.SetActive(false);
    }
}
