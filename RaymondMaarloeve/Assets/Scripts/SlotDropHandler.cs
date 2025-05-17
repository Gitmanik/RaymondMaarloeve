using UnityEngine;
using UnityEngine.EventSystems;

public class SlotDropHandler : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        var dragged = eventData.pointerDrag;
        if (dragged != null)
        {
            dragged.transform.SetParent(transform);
            var rt = dragged.GetComponent<RectTransform>();
            rt.anchoredPosition = Vector2.zero;
        }
    }
}
