using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ImageTracking : MonoBehaviour
{
    private ARTrackedImageManager trackedImageManager;

    void Awake()
    {
        trackedImageManager = FindObjectOfType<ARTrackedImageManager>();
    }

    void OnEnable()
    {
        trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    void OnDisable()
    {
        trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs args)
    {
        foreach (var trackedImage in args.added)
        {
            // Enable object kapag detected
            trackedImage.transform.GetChild(0).gameObject.SetActive(true);
        }

        foreach (var trackedImage in args.removed)
        {
            // Disable object kapag wala na
            trackedImage.transform.GetChild(0).gameObject.SetActive(false);
        }
    }
}

