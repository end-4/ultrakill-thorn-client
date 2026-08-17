using System;
using ThornClient.Core.ConfigurableElements;
using ThornClient.Managers;
using ThornClient.System;
using UnityEngine;
using ThornClient.HUD;

namespace ThornClient.Modules.HUD;

/// <summary>
/// Module that shows speed
/// </summary>
public class Speedometer : TextHudModule {
    /// <summary>
    /// Setting: whether to show "Speed" text on the HUD element
    /// </summary>
    public Setting<bool> ShowSpeedText;

    /// <inheritdoc />
    public override Sprite Icon => AssetManager.Get<Sprite>(ClickGUI.BundleKey, "speed");

    /// <inheritdoc />
    public override Sprite DisplayIcon => Icon;

    /// <inheritdoc />
    public override string[] Tags => ["fast", "velocity", "unit", "meter"];

    /// <summary>
    /// Constructor
    /// </summary>
    public Speedometer() : base("thorn.speedometer", "Speedometer", "Shows speed (in units/sec)") {
        ShowSpeedText = CreateSetting("showSpeedText", "Show \"Speed\" text",
            "Makes the indicator say \"Speed:100\" instead of \"100\"", true);
    }

    protected override void OnHudModuleEnable() {
        ShowSpeedText.OnChanged += OnFixedUpdate;
        OnFixedUpdate();
    }

    protected override void OnHudModuleDisable() {
        ShowSpeedText.OnChanged -= OnFixedUpdate;
    }

    private PlayerTracker? ptrack => PlayerTracker.Instance;

    /// <inheritdoc />
    public override void OnFixedUpdate() {
        if (ptrack == null) return;
        var vel = ptrack.GetPlayerVelocity(trueVelocity: true);
        if (vel == null) return;
        var mag = vel.magnitude;
        var prefix = ShowSpeedText.Value ? "Speed:" : "";
        var formattedVal = Math.Round(mag, 1);
        Text = $"{prefix}{formattedVal}";
    }
}
