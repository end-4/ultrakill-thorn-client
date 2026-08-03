using ThornClient.Core.ConfigurableElements;
using UnityEngine;
using UnityEngine.UI;

namespace ThornClient.HUD.HUDComponents;

/// <summary>
/// Component that syncs the same-GameObject Image component color to that of a setting
/// </summary>
public class ColorSettingSyncer : MonoBehaviour {
    public Setting<Color>? TargetSetting;
    private Image? _img;
    private void Start() {
        if (TargetSetting == null) return;
        _img = gameObject.GetComponent<Image>();
        TargetSetting.OnValueChanged += UpdateColor;
        UpdateColor(TargetSetting.Value);
    }

    private void OnDestroy() {
        if (TargetSetting == null) return;
        TargetSetting.OnValueChanged -= UpdateColor;
    }

    private void UpdateColor(Color color) {
        if (_img == null) return;
        _img.color = color;
    }
}
