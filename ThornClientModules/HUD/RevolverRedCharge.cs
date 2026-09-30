using System;
using ThornClient.Core.DataTypes;
using ThornClient.HUD;
using ThornClient.Managers;
using UnityEngine;

namespace ThornClientModules.HUD;

/// <summary>
/// Shows (red) rocket launcher oil charge
/// </summary>
public class RevolverRedCharge : BoundedValueHudModule {
    /// <inheritdoc />
    public override Sprite Icon => AssetManager.Get<Sprite>(HudManager.BundleKey, "sharpshooter");

    /// <inheritdoc />
    public override string[] Tags => ["cooldown"];

    /// <inheritdoc />
    public RevolverRedCharge() : base("thorn.revolverRedCharge", "Revolver: <color=#f00>Sharpshooter</color> charge",
        "Shows alt fire charge for the red revolver",
        bound: 3, displayName: "Sharpshooter", defaultValueColor: new EnhancedColor(1f, 0.235f, 0.235f)
    ) {
    }

    /// <inheritdoc />
    public override void OnUpdate() {
        var wc = WeaponCharges.Instance;
        if (wc == null) return;
        var scaled = wc.rev2charge / 100f;
        ChargeValue = scaled;
        Value = Mathf.Floor(scaled);
    }
}
