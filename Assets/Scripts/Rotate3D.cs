using UnityEngine;
using System.Collections;

public class RotateAndZoom3DModel : MonoBehaviour
{
    public float rotationSpeed = 5f;
    public float autoRotationSpeed = 10f; // Slower auto-rotation speed
    public float elasticity = 2f;
    public float damping = 0.85f;

    public float zoomSpeed = 0.05f;
    public float maxZoomMultiplier = 2.5f;

    private Vector3 initialScale;
    private Vector2 lastTouchPosition;
    private bool isDragging = false;
    private Vector3 currentVelocity;
    private bool isHorizontalSwipe = false;

    private bool isAutoRotating = true; // Controls auto-rotation
    private float lastInteractionTime; // Tracks last user interaction

    void Start()
    {
        initialScale = transform.localScale;
        lastInteractionTime = Time.time; // Start auto-rotation immediately
    }

    void Update()
    {
        HandleRotation();
        HandleZoom();
        ApplyElasticEffect();

        // Restart auto-rotation if no interaction for 5 seconds
        if (!isDragging && (Time.time - lastInteractionTime > 3f))
        {
            isAutoRotating = true;
        }

        if (isAutoRotating)
        {
            AutoRotate();
        }
    }

    void HandleRotation()
    {
        if (Input.touchCount == 1)
        {
            StopAutoRotation(); // Stop auto-rotation on touch

            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                lastTouchPosition = touch.position;
                isDragging = true;
                isHorizontalSwipe = false;
            }
            else if (touch.phase == TouchPhase.Moved && isDragging)
            {
                Vector2 delta = touch.position - lastTouchPosition;

                if (!isHorizontalSwipe)
                {
                    if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                        isHorizontalSwipe = true;
                }

                if (isHorizontalSwipe)
                {
                    RotateModel(delta.x);
                    currentVelocity = new Vector3(0, -delta.x * rotationSpeed * Time.deltaTime, 0);
                }

                lastTouchPosition = touch.position;
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                isDragging = false;
                isHorizontalSwipe = false;
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            StopAutoRotation(); // Stop auto-rotation on mouse interaction

            lastTouchPosition = Input.mousePosition;
            isDragging = true;
            isHorizontalSwipe = false;
        }
        else if (Input.GetMouseButton(0) && isDragging)
        {
            Vector2 delta = (Vector2)Input.mousePosition - lastTouchPosition;

            if (!isHorizontalSwipe)
            {
                if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                    isHorizontalSwipe = true;
            }

            if (isHorizontalSwipe)
            {
                RotateModel(delta.x);
                currentVelocity = new Vector3(0, -delta.x * rotationSpeed * Time.deltaTime, 0);
            }

            lastTouchPosition = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            isHorizontalSwipe = false;
        }
    }

    void HandleZoom()
    {
        if (Input.touchCount == 2)
        {
            StopAutoRotation(); // Stop auto-rotation when zooming

            Touch touch1 = Input.GetTouch(0);
            Touch touch2 = Input.GetTouch(1);

            float prevDistance = (touch1.position - touch1.deltaPosition - (touch2.position - touch2.deltaPosition)).magnitude;
            float currentDistance = (touch1.position - touch2.position).magnitude;

            float deltaDistance = currentDistance - prevDistance;
            float scaleFactor = 1 + (deltaDistance * zoomSpeed * Time.deltaTime);

            ZoomModel(scaleFactor);
        }

        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            StopAutoRotation(); // Stop auto-rotation on scroll zoom

            float scaleFactor = 1 + (Input.GetAxis("Mouse ScrollWheel") * zoomSpeed);
            ZoomModel(scaleFactor);
        }
    }

    void ZoomModel(float scaleFactor)
    {
        Vector3 newScale = transform.localScale * scaleFactor;
        float maxZoom = initialScale.x * maxZoomMultiplier;

        newScale.x = Mathf.Clamp(newScale.x, initialScale.x, maxZoom);
        newScale.y = Mathf.Clamp(newScale.y, initialScale.y, maxZoom);
        newScale.z = Mathf.Clamp(newScale.z, initialScale.z, maxZoom);

        transform.localScale = newScale;
    }

    void RotateModel(float deltaX)
    {
        float rotationY = -deltaX * rotationSpeed * Time.deltaTime;
        transform.Rotate(0, rotationY, 0, Space.Self);
    }

    void ApplyElasticEffect()
    {
        if (!isDragging)
        {
            currentVelocity *= damping;
            transform.Rotate(currentVelocity * elasticity * Time.deltaTime, Space.Self);
        }
    }

    void AutoRotate()
    {
        transform.Rotate(0, autoRotationSpeed * Time.deltaTime, 0, Space.Self);
    }

    void StopAutoRotation()
    {
        isAutoRotating = false;
        lastInteractionTime = Time.time; // Reset the timer
    }
}
