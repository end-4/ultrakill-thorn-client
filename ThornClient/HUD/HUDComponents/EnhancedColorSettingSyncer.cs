using ThornClient.Core.ConfigurableElements;
using ThornClient.Core.DataTypes;
using UnityEngine;
using UnityEngine.UI;

namespace ThornClient.HUD.HUDComponents;

/// <summary>
/// Component that syncs the same-GameObject Image component color to that of a Thorn Setting.
/// Just set the TargetSetting property right after creation
/// </summary>
public class EnhancedColorSettingSyncer : MonoBehaviour {
    /// <summary>
    /// The setting to sync the color of the Image component to
    /// </summary>
    public Setting<EnhancedColor>? TargetSetting;
    private Image? _img;
    private void Start() {
        if (TargetSetting == null) return;
        _img = gameObject.GetComponent<Image>();
        TargetSetting.OnChanged += UpdateColor;
        TargetSetting.OnEffectiveValueChanged += UpdateColor;
        UpdateColor();
    }

    private void OnDestroy() {
        if (TargetSetting == null) return;
        TargetSetting.OnChanged -= UpdateColor;
        TargetSetting.OnEffectiveValueChanged -= UpdateColor;
    }

    private void UpdateColor() {
        if (_img == null || TargetSetting == null) return;
        _img.color = TargetSetting.Value.GetCurrentColor();
    }
}
