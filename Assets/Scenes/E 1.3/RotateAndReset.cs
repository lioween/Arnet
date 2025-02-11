using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateAndReset : MonoBehaviour
{
    private Quaternion initialRotation;
    private bool isRotating = false;
    private float resetTime = 2f;
    private float rotationSpeed = 5f;

    void Start()
    {
        initialRotation = transform.rotation;
    }

    void Update()
    {
        // For Mouse Drag (PC)
        if (Input.GetMouseButton(0))
        {
            float rotateX = Input.GetAxis("Mouse X") * rotationSpeed;
            float rotateY = Input.GetAxis("Mouse Y") * rotationSpeed;
            transform.Rotate(Vector3.up, -rotateX, Space.World);
            transform.Rotate(Vector3.right, rotateY, Space.World);
            isRotating = true;
            CancelInvoke("ResetRotation");
        }
        else if (isRotating)
        {
            isRotating = false;
            Invoke("ResetRotation", resetTime);
        }
    }

    void ResetRotation()
    {
        StartCoroutine(SmoothReset());
    }

    System.Collections.IEnumerator SmoothReset()
    {
        float duration = 1.3f;  // Smooth reset duration
        float elapsed = 0f;

        Quaternion startRotation = transform.rotation;

        while (elapsed < duration)
        {
            transform.rotation = Quaternion.Lerp(startRotation, initialRotation, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.rotation = initialRotation;
    }
}

