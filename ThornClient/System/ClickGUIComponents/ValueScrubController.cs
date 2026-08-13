using System;
using ThornClient.Managers;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ThornClient.System.ClickGUIComponents;

/// <summary>
/// Controller to support value scrubbing
/// </summary>
public class ValueScrubController : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler,
    IPointerEnterHandler,
    IPointerExitHandler {
    public float scaleFactor = 0.1f;
    private Vector2 _hotspot = new Vector2(48, 48);
    private Vector2 _cumulatedDelta;

    private bool _dragging = false;
    private bool _hovering = false;

    /// <summary>
    /// The event emitted when dragged. This is the TOTAL value since drag starts
    /// </summary>
    public event Action<float> OnValueScrub;

    /// <summary>
    /// The event when dragging starts.
    /// </summary>
    public event Action OnScrubStart;

    public void OnDisable() {
        _hovering = false;
        _dragging = false;
        UpdateCursor();
    }

    public void OnDrag(PointerEventData eventData) {
        // TODO add a snackbar/toast to Notiffy then use it for vertical sensitivity adjustment, just like in Figma...
        _cumulatedDelta += eventData.delta * scaleFactor;
        OnValueScrub?.Invoke(_cumulatedDelta.x);
    }

    public void OnBeginDrag(PointerEventData eventData) {
        _cumulatedDelta = Vector2.zero;
        OnScrubStart?.Invoke();
        _dragging = true;
    }

    public void OnEndDrag(PointerEventData eventData) {
        _cumulatedDelta = Vector2.zero;
        OnScrubStart?.Invoke();
        _dragging = false;
        UpdateCursor();
    }

    public void OnPointerEnter(PointerEventData eventData) {
        _hovering = true;
        UpdateCursor();
    }

    public void OnPointerExit(PointerEventData eventData) {
        _hovering = false;
        UpdateCursor();
    }

    private void UpdateCursor() {
        if (_hovering || _dragging) {
            Cursor.SetCursor(AssetManager.Get<Texture2D>(ClickGUI.BundleKey, "move_horizontal_cursor"), _hotspot,
                CursorMode.Auto);
        } else {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }
    }
}
