using ThornClient.Core;
using UnityEngine;

namespace ThornClient.HUD.HUDComponents;

/// <summary>
/// Controller for HUD elements. Handles positioning including the coordinates and pivot.
/// </summary>
public class HudElementController : MonoBehaviour {
    /// <summary>
    /// The HUD module that this controller is associated with.
    /// </summary>
    public HudModule? hudModule;

    private void Start() {
        if (hudModule == null) return;
        var dragComp = gameObject.GetOrAddComponent<HudElementPositioningController>();
        dragComp.TargetModule = hudModule;

        hudModule.PositionX.OnValueChanged += UpdateX;
        hudModule.PositionY.OnValueChanged += UpdateY;
        hudModule.PivotX.OnValueChanged += UpdatePivotX;
        hudModule.PivotY.OnValueChanged += UpdatePivotY;
        hudModule.Surface.OnValueChanged += hudModule.Reparent;
        UpdateX(hudModule.PositionX.Value);
        UpdateY(hudModule.PositionY.Value);
        UpdatePivotX(hudModule.PivotX.Value);
        UpdatePivotY(hudModule.PivotY.Value);
        hudModule.Reparent(hudModule.Surface.Value);
    }

    private void OnDestroy() {
        if (hudModule == null) return;

        hudModule.PositionX.OnValueChanged -= UpdateX;
        hudModule.PositionY.OnValueChanged -= UpdateY;
        hudModule.PivotX.OnValueChanged -= UpdatePivotX;
        hudModule.PivotY.OnValueChanged -= UpdatePivotY;
        hudModule.Surface.OnValueChanged -= hudModule.Reparent;
    }

    private void UpdateX(float value) {
        var localPos = transform.localPosition;
        transform.localPosition = new Vector3(value, localPos.y, localPos.z);
    }

    private void UpdateY(float value) {
        var localPos = transform.localPosition;
        transform.localPosition = new Vector3(localPos.x, value, localPos.z);
    }

    private void UpdatePivotX(float value) {
        if (transform is RectTransform rectTransform) {
            var pivot = rectTransform.pivot;
            rectTransform.pivot = new Vector2(value, pivot.y);
        }
    }

    private void UpdatePivotY(float value) {
        if (transform is RectTransform rectTransform) {
            var pivot = rectTransform.pivot;
            rectTransform.pivot = new Vector2(pivot.x, value);
        }
    }
}
