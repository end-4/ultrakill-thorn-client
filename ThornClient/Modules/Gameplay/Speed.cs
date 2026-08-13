using ThornClient.Managers;
using ThornClient.System;
using UnityEngine;
using ThornClient.Core;
using ThornClient.Core.ConfigurableElements;

namespace ThornClient.Modules.Gameplay;

/// <summary>
/// I'm Sonic
/// </summary>
public class Speed : Module {
    private static readonly float BaseWalkSpeed = 750;

    /// <summary>
    /// How fast to go
    /// </summary>
    public Setting<float> SpeedMultiplier { get; }

    /// <summary>
    /// Icon of this module
    /// </summary>
    public override Sprite Icon => AssetManager.Get<Sprite>(ClickGUI.BundleKey, "speed");

    /// <summary>
    /// Tags for search
    /// </summary>
    public override string[] Tags => ["cheat", "fast", "slow", "movement", "sonic"];

    /// <summary>
    /// Constructor
    /// </summary>
    public Speed() : base("thorn.speed", "Speed", "Adjusts your speed",
        ModuleCategory.Gameplay) {
        SpeedMultiplier = CreateSetting("speedMultiplier", "Speed multiplier", "How much to scale the speed", 2f);
    }

    /// <summary>
    /// Why this disables leaderboards
    /// </summary>
    public override string? CheatReason => IsEnabled ? "Adds crazy movement" : "";

    /// <summary>
    /// Stuff that run when enabled
    /// </summary>
    protected override void OnEnable() {
        UpdateSpeed(SpeedMultiplier.Value);
        SpeedMultiplier.OnValueChanged += UpdateSpeed;
    }

    /// <summary>
    /// Stuff that run when disabled
    /// </summary>
    protected override void OnDisable() {
        SpeedMultiplier.OnValueChanged -= UpdateSpeed;
        UpdateSpeed(1f);
    }

    private void UpdateSpeed(float multiplier) {
        var nm = NewMovement.Instance;
        if (nm == null) return;
        nm.walkSpeed = BaseWalkSpeed * multiplier;
    }
}
