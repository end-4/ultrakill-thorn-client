using System;
using ThornClient.Core.DataTypes;
using ThornClient.HUD;
using ThornClient.Managers;
using UnityEngine;

namespace ThornClientModules.HUD;

/// <summary>
/// Shows blue rocket launcher freeze juice
/// </summary>
public class RocketBlueCharge : BoundedValueHudModule {
    /// <inheritdoc />
    public override Sprite Icon => AssetManager.Get<Sprite>(HudManager.BundleKey, "freeze");

    /// <inheritdoc />
    public override string[] Tags => ["cooldown"];

    /// <inheritdoc />
    public RocketBlueCharge() : base("thorn.rocketBlueCharge",
        "Rocket: <color=#40e7ff>Freeze</color> duration charge",
        "Shows the blue rocket launcher's charge for the rocket-freezing alt fire",
        bound: 5, displayName: "Freeze", defaultValueColor: new EnhancedColor(0.25f, 0.91f, 1f)
    ) {
    }

    /// <inheritdoc />
    public override void OnUpdate() {
        var wc = WeaponCharges.Instance;
        if (wc == null) return;
        Value = wc.rocketFreezeTime;
    }
}
