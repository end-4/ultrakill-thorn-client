using System;
using System.Collections.Generic;
using ThornClient.Core.ConfigurableElements;
using ThornClient.Core.DataTypes;
using ThornClient.System;
using UnityEngine;

namespace ThornClient.Managers;

/// <summary>
/// Handles EnhancedColor effects
/// </summary>
public static class ColorManager {
    public static float GlobalHue { get; private set; }
    public static float GlobalSpeed => ThornModule.Instance?.GlobalHuePulseRate.Value ?? 0.5f;

    private static readonly HashSet<Setting<EnhancedColor>> EnhancedColorSettings = [];

    /// <summary>
    /// Registers an enhanced color setting to receive effective value change events
    /// </summary>
    /// <param name="enhancedColorSetting">The EnhancedColor setting</param>
    public static void RegisterEnhancedColorSetting(Setting<EnhancedColor> enhancedColorSetting) {
        EnhancedColorSettings.Add(enhancedColorSetting);
    }

    /// <summary>
    /// Un-registers an enhanced color setting
    /// </summary>
    /// <param name="enhancedColorSetting">The EnhancedColor setting</param>
    public static void UnregisterEnhancedColorSetting(Setting<EnhancedColor> enhancedColorSetting) {
        EnhancedColorSettings.Remove(enhancedColorSetting);
    }

    /// <summary>
    /// Update loop
    /// </summary>
    public static void Update() {
        GlobalHue = (Time.unscaledTime * GlobalSpeed) % 1.0f;
        foreach (var setting in EnhancedColorSettings) {
            var val = setting.Value;
            switch (val.Mode) {
                case EnhancedColorMode.Static:
                    continue;
                case EnhancedColorMode.HuePulse:
                    setting.RaiseOnEffectiveValueChanged();
                    break;
            }
        }
    }
}
