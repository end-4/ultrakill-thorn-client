using System;
using ThornClient.Core.DataTypes;
using ThornClient.HUD;
using ThornClient.Managers;
using UnityEngine;

namespace ThornClientModules.HUD;

/// <summary>
/// Shows core (blue) shotgun charge
/// </summary>
public class ShotgunBlueChargeAlt : BoundedValueHudModule {
    /// <inheritdoc />
    public override Sprite Icon => AssetManager.Get<Sprite>(HudManager.BundleKey, "jackhammer_core");

    /// <inheritdoc />
    public override string[] Tags => ["cooldown", "core eject", "nuke", "blue", "explosion"];

    /// <inheritdoc />
    public ShotgunBlueChargeAlt() : base("thorn.shotgunBlueChargeAlt", "Shotgun: <color=#40e7ff>Core</color> charge (hammer)",
        "Shows the blue hammer's core availability",
        bound: 1, displayName: "Hammer Core", defaultValueColor: new EnhancedColor(0.25f, 0.91f, 1f)
    ) {
    }

    /// <inheritdoc />
    public override void OnUpdate() {
        var wc = WeaponCharges.Instance;
        if (wc == null) return;
        ChargeValue = wc.shoAltNadeCharge;
        Value = Mathf.Floor(wc.shoAltNadeCharge);
    }
}
