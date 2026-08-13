using NukeLib.UI;
using ThornClient.Core.ConfigurableElements;
using UnityEngine;
using UnityEngine.UI;

namespace ThornClient.System.ClickGUIComponents;

internal class SwitchBoolSettingController : MonoBehaviour {
    public Setting<bool>? TargetSetting;
    private GameObject? _off;
    private GameObject? _on;

    private void Start() {
        GetComponent<Button>().onClick.AddListener(ToggleSetting);
        if (TargetSetting == null) return;
        TargetSetting.OnValueChanged += UpdateDisplay;
        _on = gameObject.FindRecursive("Base/On");
        _off = gameObject.FindRecursive("Base/Off");
        UpdateDisplay(TargetSetting.Value);
    }

    private void OnDestroy() {
        GetComponent<Button>().onClick.RemoveListener(ToggleSetting);
        if (TargetSetting == null) return;
        TargetSetting.OnValueChanged -= UpdateDisplay;
    }

    private void ToggleSetting() {
        if (TargetSetting == null) return;
        TargetSetting.Value = !TargetSetting.Value;
    }

    private void UpdateDisplay(bool value) {
        if (_on != null) _on.SetActiveAnimated(value, 10 * Vector2.left);
        if (_off != null) _off.SetActiveAnimated(!value, 10 * Vector2.right);
    }
}
