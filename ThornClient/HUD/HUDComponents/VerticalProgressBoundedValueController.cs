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
public class VerticalProgressBoundedValueController : MonoBehaviour, IBoundedValueController {
    /// <summary>
    /// The BoundedValueHudModule that this controller works with. This must be set immediately after creation.
    /// </summary>
    public BoundedValueHudModule? TargetModule { get; set; }

    private TextMeshProUGUI? _textName;
    private RectTransform? _transTrough;
    private RectTransform? _transValue;
    private RectTransform? _transSoftMask;
    private RectTransform? _transSoftBound;
    private Image? _icon;
    private BatchBoolSettingVisibilitySyncer? _visibilitySyncer;

    private void Start() {
        if (TargetModule == null) return;
        _textName = gameObject.FindRecursive("Name")?.GetComponent<TextMeshProUGUI>();
        _icon = gameObject.FindRecursive("Trough/Icon")?.GetComponent<Image>();
        _transTrough = gameObject.FindRecursive("Trough")?.GetComponent<RectTransform>();
        _transValue = gameObject.FindRecursive("Trough/Value")?.GetComponent<RectTransform>();
        _transSoftMask = gameObject.FindRecursive("Trough/SoftBoundMask")?.GetComponent<RectTransform>();
        _transSoftBound = gameObject.FindRecursive("Trough/SoftBoundMask/SoftBound")?.GetComponent<RectTransform>();
        var valObj = gameObject.FindRecursive("Trough/Value");
        if (valObj != null) {
            valObj.AddComponent<ColorSettingSyncer>().TargetSetting = TargetModule.ValueColor;
        }

        var sofObj = gameObject.FindRecursive("Trough/SoftBoundMask/SoftBound");
        if (sofObj != null) {
            sofObj.AddComponent<ColorSettingSyncer>().TargetSetting = TargetModule.SoftBoundColor;
        }

        _visibilitySyncer = gameObject.GetOrAddComponent<BatchBoolSettingVisibilitySyncer>();
        _visibilitySyncer.SyncPairs = new Dictionary<Setting<bool>, string> {
            { TargetModule.ShowName, "Name" }
        };

        UpdateName();
        UpdateIcon();
        UpdateValue();
        gameObject.UnfuckLayoutHack();

        // Hook
        TargetModule.NameChanged += UpdateName;
        TargetModule.IconChanged += UpdateIcon;
        TargetModule.ValueChanged += UpdateValue;
        TargetModule.BoundChanged += UpdateValue;
        TargetModule.SoftBoundChanged += UpdateValue;
        TargetModule.DecimalPlacesChanged += UpdateValue;
        TargetModule.VerticalProgressLength.OnChanged += UpdateValue;
        TargetModule.VerticalProgressShowIcon.OnChanged += UpdateIcon;
    }

    private void OnDestroy() {
        if (TargetModule == null) return;
        TargetModule.NameChanged -= UpdateName;
        TargetModule.IconChanged -= UpdateIcon;
        TargetModule.ValueChanged -= UpdateValue;
        TargetModule.BoundChanged -= UpdateValue;
        TargetModule.SoftBoundChanged -= UpdateValue;
        TargetModule.DecimalPlacesChanged -= UpdateValue;
        TargetModule.VerticalProgressLength.OnChanged -= UpdateValue;
        TargetModule.VerticalProgressShowIcon.OnChanged -= UpdateIcon;
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
        if (_icon == null || TargetModule == null) return;
        // Plugin.Log.LogInfo($"Set icon to {value}");
        var actualValue = value;
        if (!TargetModule.VerticalProgressShowIcon.Value) actualValue = null;
        if (_icon.sprite == actualValue) return;
        _icon.sprite = actualValue;
        if (_icon.gameObject.activeSelf != (_icon.sprite != null)) _icon.gameObject.SetActive(_icon.sprite != null);
    }

    private void UpdateValue(float _) {
        UpdateValue();
    }

    private void UpdateValue(int _) {
        UpdateValue();
    }

    private void UpdateValue() {
        var normalizedValue = Math.Clamp((TargetModule?.Value ?? 0) / (TargetModule?.Bound ?? 1), 0f, 1f);
        var normalizedSoftBound = Math.Clamp( // normalized softbound segment width
            (TargetModule?.BoundReduction ?? 0) / (TargetModule?.Bound ?? 1), 0f, 1f
        );
        if (TargetModule == null || _transTrough == null || _transValue == null || _transSoftBound == null || _transSoftMask == null) return;
        _transTrough.sizeDelta = new Vector2(_transTrough.sizeDelta.x, TargetModule.VerticalProgressLength.Value);
        _transSoftMask.sizeDelta = new Vector2(_transSoftMask.sizeDelta.x, _transTrough.sizeDelta.y);
        var width = _transValue.sizeDelta.x;
        var height = _transValue.sizeDelta.y;
        var softHeight = _transSoftBound.sizeDelta.y;
        var availableHeight = _transTrough.sizeDelta.y;
        float currNormalized = height / availableHeight;
        float currSoftNormalized = softHeight / availableHeight;
        if (!Mathf.Approximately(currNormalized, normalizedValue)) {
            _transValue.sizeDelta = new Vector2(width, availableHeight * normalizedValue);
        }

        if (!Mathf.Approximately(currSoftNormalized, normalizedSoftBound)) {
            _transSoftBound.sizeDelta = new Vector2(width, availableHeight * normalizedSoftBound);
        }
    }
}
