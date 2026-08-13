using System.Collections.Generic;
using NukeLib.UI;
using ThornClient.Core.ConfigurableElements;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ThornClient.HUD.HUDComponents;

internal class CircularBoundedValueController : MonoBehaviour, IBoundedValueController {
    public BoundedValueHudModule? TargetModule { get; set; }

    private TextMeshProUGUI? _textName;
    private Image? _fillValue;
    private Image? _fillSoftBound;
    private Image? _icon;
    private BatchBoolSettingVisibilitySyncer? _visibilitySyncer;

    private void Start() {
        if (TargetModule == null) return;
        _textName = gameObject.FindRecursive("Name")?.GetComponent<TextMeshProUGUI>();
        _icon = gameObject.FindRecursive("Trough/Icon")?.GetComponent<Image>();
        _fillValue = gameObject.FindRecursive("Trough/Value")?.GetComponent<Image>();
        _fillSoftBound = gameObject.FindRecursive("Trough/SoftBound")?.GetComponent<Image>();
        var valObj = gameObject.FindRecursive("Trough/Value/ValueBase");
        if (valObj != null) {
            valObj.GetOrAddComponent<ColorSettingSyncer>().TargetSetting = TargetModule.ValueColor;
        }
        var sofObj = gameObject.FindRecursive("Trough/SoftBound/SoftBoundBase");
        if (sofObj != null) {
            sofObj.GetOrAddComponent<ColorSettingSyncer>().TargetSetting = TargetModule.SoftBoundColor;
        }
        _visibilitySyncer = gameObject.GetOrAddComponent<BatchBoolSettingVisibilitySyncer>();
        _visibilitySyncer.SyncPairs = new Dictionary<Setting<bool>, string> {
            {TargetModule.ShowName, "Name"}
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
    }

    private void OnDestroy() {
        if (TargetModule == null) return;
        TargetModule.NameChanged -= UpdateName;
        TargetModule.IconChanged -= UpdateIcon;
        TargetModule.ValueChanged -= UpdateValue;
        TargetModule.BoundChanged -= UpdateValue;
        TargetModule.SoftBoundChanged -= UpdateValue;
        TargetModule.DecimalPlacesChanged -= UpdateValue;
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
        var normalizedSoftBound = (TargetModule?.BoundReduction ?? 0) / (TargetModule?.Bound ?? 1); // normalized softbound segment width

        if (_fillValue == null || _fillSoftBound == null) return;
        if (!Mathf.Approximately(_fillValue.fillAmount, normalizedValue)) {
            _fillValue.fillAmount = normalizedValue;
        }

        if (!Mathf.Approximately(_fillSoftBound.fillAmount, normalizedSoftBound)) {
            _fillSoftBound.fillAmount = normalizedSoftBound;
        }
    }
}
