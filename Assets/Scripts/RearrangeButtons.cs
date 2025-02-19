using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RearrangeButton : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Transform parentAfterDrag;
    private LayoutElement layoutElement;
    private int originalSiblingIndex;

    void Start()
    {
        layoutElement = GetComponent<LayoutElement>();
        if (layoutElement == null)
        {
            layoutElement = gameObject.AddComponent<LayoutElement>();
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        parentAfterDrag = transform.parent;
        originalSiblingIndex = transform.GetSiblingIndex();

        // Disable layout updates while dragging
        layoutElement.ignoreLayout = true;
        transform.SetParent(parentAfterDrag.parent);
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Re-enable layout and return to original parent
        transform.SetParent(parentAfterDrag);
        layoutElement.ignoreLayout = false;

        // Find the closest sibling index and insert the button there
        int newIndex = FindClosestSiblingIndex();
        transform.SetSiblingIndex(newIndex);
    }

    private int FindClosestSiblingIndex()
    {
        Transform panel = parentAfterDrag;
        int closestIndex = 0;
        float closestDistance = float.MaxValue;

        for (int i = 0; i < panel.childCount; i++)
        {
            Transform child = panel.GetChild(i);
            if (child == transform) continue;

            float distance = Mathf.Abs(transform.position.y - child.position.y);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestIndex = i;
            }
        }

        return closestIndex;
    }
}
