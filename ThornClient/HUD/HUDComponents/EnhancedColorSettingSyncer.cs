using ThornClient.Core.ConfigurableElements;
using ThornClient.Core.DataTypes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ThornClient.HUD.HUDComponents;

/// <summary>
/// Component that syncs the same-GameObject Image or TextMeshProUGUI component color to that of a Thorn Setting.
/// Just set the TargetSetting property right after creation
/// </summary>
public class EnhancedColorSettingSyncer : MonoBehaviour {
    /// <summary>
    /// The setting to sync the color of the Image component to
    /// </summary>
    public Setting<EnhancedColor>? TargetSetting;
    private Image? _img;
    private TextMeshProUGUI? _text;
    private void Start() {
        if (TargetSetting == null) return;
        _img = gameObject.GetComponent<Image>();
        _text = gameObject.GetComponent<TextMeshProUGUI>();
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
        if (TargetSetting == null) return;
        if (_img != null) _img.color = TargetSetting.Value.GetCurrentColor();
        if (_text != null) _text.color = TargetSetting.Value.GetCurrentColor();
    }
}
