using ThornClient.Core;
using UnityEngine;

namespace ThornClient.HUD.HUDComponents;

/// <summary>
/// Note that most processing is still in the HudModule.
/// We use a MonoBehavior here to make use of the active/inactive state for easier unhooking
/// </summary>
public class HudElementController : MonoBehaviour {
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
