using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

[RequireComponent(typeof(CanvasGroup))]
public class HistoryBlockDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    /// <summary>
    /// CanvasGroup used to enable or disable raycast blocking during drag.
    /// </summary>
    private CanvasGroup canvasGroup;

    /// <summary>
    /// RectTransform of this history block, used to adjust its anchored position.
    /// </summary>
    private RectTransform rect;

    /// <summary>
    /// The original parent Transform before dragging begins.
    /// </summary>
    private Transform originalParent;

    /// <summary>
    /// The original anchored position before dragging begins.
    /// </summary>
    private Vector2 originalPosition;

    /// <summary>
    /// The top‐level Canvas in the scene, used to temporarily reparent the block during dragging.
    /// </summary>
    private Canvas rootCanvas;

    /// <summary>
    /// Called when the script instance is being loaded.
    /// Caches references to required components and finds the root canvas.
    /// </summary>
    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rect = GetComponent<RectTransform>();
        // Find the Canvas at the top of this block's hierarchy
        rootCanvas = GetComponentInParent<Canvas>();
    }

    /// <summary>
    /// Called by the EventSystem when a drag operation starts on this block.
    /// Records the original parent and position, re‐parents to the root canvas,
    /// and disables raycast blocking so drop targets can receive events.
    /// </summary>
    /// <param name="eventData">
    /// Pointer event data containing information about the drag.
    /// </param>
    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        originalPosition = rect.anchoredPosition;
        // Move to top‐level canvas so it renders above other UI
        transform.SetParent(rootCanvas.transform);
        // Allow other UI elements to receive raycasts while dragging
        canvasGroup.blocksRaycasts = false;
    }

    /// <summary>
    /// Called by the EventSystem on each drag update.
    /// Moves the block by the pointer delta, accounting for canvas scale.
    /// </summary>
    /// <param name="eventData">
    /// Pointer event data containing the delta movement.
    /// </param>
    public void OnDrag(PointerEventData eventData)
    {
        // Adjust movement for canvas scaling
        rect.anchoredPosition += eventData.delta / rootCanvas.scaleFactor;
    }

    /// <summary>
    /// Called by the EventSystem when the drag operation ends.
    /// If the block hasn't been reparented by a drop handler, it returns to its original parent
    /// and position. Raycast blocking is re‐enabled.
    /// </summary>
    /// <param name="eventData">
    /// Pointer event data for the end of the drag.
    /// </param>
    public void OnEndDrag(PointerEventData eventData)
    {
        // If still on root canvas, revert to original parent and position
        if (transform.parent == rootCanvas.transform)
        {
            transform.SetParent(originalParent);
            rect.anchoredPosition = originalPosition;
        }
        // Re‐enable raycasts so the block can be dragged again
        canvasGroup.blocksRaycasts = true;
    }
}
