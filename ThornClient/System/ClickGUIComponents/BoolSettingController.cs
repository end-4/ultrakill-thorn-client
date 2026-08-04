using NukeLib.UI;
using ThornClient.Core.ConfigurableElements;
using UnityEngine;
using UnityEngine.UI;

namespace ThornClient.System.ClickGUIComponents;

internal class BoolSettingController : MonoBehaviour {
    public Setting<bool>? TargetSetting;
    private GameObject? _checkMark;

    private void Start() {
        GetComponent<Button>().onClick.AddListener(ToggleSetting);
        if (TargetSetting == null) return;
        TargetSetting.OnValueChanged += UpdateCheckmark;
        UpdateCheckmark(TargetSetting.Value);
    }

    private void OnDestroy() {
        GetComponent<Button>().onClick.RemoveListener(ToggleSetting);
        if (TargetSetting == null) return;
        TargetSetting.OnValueChanged -= UpdateCheckmark;
    }

    private void ToggleSetting() {
        if (TargetSetting == null) return;
        TargetSetting.Value = !TargetSetting.Value;
    }

    private void UpdateCheckmark(bool value) {
        if (_checkMark == null) _checkMark = gameObject.FindRecursive("Checkbox/Mark");
        if (_checkMark != null) _checkMark.SetActive(value);
    }
}
