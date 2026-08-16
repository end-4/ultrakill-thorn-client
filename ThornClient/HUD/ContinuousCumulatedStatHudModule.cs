using System;
using ThornClient.Core.ConfigurableElements;
using UnityEngine;

namespace ThornClient.HUD;

/// <summary>
/// Module that shows a value collected continuously
/// </summary>
public abstract class ContinuousCumulatedStatHudModule : TextHudModule {
    public enum DataSmoothing {
        None,
        ExpoWeightedMovingAverage,
    }

    public SettingGroup MeasurementGroup;

    /// <summary>
    /// Setting: whether to show the icon on the HUD element
    /// </summary>
    public Setting<bool> ShowIcon;

    /// <summary>
    /// Setting: how fast to update the text (in seconds)
    /// </summary>
    public Setting<float> UpdateInterval;

    /// <summary>
    /// How to smooth the data
    /// </summary>
    public Setting<DataSmoothing> ValueSmoothing;

    /// <summary>
    /// Setting: smoothing factor (alpha) for EWMA. Higher values give more weight to recent data.
    /// </summary>
    public Setting<float> EwmaAlpha;

    private float _accumulatedTime = 0f;
    private float _collected = 0f;
    private float? _smoothedValue = null;

    /// <summary>
    /// Constructor
    /// </summary>
    public ContinuousCumulatedStatHudModule(string guid, string name, string description,
        DataSmoothing smoothing = DataSmoothing.None, float updateInterval = 0.5f, float ewmaAlpha = 0.5f) : base(guid, name, description) {
        ShowIcon = CreateSetting("showIcon", "Show icon", "Shows an icon next to the text (if available)", true);
        MeasurementGroup = CreateGroup("measurementGroup", "Measurement", "How to measure the data");
        UpdateInterval = CreateSetting("updateInterval", "Update interval", "How fast to update the text", updateInterval, MeasurementGroup);
        ValueSmoothing = CreateSetting("valueSmoothing", "Value smoothing", "Whether to smooth out the value", smoothing, MeasurementGroup);
        EwmaAlpha = CreateSetting("ewmaAlpha", "EWMA Alpha", "Smoothing coefficient for EWMA (0.01 to 1.0)", ewmaAlpha, MeasurementGroup);

        UpdateDisplayIcon();
        ShowIcon.OnChanged += UpdateDisplayIcon;
    }

    /// <summary>
    /// This is sealed! You should override CollectUpdate.
    /// </summary>
    public sealed override void OnUpdate() {
        _accumulatedTime += Time.unscaledDeltaTime;
        _collected += CollectUpdate();

        if (_accumulatedTime >= UpdateInterval.Value) {
            // Calculate rate for the current interval
            float currentRate = _collected / _accumulatedTime;
            float finalValue;

            switch (ValueSmoothing.Value) {
                case DataSmoothing.ExpoWeightedMovingAverage:
                    // EWMA Formula: S_t = alpha * Y_t + (1 - alpha) * S_{t-1}
                    if (!_smoothedValue.HasValue) {
                        _smoothedValue = currentRate;
                    } else {
                        float alpha = Mathf.Clamp01(EwmaAlpha.Value);
                        _smoothedValue = (alpha * currentRate) + ((1f - alpha) * _smoothedValue.Value);
                    }
                    finalValue = _smoothedValue.Value;
                    break;

                case DataSmoothing.None:
                default:
                    _smoothedValue = null; // Reset smoothing buffer if switched off
                    finalValue = currentRate;
                    break;
            }

            // Update displayed text
            Text = FormatStat(finalValue);

            // Reset tracking buffers
            _accumulatedTime = 0f;
            _collected = 0f;
        }
    }

    /// <summary>
    /// The method that should return the addition each frame.
    /// For FPS, this is always 1. For DPS, this is the damage cumulated the last frame.
    /// </summary>
    /// <returns></returns>
    protected abstract float CollectUpdate();

    /// <summary>
    /// How to format the stat
    /// </summary>
    /// <param name="value">The value</param>
    /// <returns>The string representation</returns>
    protected virtual string FormatStat(float value) {
        return $"{Math.Round(value)}";
    }

    private void UpdateDisplayIcon() {
        DisplayIcon = ShowIcon.Value ? Icon : null;
    }
}
