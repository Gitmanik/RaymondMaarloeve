using UnityEngine;
using UnityEngine.EventSystems;

namespace YourNamespace
{
    /// <summary>
    /// Component responsible for handling UI drop events onto this slot.
    /// When an object is dropped, it is parented to this slot and centered.
    /// </summary>
    public class SlotDropHandler : MonoBehaviour, IDropHandler
    {
        /// <summary>
        /// Called by the UI EventSystem when a draggable object is dropped on this slot.
        /// </summary>
        /// <param name="eventData">
        /// Event data containing information about the drag event, including a reference
        /// to the <see cref="PointerEventData.pointerDrag"/> GameObject.
        /// </param>
        public void OnDrop(PointerEventData eventData)
        {
            // Get the dragged GameObject
            GameObject dragged = eventData.pointerDrag;
            if (dragged != null)
            {
                // Parent the dragged object to this slot
                dragged.transform.SetParent(transform);

                // Reset its RectTransform anchored position to center it
                RectTransform rt = dragged.GetComponent<RectTransform>();
                rt.anchoredPosition = Vector2.zero;
            }
        }
    }
}
