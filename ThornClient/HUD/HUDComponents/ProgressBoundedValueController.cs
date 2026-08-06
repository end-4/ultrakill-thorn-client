using System;
using System.Collections.Generic;
using NukeLib.UI;
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
    private RectTransform? _transSoftBound;
    private Image? _icon;
    private TextMeshProUGUI? _textValue;
    private TextMeshProUGUI? _textCap;
    private BatchBoolSettingVisibilitySyncer? _visibilitySyncer;

    private void Start() {
        if (TargetModule == null) return;
        _textName = gameObject.FindRecursive("NameLayout/Name")?.GetComponent<TextMeshProUGUI>();
        _icon = gameObject.FindRecursive("Trough/ValueLayout/Icon")?.GetComponent<Image>();
        _transTrough = gameObject.FindRecursive("Trough")?.GetComponent<RectTransform>();
        _transValue = gameObject.FindRecursive("Trough/Value")?.GetComponent<RectTransform>();
        _transSoftBound = gameObject.FindRecursive("Trough/SoftBound")?.GetComponent<RectTransform>();
        _textValue = gameObject.FindRecursive("Trough/ValueLayout/Value")?.GetComponent<TextMeshProUGUI>();
        _textCap = gameObject.FindRecursive("Trough/ValueLayout/Cap")?.GetComponent<TextMeshProUGUI>();
        var valObj = gameObject.FindRecursive("Trough/Value");
        if (valObj != null) {
            valObj.AddComponent<ColorSettingSyncer>().TargetSetting = TargetModule.ValueColor;
        }

        var sofObj = gameObject.FindRecursive("Trough/SoftBound");
        if (sofObj != null) {
            sofObj.AddComponent<ColorSettingSyncer>().TargetSetting = TargetModule.SoftBoundColor;
        }

        _visibilitySyncer = gameObject.GetOrAddComponent<BatchBoolSettingVisibilitySyncer>();
        _visibilitySyncer.SyncPairs = new Dictionary<Setting<bool>, string> {
            { TargetModule.ShowName, "NameLayout" }
        };

        UpdateName();
        UpdateIcon();
        UpdateValue();
        UpdateSoftBound();
        gameObject.UnfuckLayoutHack();

        // Hook
        TargetModule.NameChanged += UpdateName;
        TargetModule.IconChanged += UpdateIcon;
        TargetModule.ValueChanged += UpdateValue;
        TargetModule.BoundChanged += UpdateValue;
        TargetModule.BoundChanged += UpdateSoftBound;
        TargetModule.SoftBoundChanged += UpdateValue;
        TargetModule.SoftBoundChanged += UpdateSoftBound;
        TargetModule.DecimalPlacesChanged += UpdateValue;
        TargetModule.DecimalPlacesChanged += UpdateSoftBound;
    }

    private void OnDestroy() {
        if (TargetModule == null) return;
        TargetModule.NameChanged -= UpdateName;
        TargetModule.IconChanged -= UpdateIcon;
        TargetModule.ValueChanged -= UpdateValue;
        TargetModule.BoundChanged -= UpdateValue;
        TargetModule.BoundChanged -= UpdateSoftBound;
        TargetModule.SoftBoundChanged -= UpdateValue;
        TargetModule.SoftBoundChanged -= UpdateSoftBound;
        TargetModule.DecimalPlacesChanged -= UpdateValue;
        TargetModule.DecimalPlacesChanged -= UpdateSoftBound;
    }

    private void UpdateName() {
        UpdateName(TargetModule?.DisplayName ?? "");
    }

    private void UpdateName(string value) {
        _textName?.SetText(value);
        gameObject.UnfuckLayoutHack();
    }

    private void UpdateIcon() {
        UpdateIcon(TargetModule?.DisplayIcon);
    }

    private void UpdateIcon(Sprite? value) {
        if (_icon == null || _icon.sprite == value) return;
        // Plugin.Log.LogInfo($"Set icon to {value}");
        _icon.sprite = value;
        if (_icon.gameObject.activeSelf != (value == null)) _icon.gameObject.SetActive(value != null);
    }

    private void UpdateValue(float _) {
        UpdateValue();
    }

    private void UpdateValue(int _) {
        UpdateValue();
    }

    private void UpdateValue() {
        var normalizedValue = (TargetModule?.Value ?? 0) / (TargetModule?.Bound ?? 1);
        var normalizedSoftBound =
            (TargetModule?.BoundReduction ?? 0) / (TargetModule?.Bound ?? 1); // normalized softbound segment width
        if (_transTrough == null || _transValue == null || _transSoftBound == null) return;
        var height = _transValue.sizeDelta.y;
        var width = _transValue.sizeDelta.x;
        var softWidth = _transSoftBound.sizeDelta.x;
        var availableWidth = _transTrough.sizeDelta.x;
        float currNormalized = width / availableWidth;
        float currSoftNormalized = softWidth / availableWidth;
        if (!Mathf.Approximately(currNormalized, normalizedValue)) {
            _transValue.sizeDelta = new Vector2(availableWidth * normalizedValue, height);
        }

        if (!Mathf.Approximately(currSoftNormalized, normalizedSoftBound)) {
            _transSoftBound.sizeDelta = new Vector2(availableWidth * normalizedSoftBound, height);
        }

        _textValue?.SetText($"{Math.Round(TargetModule?.Value ?? 0, TargetModule?.DecimalPlaces ?? 1)}");
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
    }
}
