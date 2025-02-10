using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private Vector2 pointerDownPosition;
    private bool isDragging = false;
    private float screenWidth;
    private const float dragThreshold = 10f;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();

        if (canvas == null)
        {
            Debug.LogError("❌ No Canvas found! Make sure the button is inside a UI Canvas.");
            return;
        }

        screenWidth = Screen.width;
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        pointerDownPosition = eventData.position;
        isDragging = false;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (Vector2.Distance(pointerDownPosition, eventData.position) > dragThreshold)
        {
            isDragging = true;
            canvasGroup.blocksRaycasts = false; // ✅ Prevents blocking interactions while dragging
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging || canvas == null) return;

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform, eventData.position, eventData.pressEventCamera, out localPoint
        );

        rectTransform.anchoredPosition = localPoint; // ✅ Ensures proper positioning in UI space
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging) return;

        float middleX = screenWidth / 2;
        float buttonX = rectTransform.position.x;

        // ✅ Smoothly snap to left or right edge
        if (buttonX < middleX)
        {
            SnapToEdge(Vector2.left);
        }
        else
        {
            SnapToEdge(Vector2.right);
        }

        canvasGroup.blocksRaycasts = true; // ✅ Restore raycast interaction
        isDragging = false;
    }

    private void SnapToEdge(Vector2 direction)
    {
        float targetX = direction == Vector2.left ? 50f : screenWidth - 50f;
        Vector2 targetPosition = new Vector2(targetX, rectTransform.position.y);

        rectTransform.position = targetPosition; // ✅ Instantly snap (consider Lerp for smooth effect)
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (isDragging)
        {
            Debug.Log("❌ Click disabled due to dragging.");
            return;
        }

        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.Invoke();
        }
    }
}
