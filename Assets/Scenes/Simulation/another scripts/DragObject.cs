using UnityEngine;
using UnityEngine.UI; // Required for UI elements

public class DragAndZoomObject : MonoBehaviour
{
    private Vector3 offset;
    private Camera mainCamera;
    private float zCoord; // Depth position relative to camera

    private float initialScale; // Initial scale of the object
    public float zoomSpeed = 0.1f; // Speed for zooming

    private static DragAndZoomObject selectedObject = null; // Track the currently selected object

    private Renderer objectRenderer; // Renderer for highlighting
    public Color highlightColor = Color.yellow; // Color when selected
    private Color originalColor; // Original color of the object

    // New variable for toggle activation
    public static bool isInteractionEnabled = false; // Static so it applies globally

    void Start()
    {
        // Cache the main camera for efficiency
        mainCamera = Camera.main;

        // Cache the initial scale
        initialScale = transform.localScale.x;

        // Cache the object's renderer and its original color
        objectRenderer = GetComponent<Renderer>();
        if (objectRenderer != null)
        {
            originalColor = objectRenderer.material.color;
        }
    }

    void Update()
    {
        // Only handle zoom if this object is selected and interaction is enabled
        if (isInteractionEnabled && selectedObject == this)
        {
            HandleZoom();
        }
    }

    void OnMouseDown()
    {
        // Allow selection only if interaction is enabled
        if (!isInteractionEnabled) return;

        // Select this object and deselect others
        if (selectedObject != null && selectedObject != this)
        {
            selectedObject.DeselectObject();
        }

        selectedObject = this;
        HighlightObject(true); // Highlight this object

        // Get the depth (z-coordinate) of the object relative to the camera
        zCoord = mainCamera.WorldToScreenPoint(transform.position).z;

        // Calculate the offset between the object position and the mouse world position
        offset = transform.position - GetMouseWorldPosition();
    }

    void OnMouseDrag()
    {
        // Allow dragging only if this object is selected and interaction is enabled
        if (isInteractionEnabled && selectedObject == this)
        {
            transform.position = GetMouseWorldPosition() + offset;
        }
    }

    private Vector3 GetMouseWorldPosition()
    {
        // Get the mouse position in screen space
        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = zCoord; // Maintain the depth position

        // Convert screen position to world position
        return mainCamera.ScreenToWorldPoint(mouseScreenPos);
    }

    private void HandleZoom()
    {
        // Mobile Pinch Zoom
        if (Input.touchCount == 2)
        {
            Touch touch1 = Input.GetTouch(0);
            Touch touch2 = Input.GetTouch(1);

            // Get the current and previous positions of the touches
            Vector2 touch1PrevPos = touch1.position - touch1.deltaPosition;
            Vector2 touch2PrevPos = touch2.position - touch2.deltaPosition;

            // Get the distance between the touches
            float prevTouchDeltaMag = (touch1PrevPos - touch2PrevPos).magnitude;
            float currentTouchDeltaMag = (touch1.position - touch2.position).magnitude;

            // Find the difference in distances
            float deltaMagnitudeDiff = currentTouchDeltaMag - prevTouchDeltaMag;

            // Adjust the scale of the object
            ScaleObject(deltaMagnitudeDiff * zoomSpeed);
        }

        // Mouse Scroll Zoom
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            ScaleObject(scroll * zoomSpeed * 10f); // Increase speed for mouse scroll
        }
    }

    private void ScaleObject(float scaleDelta)
    {
        // Clamp the scale to avoid object disappearing or being too large
        float newScale = Mathf.Clamp(transform.localScale.x + scaleDelta, initialScale * 0.5f, initialScale * 2f);
        transform.localScale = new Vector3(newScale, newScale, newScale);
    }

    private void HighlightObject(bool highlight)
    {
        // Change the object's color to highlight it
        if (objectRenderer != null)
        {
            objectRenderer.material.color = highlight ? highlightColor : originalColor;
        }
    }

    public void DeselectObject()
    {
        // Reset the selected object
        if (selectedObject == this)
        {
            selectedObject = null;
        }

        // Remove the highlight from this object
        HighlightObject(false);
    }

    // Function to toggle interaction globally
    public static void ToggleInteraction()
    {
        isInteractionEnabled = !isInteractionEnabled;
    }
}
