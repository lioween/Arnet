using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class NestedScrollFix : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    private ScrollRect parentScrollRect; // Vertical ScrollView
    private ScrollRect horizontalScrollRect; // Horizontal ScrollView
    private bool isVerticalDrag; // Tracks whether the user is dragging vertically

    void Start()
    {
        horizontalScrollRect = GetComponent<ScrollRect>();
        parentScrollRect = transform.parent.GetComponentInParent<ScrollRect>(); // Find the parent vertical ScrollView
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Detect if the drag is mostly vertical
        isVerticalDrag = Mathf.Abs(eventData.delta.y) > Mathf.Abs(eventData.delta.x);

        if (isVerticalDrag)
        {
            // Forward drag to parent (allow vertical scrolling)
            if (parentScrollRect != null)
            {
                parentScrollRect.OnBeginDrag(eventData);
                horizontalScrollRect.enabled = false; // Disable horizontal scrolling momentarily
            }
        }
        else
        {
            // If dragging horizontally, disable vertical scrolling
            if (parentScrollRect != null)
                parentScrollRect.vertical = false;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isVerticalDrag && parentScrollRect != null)
        {
            parentScrollRect.OnDrag(eventData); // Forward drag to vertical ScrollView
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (parentScrollRect != null)
        {
            parentScrollRect.OnEndDrag(eventData);
            parentScrollRect.vertical = true; // Re-enable vertical scrolling
        }

        horizontalScrollRect.enabled = true; // Re-enable horizontal scrolling
    }
}
