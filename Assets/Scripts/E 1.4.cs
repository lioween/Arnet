using Firebase.Firestore;
using Firebase.Auth;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.EventSystems;
using System;

public class E14game : MonoBehaviour
{
    public GameObject[] PCToShow; // Assign objects in the Inspector
    public Button btnPC; // Assign the button
    public TMP_Text PCnumber; // Assign the TMP text inside the button
    private int PCcurrentIndex = 0; // Tracks which object to reveal
    private int PCcount = 5; // Default start number

    public GameObject[] RouterToShow; // Assign objects in the Inspector
    public Button btnRouter; // Assign the button
    public TMP_Text Routernumber; // Assign the TMP text inside the button
    private int RoutercurrentIndex = 0; // Tracks which object to reveal
    private int Routercount = 1; // Default start number

    void Start()
    {

        // Hide all objects at the start
        foreach (GameObject obj in PCToShow)
        {
            if (obj != null) obj.SetActive(false);
        }

        foreach (GameObject obj in RouterToShow)
        {
            if (obj != null) obj.SetActive(false);
        }
    }




    public void RevealPC()
    {
        if (PCcurrentIndex < PCToShow.Length)
        {
            PCToShow[PCcurrentIndex].SetActive(true); // Show the next object
            PCcurrentIndex++; // Move to the next object
        }

        // Reduce the number and update TMP text
        PCcount--;
        if (PCnumber != null)
        {
            PCnumber.text = PCcount.ToString();
        }

        // Disable button when count reaches 0
        if (PCcount <= 0)
        {
            btnPC.interactable = false;
        }
    }

    public void RevealRouter()
    {
        if (RoutercurrentIndex < RouterToShow.Length)
        {
            RouterToShow[RoutercurrentIndex].SetActive(true); // Show the next object
            RoutercurrentIndex++; // Move to the next object
        }

        // Reduce the number and update TMP text
        Routercount--;
        if (Routernumber != null)
        {
            Routernumber.text = Routercount.ToString();
        }

        // Disable button when count reaches 0
        if (Routercount <= 0)
        {
            btnRouter.interactable = false;
        }
    }

    

    
}
