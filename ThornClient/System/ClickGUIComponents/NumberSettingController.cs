using System;
using NukeLib.UI;
using ThornClient.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ThornClient.System.ClickGUIComponents;

/// <summary>
/// Controller for number setting field. int and float only, please use IntSettingController or FloatSettingController
/// </summary>
/// <typeparam name="T">Number type</typeparam>
public class NumberSettingController<T> : MonoBehaviour {
    public Setting<T> TargetSetting;
    private TMP_InputField? _inputField;
    private string _lastString = string.Empty;

    private void Start() {
        _inputField = gameObject.FindRecursive("Input").GetComponent<TMP_InputField>();
        if (_inputField == null) return;
        _inputField.onSelect.AddListener(SavePrevValue);
        _inputField.onEndEdit.AddListener(SaveNewValue);
        TargetSetting.OnValueChanged += UpdateFieldValue;
        UpdateFieldValue(TargetSetting.Value);
    }

    private void OnDestroy() {
        TargetSetting.OnValueChanged -= UpdateFieldValue;
        if (_inputField == null) return;
        _inputField.onSelect.RemoveListener(SavePrevValue);
        _inputField.onEndEdit.RemoveListener(SaveNewValue);
    }

    private void UpdateFieldValue(T value) {
        if (_inputField == null) return;
        try {
            _inputField.text = value.ToString();
        } catch (Exception e) {
            Plugin.Log.LogWarning($"{typeof(T)} cannot be converted to string: {e}");
        }
    }

    private void SavePrevValue(string str) {
        _lastString = str;
    }

    private void SaveNewValue(string str) {
        try {
            if (double.TryParse(str, out var number)) {
                TargetSetting.Value = (T)Convert.ChangeType(number, typeof(T));
            }
        } catch {
            // Revert if invalid
            if (_inputField != null) {
                _inputField.text = _lastString;
            }
            UpdateFieldValue(TargetSetting.Value);
        }
    }
}
