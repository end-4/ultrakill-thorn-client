using System;
using NukeLib.UI;
using ThornClient.Core;
using ThornClient.Core.ConfigurableElements;
using TMPro;
using UnityEngine;

namespace ThornClient.System.ClickGUIComponents;

/// <summary>
/// Controller for text/string setting fields.
/// </summary>
public class TextSettingController : MonoBehaviour {
    public Setting<string>? TargetSetting;
    private TMP_InputField? _inputField;
    private string _lastString = string.Empty;

    private void Start() {
        _inputField = gameObject.FindRecursive("Input")?.GetComponent<TMP_InputField>();
        if (_inputField == null) return;

        _inputField.GetOrAddComponent<InputFocusGrab>();
        _inputField.onSelect.AddListener(SavePrevValue);
        _inputField.onEndEdit.AddListener(SaveNewValue);
        if (TargetSetting != null) {
            TargetSetting.OnValueChanged += UpdateFieldValue;
            UpdateFieldValue(TargetSetting.Value);
        }
    }

    private void OnDestroy() {
        if (TargetSetting != null) TargetSetting.OnValueChanged -= UpdateFieldValue;
        if (_inputField == null) return;
        _inputField.onSelect.RemoveListener(SavePrevValue);
        _inputField.onEndEdit.RemoveListener(SaveNewValue);
    }

    private void UpdateFieldValue(string value) {
        if (_inputField == null) return;
        _inputField.text = value ?? string.Empty;
    }

    private void SavePrevValue(string str) {
        _lastString = str;
    }

    private void SaveNewValue(string str) {
        try {
            if (TargetSetting != null) TargetSetting.Value = str;
        } catch (Exception e) {
            Plugin.Log.LogWarning($"Failed to set text setting: {e}");
            if (_inputField != null) {
                _inputField.text = _lastString;
            }
        }
    }
}
