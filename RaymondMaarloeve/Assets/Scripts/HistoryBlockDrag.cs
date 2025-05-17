using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CanvasGroup))]
public class HistoryBlockDrag : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    CanvasGroup canvasGroup;
    RectTransform rect;
    Transform startParent;
    Vector2 startPosition;
    Canvas rootCanvas;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rect = GetComponent<RectTransform>();
        rootCanvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        startParent = transform.parent;
        startPosition = rect.anchoredPosition;
        transform.SetParent(rootCanvas.transform);
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rect.anchoredPosition += eventData.delta / rootCanvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Jeżeli upuszczono na SequencePanel lub PoolPanel, zostanie tam parentowany przez SlotDropHandler
        if (transform.parent == rootCanvas.transform)
        {
            // nie trafiono w żaden slot -> wróć na start
            transform.SetParent(startParent);
            rect.anchoredPosition = startPosition;
        }
        canvasGroup.blocksRaycasts = true;
    }
}
