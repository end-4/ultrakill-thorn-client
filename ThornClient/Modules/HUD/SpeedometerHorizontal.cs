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
public class SpeedometerHorizontal : TextHudModule {
    /// <summary>
    /// Setting: whether to show "Speed" text on the HUD element
    /// </summary>
    public Setting<bool> ShowSpeedText;

    /// <inheritdoc />
    public override Sprite Icon => AssetManager.Get<Sprite>(ClickGUI.BundleKey, "move_horizontal");

    /// <inheritdoc />
    public override Sprite DisplayIcon => Icon;

    /// <inheritdoc />
    public override string[] Tags => ["velocity", "fast", "unit", "meter"];

    /// <summary>
    /// Constructor
    /// </summary>
    public SpeedometerHorizontal() : base("thorn.speedometerHorizontal", "Speedometer (Horizontal)",
        "Shows horizontal speed (in units/sec)") {
        ShowSpeedText = CreateSetting("showSpeedText", "Show \"H-Speed\" text",
            "Makes the indicator say \"H-Speed:100\" instead of \"100\"", true);
    }

    private NewMovement? nm => NewMovement.Instance;
    private PlayerTracker? ptrack => PlayerTracker.Instance;

    /// <inheritdoc />
    public override void OnFixedUpdate() {
        if (nm == null || ptrack == null) return;
        var prefix = ShowSpeedText.Value ? "H-Speed:" : "";
        var vel = ptrack.GetPlayerVelocity(trueVelocity: true);
        Vector3 gravityDir = nm.rb.GetGravityDirection();
        var projected = Vector3.ProjectOnPlane(vel, gravityDir);
        var magnitude = projected.magnitude;
        var formattedVal = Math.Round(magnitude, 1);
        Text = $"{prefix}{formattedVal}";
    }
}
