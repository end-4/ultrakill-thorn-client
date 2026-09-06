using NukeLib.UI;
using ThornClient.Core.ConfigurableElements;
using ThornClient.Core.DataTypes;
using ThornClient.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ThornClient.System.ClickGUIComponents;

/// <summary>
/// Controller for setting item that handles EnhancedColor
/// </summary>
public class EnhancedColorSettingController : MonoBehaviour {
    /// <summary>
    /// The setting this controller handles
    /// </summary>
    public Setting<EnhancedColor>? TargetSetting;

    private Button? _btn;
    private GameObject? _panel;
    private Image? _preview;

    private void Start() {
        _btn = GetComponent<Button>();
        _preview = gameObject.FindRecursive("ValueRow/ColorBorder/ColorPreview")?.GetComponent<Image>();

        if (_btn != null) _btn.onClick.AddListener(OpenWindow);
        if (TargetSetting != null) {
            TargetSetting.OnChanged += UpdatePreview;
            TargetSetting.OnEffectiveValueChanged += UpdatePreview;
        }
        UpdatePreview();
    }

    private void OnDestroy() {
        if (_btn != null) _btn.onClick.RemoveListener(OpenWindow);
        if (TargetSetting != null) {
            TargetSetting.OnChanged -= UpdatePreview;
            TargetSetting.OnEffectiveValueChanged -= UpdatePreview;
        }
    }

    private void UpdatePreview() {
        if (_preview == null || TargetSetting == null) return;
        _preview.color = TargetSetting.Value.GetCurrentColor();
    }

    private void OpenWindow() {
        if (_panel != null) return;
        _panel = CreateSelectionPanel(TargetSetting);
        if (_panel == null) return;
        ClickGUI.SpawnContent(_panel);
        ClickGUI.SurrenderTooltipText(TargetSetting?.Description ?? "");
    }

    private GameObject? CreateSelectionPanel(Setting<EnhancedColor>? setting) {
        var prefab = AssetManager.Get<GameObject>(ClickGUI.BundleKey, "EnhancedColorWindow");
        if (prefab == null || setting == null) return null;

        var obj = Instantiate(prefab);
        if (obj == null) return null;

        var ctl = obj.GetOrAddComponent<EnhancedColorWindowController>();
        ctl.TargetSetting = setting;

        return obj;
    }
}
