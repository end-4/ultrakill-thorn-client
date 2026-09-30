using System;
using ThornClient.Core.DataTypes;
using ThornClient.HUD;
using ThornClient.Managers;
using UnityEngine;

namespace ThornClientModules.HUD;

/// <summary>
/// Shows saw (red) shotgun charge
/// </summary>
public class ShotgunRedCharge : BoundedValueHudModule {
    /// <inheritdoc />
    public override Sprite Icon => AssetManager.Get<Sprite>(HudManager.BundleKey, "cube");

    /// <inheritdoc />
    public override string[] Tags => ["cooldown"];

    /// <inheritdoc />
    public ShotgunRedCharge() : base("thorn.shotgunRedCharge", "Shotgun: <color=#f00>Saw</color>",
        "Shows shotgun saw availability",
        bound: 1, displayName: "Shotgun Saw", defaultValueColor: new EnhancedColor(1f, 0.235f, 0.235f)
    ) {
    }

    /// <inheritdoc />
    public override void OnUpdate() {
        var wc = WeaponCharges.Instance;
        if (wc == null) return;
        ChargeValue = wc.shoSawCharge;
        Value = Mathf.Floor(wc.shoSawCharge);
    }
}
