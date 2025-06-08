/**
 * @file SlotDropHandler.cs
 * @brief Handles dropping dragged objects into a UI slot.
 */

using UnityEngine;
using UnityEngine.EventSystems;

/**
 * @class SlotDropHandler
 * @brief Implements IDropHandler to handle drop events.
 */
public class SlotDropHandler : MonoBehaviour, IDropHandler
{
    /**
     * @brief Called when an object is dropped onto this slot.
     * @param eventData Data associated with the drag/drop event.
     */
    public void OnDrop(PointerEventData eventData)
    {
        GameObject dragged = eventData.pointerDrag;
        if (dragged != null)
        {
            // Move the dragged object into this slot and center it
            dragged.transform.SetParent(transform);
            RectTransform rt = dragged.GetComponent<RectTransform>();
            rt.anchoredPosition = Vector2.zero;
        }
    }
}
