using System;
using System.Collections.Generic;
using NukeLib.UI;
using ThornClient.Core.ConfigurableElements;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ThornClient.System.ClickGUIComponents;

public class RadioChoiceEnumSettingController : MonoBehaviour {
    public Setting? TargetSetting;
    public object? TargetValue;
    private GameObject? _check;
    private TextMeshProUGUI? _name;
    private Button? _btn;

    private void Start() {
        _btn = GetComponent<Button>();
        _check = gameObject.FindRecursive("Radio/Check");
        _name = gameObject.FindRecursive("Name")?.GetComponent<TextMeshProUGUI>();

        // TODO HOOKS
        if (TargetSetting != null) TargetSetting.OnChanged += UpdateSelected;
        if (_btn != null) _btn.onClick.AddListener(SelectThis);

        UpdateName();
        UpdateSelected();
    }

    private void OnDestroy() {
        if (TargetSetting != null) TargetSetting.OnChanged -= UpdateSelected;
        if (_btn != null) _btn.onClick.RemoveListener(SelectThis);
    }

    private void UpdateName() {
        if (TargetValue == null) return;
        if (_name != null) {
            var str = TargetValue.ToString();
            var displayText = TargetSetting?.Hints?.EnumSubstitutions.GetValueOrDefault(str, str);
            _name.SetText(displayText);
        }
    }

    private void UpdateSelected() {
        if (TargetValue == null || TargetSetting == null || _check == null) return;
        var selected = TargetValue.Equals(TargetSetting.GetValue());
        _check.SetActiveAnimated(selected, Vector2.zero);
    }

    private void SelectThis() {
        if (TargetSetting == null || TargetValue == null) return;
        try {
            TargetSetting.SetValue(TargetValue);
        } catch (Exception e) {
            Plugin.Log.LogError($"[RadioChoiceEnumSettingController] Could not set {TargetSetting.Name} to {TargetValue.ToString()}");
        }
    }
}
