using System;
using System.Collections.Generic;
using NukeLib.UI;
using NukeLib.Utils;
using ThornClient.Core.ConfigurableElements;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ThornClient.HUD.HUDComponents;

/// <summary>
/// The controller for a BoundedValueHudModule.
/// </summary>
public class ProgressBoundedValueController : MonoBehaviour, IBoundedValueController {
    /// <summary>
    /// The BoundedValueHudModule that this controller works with. This must be set immediately after creation.
    /// </summary>
    public BoundedValueHudModule? TargetModule { get; set; }

    private TextMeshProUGUI? _textName;
    private RectTransform? _transTrough;
    private RectTransform? _transValue;
    private RectTransform? _transChargeValue;
    private RectTransform? _transSoftBound;
    private RectTransform? _transValueLayout;
    private Image? _icon;
    private TextMeshProUGUI? _textValue;
    private TextMeshProUGUI? _textCap;
    private Image? _valueIcon;
    private TextMeshProUGUI? _valueTextValue;
    private TextMeshProUGUI? _valueTextCap;
    private BatchBoolSettingVisibilitySyncer? _visibilitySyncer;

    private void Start() {
        if (TargetModule == null) return;
        _textName = gameObject.FindRecursive("NameLayout/Name")?.GetComponent<TextMeshProUGUI>();
        var trough = gameObject.FindRecursive("Trough");
        _transTrough = trough?.GetComponent<RectTransform>();
        _transValue = trough?.FindRecursive("ValueMask/Value")?.GetComponent<RectTransform>();
        _transChargeValue = trough?.FindRecursive("ChargeValue")?.GetComponent<RectTransform>();
        _transSoftBound = trough?.FindRecursive("ValueMask/SoftBound")?.GetComponent<RectTransform>();
        _transValueLayout = trough?.FindRecursive("ValueMask/Value/ValueLayout")?.GetComponent<RectTransform>();
        _icon = trough?.FindRecursive("ValueLayout/Icon")?.GetComponent<Image>();
        _textValue = trough?.FindRecursive("ValueLayout/Value")?.GetComponent<TextMeshProUGUI>();
        _textCap = trough?.FindRecursive("ValueLayout/Cap")?.GetComponent<TextMeshProUGUI>();
        _valueIcon = trough?.FindRecursive("ValueMask/Value/ValueLayout/Icon")?.GetComponent<Image>();
        _valueTextValue = trough?.FindRecursive("ValueMask/Value/ValueLayout/Value")?.GetComponent<TextMeshProUGUI>();
        _valueTextCap = trough?.FindRecursive("ValueMask/Value/ValueLayout/Cap")?.GetComponent<TextMeshProUGUI>();
        if (_textName != null)
            _textName.GetOrAddComponent<EnhancedColorSettingSyncer>().TargetSetting = TargetModule.ForegroundColor;
        if (_icon != null)
            _icon.GetOrAddComponent<EnhancedColorSettingSyncer>().TargetSetting = TargetModule.ForegroundColor;
        if (_textValue != null)
            _textValue.GetOrAddComponent<EnhancedColorSettingSyncer>().TargetSetting = TargetModule.ForegroundColor;
        if (_textCap != null)
            _textCap.GetOrAddComponent<EnhancedColorSettingSyncer>().TargetSetting = TargetModule.ForegroundColor;
        if (_valueIcon != null)
            _valueIcon.GetOrAddComponent<EnhancedColorSettingSyncer>().TargetSetting = TargetModule.ValueForegroundColor;
        if (_valueTextValue != null)
            _valueTextValue.GetOrAddComponent<EnhancedColorSettingSyncer>().TargetSetting = TargetModule.ValueForegroundColor;
        if (_valueTextCap != null)
            _valueTextCap.GetOrAddComponent<EnhancedColorSettingSyncer>().TargetSetting = TargetModule.ValueForegroundColor;

        var valObj = gameObject.FindRecursive("Trough/ValueMask/Value");
        if (valObj != null)
            valObj.AddComponent<EnhancedColorSettingSyncer>().TargetSetting = TargetModule.ValueColor;

        var chargeObj = gameObject.FindRecursive("Trough/ChargeValue");
        if (chargeObj != null)
            chargeObj.AddComponent<EnhancedColorSettingSyncer>().TargetSetting = TargetModule.ChargeValueColor;

        var sofObj = gameObject.FindRecursive("Trough/ValueMask/SoftBound");
        if (sofObj != null)
            sofObj.AddComponent<EnhancedColorSettingSyncer>().TargetSetting = TargetModule.SoftBoundColor;

        _visibilitySyncer = gameObject.GetOrAddComponent<BatchBoolSettingVisibilitySyncer>();
        _visibilitySyncer.SyncPairs = new Dictionary<Setting<bool>, string> {
            { TargetModule.ShowName, "NameLayout" },
        };

        UpdateName();
        UpdateIcon();
        UpdateNumbersVisibility();
        UpdateValue();
        UpdateSoftBound();
        gameObject.UnfuckLayoutHack();

        // Hook
        TargetModule.NameChanged += UpdateName;
        TargetModule.IconChanged += UpdateIcon;
        TargetModule.ValueChanged += UpdateValue;
        TargetModule.ChargeValueChanged += UpdateValue;
        TargetModule.BoundChanged += UpdateValue;
        TargetModule.BoundChanged += UpdateSoftBound;
        TargetModule.SoftBoundChanged += UpdateValue;
        TargetModule.SoftBoundChanged += UpdateSoftBound;
        TargetModule.DecimalPlacesChanged += UpdateValue;
        TargetModule.DecimalPlacesChanged += UpdateSoftBound;
        TargetModule.ProgressLength.OnChanged += UpdateValue;
        TargetModule.ProgressShowIcon.OnChanged += UpdateIcon;
        TargetModule.ProgressShowNumbers.OnChanged += UpdateNumbersVisibility;
    }

    private void OnDestroy() {
        if (TargetModule == null) return;
        TargetModule.NameChanged -= UpdateName;
        TargetModule.IconChanged -= UpdateIcon;
        TargetModule.ValueChanged -= UpdateValue;
        TargetModule.ChargeValueChanged -= UpdateValue;
        TargetModule.BoundChanged -= UpdateValue;
        TargetModule.BoundChanged -= UpdateSoftBound;
        TargetModule.SoftBoundChanged -= UpdateValue;
        TargetModule.SoftBoundChanged -= UpdateSoftBound;
        TargetModule.DecimalPlacesChanged -= UpdateValue;
        TargetModule.DecimalPlacesChanged -= UpdateSoftBound;
        TargetModule.ProgressLength.OnChanged -= UpdateValue;
        TargetModule.ProgressShowIcon.OnChanged -= UpdateIcon;
        TargetModule.ProgressShowNumbers.OnChanged -= UpdateNumbersVisibility;
    }

    private void UpdateName() {
        UpdateName(TargetModule?.DisplayName ?? "");
    }

    private void UpdateName(string value) {
        _textName?.SetText(value);
        gameObject.UnfuckLayoutHack();
        ExecutionUtils.RunNextFrame(() => {
            if (gameObject != null) gameObject.UnfuckLayoutHack();
        });
    }

    private void UpdateIcon() {
        UpdateIcon(TargetModule?.DisplayIcon);
    }

    private void UpdateIcon(Sprite? value) {
        if (_icon == null || _valueIcon == null || TargetModule == null) return;
        // Plugin.Log.LogInfo($"Set icon to {value}");
        var actualValue = value;
        if (!TargetModule.ProgressShowIcon.Value) actualValue = null;

        if (_icon.sprite == actualValue) return;
        _icon.sprite = actualValue;
        _valueIcon.sprite = actualValue;

        if (_icon.gameObject.activeSelf != (_icon.sprite != null)) {
            _icon.gameObject.SetActive(_icon.sprite != null);
            _valueIcon.gameObject.SetActive(_valueIcon.sprite != null);
        }
    }

    private void UpdateValue(float _) {
        UpdateValue();
    }

    private void UpdateValue(int _) {
        UpdateValue();
    }

    private void UpdateValue() {
        var normalizedValue = Math.Clamp((TargetModule?.Value ?? 0) / (TargetModule?.Bound ?? 1), 0f, 1f);
        var normalizedChargeValue = Math.Clamp((TargetModule?.ChargeValue ?? 0) / (TargetModule?.Bound ?? 1), 0f, 1f);
        var normalizedSoftBound = Math.Clamp( // normalized softbound segment width
            (TargetModule?.BoundReduction ?? 0) / (TargetModule?.Bound ?? 1), 0f, 1f
        );
        if (TargetModule == null || _transTrough == null || _transValue == null || _transChargeValue == null ||
            _transSoftBound == null || _transValueLayout == null) return;
        _transTrough.sizeDelta = new Vector2(TargetModule.ProgressLength.Value, _transTrough.sizeDelta.y);
        _transValueLayout.sizeDelta = new Vector2(TargetModule.ProgressLength.Value, _transValueLayout.sizeDelta.y);
        var height = _transValue.sizeDelta.y;
        var width = _transValue.sizeDelta.x;
        var chargeWidth = _transChargeValue.sizeDelta.x;
        var softWidth = _transSoftBound.sizeDelta.x;
        var availableWidth = _transTrough.sizeDelta.x;
        float currNormalized = width / availableWidth;
        float currChargeNormalized = chargeWidth / availableWidth;
        float currSoftNormalized = softWidth / availableWidth;

        if (!Mathf.Approximately(currNormalized, normalizedValue)) {
            _transValue.sizeDelta = new Vector2(availableWidth * normalizedValue, height);
        }

        if (!Mathf.Approximately(currChargeNormalized, normalizedChargeValue)) {
            _transChargeValue.sizeDelta = new Vector2(availableWidth * normalizedChargeValue, height);
        }

        if (!Mathf.Approximately(currSoftNormalized, normalizedSoftBound)) {
            _transSoftBound.sizeDelta = new Vector2(availableWidth * normalizedSoftBound, height);
        }

        if (_textValue != null)
            _textValue.SetText($"{Math.Round(TargetModule?.Value ?? 0, TargetModule?.DecimalPlaces ?? 1)}");

        if (_valueTextValue != null)
            _valueTextValue.SetText($"{Math.Round(TargetModule?.Value ?? 0, TargetModule?.DecimalPlaces ?? 1)}");
    }

    private void UpdateSoftBound(int _) {
        UpdateSoftBound();
    }

    private void UpdateSoftBound(float _) {
        UpdateSoftBound();
    }

    private void UpdateSoftBound() {
        var value = (TargetModule?.Bound ?? 1) - (TargetModule?.BoundReduction ?? 0);
        _textCap?.SetText($"/{Math.Round(value, TargetModule?.DecimalPlaces ?? 1)}");
        _valueTextCap?.SetText($"/{Math.Round(value, TargetModule?.DecimalPlaces ?? 1)}");
    }

    private void UpdateNumbersVisibility() {
        if (TargetModule == null || _textValue == null || _textCap == null) return;
        bool visible = TargetModule.ProgressShowNumbers.Value;
        if (visible != _textValue.gameObject.activeSelf) _textValue.gameObject.SetActive(visible);
        if (visible != _textCap.gameObject.activeSelf) _textCap.gameObject.SetActive(visible);
    }
}
