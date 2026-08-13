using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ThornClient.System.ClickGUIComponents;

/// <summary>
/// Component for free draggables
/// </summary>
internal class FreeMoveDragHandler : MonoBehaviour, IDragHandler {
    /// <summary>
    /// The transform of the GameObject that should be moved when dragging the holder of this component
    /// </summary>
    public RectTransform? target;
    protected Canvas? _canvas;

    private void Awake() {
        // If no target is explicitly set, default to moving this object
        try {
            if (target == null) target = (RectTransform)transform;
            _canvas = target.GetComponentInParent<Canvas>();
        } catch (Exception e) {
            Plugin.Log.LogWarning("[FreeMoveDragHandler] may have been added to a root element");
        }
    }

    public virtual void OnDrag(PointerEventData eventData) {
        if (target == null) return;
        float scaleFactor = _canvas != null ? _canvas.scaleFactor : 1f;
        Vector2 canvasDelta = eventData.delta / scaleFactor;
        target.anchoredPosition += canvasDelta;
    }
}
