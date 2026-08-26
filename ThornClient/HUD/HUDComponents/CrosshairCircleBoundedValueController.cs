using System;
using System.Collections.Generic;
using NukeLib.UI;
using ThornClient.Core.ConfigurableElements;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ThornClient.HUD.HUDComponents;

internal class CrosshairCircleBoundedValueController : MonoBehaviour, IBoundedValueController {
    public BoundedValueHudModule? TargetModule { get; set; }

    private Image? _fillValue;
    private Image? _fillSoftBound;
    private RectTransform? _transTrough;
    private RectTransform? _transSoftBound;

    private void Start() {
        if (TargetModule == null) return;
        _fillValue = gameObject.FindRecursive("Trough/Value")?.GetComponent<Image>();
        _fillSoftBound = gameObject.FindRecursive("Trough/SoftBound")?.GetComponent<Image>();
        _transTrough = gameObject.FindRecursive("Trough")?.GetComponent<RectTransform>();
        _transSoftBound = gameObject.FindRecursive("Trough/SoftBound")?.GetComponent<RectTransform>();
        var valObj = gameObject.FindRecursive("Trough/Value");
        if (valObj != null) {
            valObj.GetOrAddComponent<ColorSettingSyncer>().TargetSetting = TargetModule.ValueColor;
        }

        var sofObj = gameObject.FindRecursive("Trough/SoftBound");
        if (sofObj != null) {
            sofObj.GetOrAddComponent<ColorSettingSyncer>().TargetSetting = TargetModule.SoftBoundColor;
        }

        UpdateContainer();
        gameObject.UnfuckLayoutHack();

        // Hook.
        TargetModule.ValueChanged += UpdateValue;
        TargetModule.BoundChanged += UpdateValue;
        TargetModule.SoftBoundChanged += UpdateValue;
        TargetModule.DecimalPlacesChanged += UpdateValue;
        TargetModule.CrosshairCircleAngleFillPercentage.OnChanged += UpdateContainer;
        TargetModule.CrosshairCircleStartAnglePercentage.OnChanged += UpdateContainer;
        // TargetModule.CrosshairCircleDiameter.OnChanged += UpdateContainer;
    }

    private void OnDestroy() {
        if (TargetModule == null) return;
        TargetModule.ValueChanged -= UpdateValue;
        TargetModule.BoundChanged -= UpdateValue;
        TargetModule.SoftBoundChanged -= UpdateValue;
        TargetModule.DecimalPlacesChanged -= UpdateValue;
        TargetModule.CrosshairCircleAngleFillPercentage.OnChanged -= UpdateContainer;
        TargetModule.CrosshairCircleStartAnglePercentage.OnChanged -= UpdateContainer;
        // TargetModule.CrosshairCircleDiameter.OnChanged -= UpdateContainer;
    }

    private void UpdateValue(float _) {
        UpdateValue();
    }

    private void UpdateValue(int _) {
        UpdateValue();
    }

    private void UpdateContainer() {
        // From where do we circle
        if (TargetModule == null || _transTrough == null || _transSoftBound == null) return;
        var rotAngle = TargetModule.CrosshairCircleStartAnglePercentage.Value * 360;
        _transTrough.localEulerAngles = new Vector3(0, 0, -rotAngle);

        // How much of the circle do we circle
        var sweep = TargetModule.CrosshairCircleAngleFillPercentage.Value * 360;
        _transSoftBound.localEulerAngles = new Vector3(0, 0, -sweep);

        // How big the circle is
        // Doesn't work as I expected... maybe use some canvas drawing? if that's a thing
        // var diameter = TargetModule.CrosshairCircleDiameter.Value;
        // _transTrough.sizeDelta = new Vector2(diameter, diameter);
        UpdateValue();
    }

    private void UpdateValue() {
        var normalizedValue = Math.Clamp((TargetModule?.Value ?? 0) / (TargetModule?.Bound ?? 1), 0f, 1f);
        var normalizedSoftBound = Math.Clamp( // normalized softbound segment width
            (TargetModule?.BoundReduction ?? 0) / (TargetModule?.Bound ?? 1), 0f, 1f
        );
        var multiplier = Mathf.Clamp01(TargetModule?.CrosshairCircleAngleFillPercentage.Value ?? 1);
        if (_fillValue == null || _fillSoftBound == null) return;

        var valueFillAmount = normalizedValue * multiplier;
        var softBoundFillAmount = normalizedSoftBound * multiplier;

        if (!Mathf.Approximately(_fillValue.fillAmount, valueFillAmount))
            _fillValue.fillAmount = valueFillAmount;
        if (!Mathf.Approximately(_fillSoftBound.fillAmount, softBoundFillAmount))
            _fillSoftBound.fillAmount = softBoundFillAmount;
    }
}
