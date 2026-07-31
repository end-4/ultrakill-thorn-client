using System;
using System.Collections.Generic;
using NukeLib.UI;
using ThornClient.Core;
using ThornClient.Core.ConfigurableElements;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ThornClient.System.ClickGUIComponents;

/// <summary>
/// For enums
/// I might have an alternate Material 3-style button group for selection later, so I'm naming this one "dropdown"
/// Keyword for pattern-mining code readers: EnumSettingController
/// </summary>
public class DropdownSettingController : MonoBehaviour {
    public Setting? TargetSetting { get; set; }
    private TMP_Dropdown? _dropdown;
    private Type? _enumType;

    private void Start() {
        if (TargetSetting == null) return;
        var currentValue = TargetSetting.GetValue();
        _enumType = currentValue.GetType();

        if (!_enumType.IsEnum) {
            Plugin.Log.LogError($"[EnumSettingController] Setting {TargetSetting.Name} is not an enum!");
            return;
        }

        _dropdown = gameObject.FindRecursive("Dropdown").GetComponent<TMP_Dropdown>();
        if (_dropdown != null) {
            var enumNames = Enum.GetNames(_enumType);
            _dropdown.ClearOptions();
            _dropdown.AddOptions([..enumNames]);
            _dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
        }
        UpdateDisplay();
        TargetSetting.OnChanged += UpdateDisplay;
    }

    private void UpdateDisplay() {
        if (TargetSetting == null || _dropdown == null || _enumType == null) return;

        var currentValue = TargetSetting.GetValue();
        var enumNames = Enum.GetNames(_enumType);
        int currentIndex = Array.IndexOf(enumNames, currentValue.ToString());

        if (currentIndex < 0) return;

        _dropdown.value = currentIndex;
        _dropdown.RefreshShownValue();
    }

    private void OnDropdownValueChanged(int index) {
        string selectedName = _dropdown!.options[index].text;
        object enumValue = Enum.Parse(_enumType, selectedName);
        TargetSetting?.SetValue(enumValue);
    }

    private void OnDestroy() {
        if (TargetSetting != null) TargetSetting.OnChanged -= UpdateDisplay;
        if (_dropdown != null) {
            _dropdown.onValueChanged.RemoveListener(OnDropdownValueChanged);
        }
    }
}
