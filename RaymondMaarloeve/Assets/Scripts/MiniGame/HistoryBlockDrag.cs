/**
 * @file HistoryBlockDrag.cs
 * @brief Enables drag-and-drop behavior for history text blocks.
 */

using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

[RequireComponent(typeof(CanvasGroup))]
/**
 * @class HistoryBlockDrag
 * @brief Implements dragging logic for history blocks in the UI.
 */
public class HistoryBlockDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private CanvasGroup canvasGroup;
    private RectTransform rect;
    private Transform originalParent;
    private Vector2 originalPosition;
    private Canvas rootCanvas;

    /**
     * @brief Unity Awake — initializes components.
     */
    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rect = GetComponent<RectTransform>();
        rootCanvas = GetComponentInParent<Canvas>();
    }

    /**
     * @brief Called when dragging starts.
     * @param eventData Data associated with the drag event.
     */
    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        originalPosition = rect.anchoredPosition;
        transform.SetParent(rootCanvas.transform);
        canvasGroup.blocksRaycasts = false;
    }

    /**
     * @brief Called during dragging; updates position.
     * @param eventData Data associated with the drag event.
     */
    public void OnDrag(PointerEventData eventData)
    {
        rect.anchoredPosition += eventData.delta / rootCanvas.scaleFactor;
    }

    /**
     * @brief Called when dragging ends; returns to original slot if not dropped on a valid target.
     * @param eventData Data associated with the end of the drag event.
     */
    public void OnEndDrag(PointerEventData eventData)
    {
        if (transform.parent == rootCanvas.transform)
        {
            transform.SetParent(originalParent);
            rect.anchoredPosition = originalPosition;
        }
        canvasGroup.blocksRaycasts = true;
    }
}
