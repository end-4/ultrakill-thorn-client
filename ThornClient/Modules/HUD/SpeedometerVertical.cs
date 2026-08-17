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
public class SpeedometerVertical : TextHudModule {
    /// <summary>
    /// Setting: whether to show "Speed" text on the HUD element
    /// </summary>
    public Setting<bool> ShowSpeedText;

    /// <inheritdoc />
    public override Sprite Icon => AssetManager.Get<Sprite>(ClickGUI.BundleKey, "move_vertical");

    /// <inheritdoc />
    public override Sprite DisplayIcon => Icon;

    /// <inheritdoc />
    public override string[] Tags => ["velocity", "fast", "unit", "meter"];

    /// <summary>
    /// Constructor
    /// </summary>
    public SpeedometerVertical() : base("thorn.speedometerVertical", "Speedometer (Vertical)",
        "Shows vertical speed (in units/sec)") {
        ShowSpeedText = CreateSetting("showSpeedText", "Show \"V-Speed\" text",
            "Makes the indicator say \"V-Speed:100\" instead of \"100\"", true);
    }

    protected override void OnHudModuleEnable() {
        ShowSpeedText.OnChanged += OnFixedUpdate;
        OnFixedUpdate();
    }

    protected override void OnHudModuleDisable() {
        ShowSpeedText.OnChanged -= OnFixedUpdate;
    }

    private NewMovement? nm => NewMovement.Instance;
    private PlayerTracker? ptrack => PlayerTracker.Instance;

    /// <inheritdoc />
    public override void OnFixedUpdate() {
        if (nm == null || ptrack == null) return;
        var prefix = ShowSpeedText.Value ? "V-Speed:" : "";
        var vel = ptrack.GetPlayerVelocity(trueVelocity: true);
        Vector3 gravityDir = nm.rb.GetGravityDirection();
        var magnitude = Mathf.Abs(Vector3.Dot(vel, gravityDir));
        var formattedVal = Math.Round(magnitude, 1);
        Text = $"{prefix}{formattedVal}";
    }
}
