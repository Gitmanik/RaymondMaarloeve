using UnityEngine;
using UnityEngine.EventSystems;

public class SlotDropHandler : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        GameObject dragged = eventData.pointerDrag;
        if (dragged != null)
        {
            dragged.transform.SetParent(transform);
            RectTransform rt = dragged.GetComponent<RectTransform>();
            rt.anchoredPosition = Vector2.zero;
        }
    }
}
