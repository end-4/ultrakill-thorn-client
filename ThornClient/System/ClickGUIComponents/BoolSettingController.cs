using NukeLib.UI;
using ThornClient.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ThornClient.System.ClickGUIComponents;

public class BoolSettingController : MonoBehaviour {
    public Setting<bool>? TargetSetting;
    private GameObject? Checkmark;

    private void Start() {
        GetComponent<Button>().onClick.AddListener(ToggleSetting);
        if (TargetSetting == null) return;
        TargetSetting.OnValueChanged += UpdateCheckmark;
        UpdateCheckmark(TargetSetting.Value);
    }

    private void OnDestroy() {
        GetComponent<Button>().onClick.RemoveListener(ToggleSetting);
        TargetSetting.OnValueChanged -= UpdateCheckmark;
        if (TargetSetting == null) return;
    }

    private void ToggleSetting() {
        TargetSetting.Value = !TargetSetting.Value;
    }

    private void UpdateCheckmark(bool value) {
        if (Checkmark == null) Checkmark = gameObject.FindRecursive("Checkbox/Mark");
        if (Checkmark != null) Checkmark.SetActive(value);
    }
}
