using System;
using ThornClient.Core.DataTypes;
using ThornClient.HUD;
using ThornClient.Managers;
using UnityEngine;

namespace ThornClientModules.HUD;

/// <summary>
/// Shows red rocket oil charge
/// </summary>
public class RocketRedCharge : BoundedValueHudModule {
    /// <inheritdoc />
    public override Sprite Icon => AssetManager.Get<Sprite>(HudManager.BundleKey, "oil");

    /// <inheritdoc />
    public override string[] Tags => ["cooldown"];

    /// <inheritdoc />
    public RocketRedCharge() : base("thorn.rocketRedCharge", "Rocket: <color=#f00>Oil</color> charge",
        "Shows oil charge of the red rocket launcher",
        bound: 1, displayName: "Oil", defaultValueColor: new EnhancedColor(1f, 0.235f, 0.235f)
    ) {
    }

    /// <inheritdoc />
    public override void OnUpdate() {
        var wc = WeaponCharges.Instance;
        if (wc == null) return;
        ChargeValue = wc.rocketNapalmFuel;
        Value = wc.rocketNapalmFuel > 0.25 ? wc.rocketNapalmFuel : 0;
    }
}
