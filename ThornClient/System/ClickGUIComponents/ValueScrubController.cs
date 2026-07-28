using System;
using ThornClient.Core;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ThornClient.System.ClickGUIComponents;

public class ValueScrubController : MonoBehaviour, IDragHandler, IBeginDragHandler {
    public float scaleFactor = 0.1f;

    private Vector2 _cumulatedDelta;

    /// <summary>
    /// The event emitted when dragged. This is the TOTAL value since drag starts
    /// </summary>
    public event Action<float> OnValueScrub;

    /// <summary>
    /// The event when dragging starts.
    /// </summary>
    public event Action OnScrubStart;

    public void OnDrag(PointerEventData eventData) {
        _cumulatedDelta += eventData.delta * scaleFactor;
        OnValueScrub?.Invoke(_cumulatedDelta.x);
    }

    public void OnBeginDrag(PointerEventData eventData) {
        _cumulatedDelta = Vector2.zero;
        OnScrubStart?.Invoke();
    }
}
