using ThornClient.Managers;
using ThornClient.System;
using UnityEngine;
using ThornClient.Core;

namespace ThornClient.Modules.Player;

public class Speed : Module {
    private static readonly float BaseWalkSpeed = 750;
    public Setting<float> SpeedMultiplier { get; }
    public override Sprite Icon => AssetManager.Get<Sprite>(ClickGUI.BundleKey, "speed");

    public Speed() : base("thorn.speed", "Speed", "Adjusts your speed",
        ModuleCategory.Player) {
        SpeedMultiplier = RegisterSetting("speedMultiplier", "Speed multiplier", "How much to scale the speed", 2f);
    }

    public override string? CheatReason => IsEnabled ? "Adds crazy movement" : "";

    protected override void OnEnable() {
        UpdateSpeed(SpeedMultiplier.Value);
        SpeedMultiplier.OnValueChanged += UpdateSpeed;
    }

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
