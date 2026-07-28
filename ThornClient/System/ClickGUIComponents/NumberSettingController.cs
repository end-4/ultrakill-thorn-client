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
public abstract class NumberSettingController<T> : MonoBehaviour {
    public Setting<T>? TargetSetting;
    private TMP_InputField? _inputField;
    private ValueScrubController _scrubController;
    private Slider? _slider;
    private string _lastString = string.Empty;
    private int _decimals = 1;

    private void Start() {
        _inputField = gameObject.FindRecursive("MainField/Input")?.GetComponent<TMP_InputField>();
        if (_inputField == null) return;
        _inputField.GetOrAddComponent<InputFocusGrab>();
        _inputField.onSelect.AddListener(SavePrevValue);
        _inputField.onEndEdit.AddListener(SaveNewValue);
        if (TargetSetting != null) {
            TargetSetting.OnValueChanged += UpdateFieldValue;
            UpdateFieldValue(TargetSetting.Value);
            if (TargetSetting.Hints != null && TargetSetting.Hints.Range != null) {
                if (TargetSetting.Hints.Decimals != null) _decimals = TargetSetting.Hints.Decimals.Value;
                var range = TargetSetting.Hints.Range;
                var sobj = gameObject.FindRecursive("Slider");
                if (sobj != null) {
                    sobj.SetActive(true);
                    _slider = sobj.GetComponent<Slider>();
                    _slider.minValue = Math.Min(range.Item1, range.Item2);
                    _slider.maxValue = Math.Max(range.Item1, range.Item2);
                    _slider.onValueChanged.AddListener(x => { SaveNewValue(Math.Round(x, _decimals).ToString()); });
                }
            }
        }

        _scrubController = gameObject.GetOrAddComponent<ValueScrubController>();
        _scrubController.OnScrubStart += SavePrevValue;
        _scrubController.OnValueScrub += UpdateScrub;
    }

    private void OnDestroy() {
        if (TargetSetting != null) TargetSetting.OnValueChanged -= UpdateFieldValue;
        if (_inputField == null) return;
        _inputField.onSelect.RemoveListener(SavePrevValue);
        _inputField.onEndEdit.RemoveListener(SaveNewValue);
        _scrubController.OnScrubStart -= SavePrevValue;
        _scrubController.OnValueScrub -= UpdateScrub;
    }

    private T _beforeScrub;

    private void SavePrevValue() {
        _beforeScrub = TargetSetting.Value;
    }

    protected abstract void UpdateScrub(T baseValue, float valueDiff);

    private void UpdateScrub(float valueDiff) {
        UpdateScrub(_beforeScrub, valueDiff);
    }

    private void UpdateFieldValue(T value) {
        if (_inputField == null) return;
        try {
            _inputField.text = value.ToString();
            if (_slider != null && float.TryParse(value.ToString(), out var floatVal)) _slider.value = floatVal;
        } catch (Exception e) {
            Plugin.Log.LogWarning($"{typeof(T)} cannot be converted to string: {e}");
        }
    }

    private void SavePrevValue(string str) {
        _lastString = str;
    }

    private void SaveNewValue(string str) {
        if (TargetSetting == null) return;
        if (double.TryParse(str, out var number)) {
            TargetSetting.Value = (T)Convert.ChangeType(number, typeof(T));
        } else {
            UpdateFieldValue(TargetSetting.Value);
        }
    }
}
