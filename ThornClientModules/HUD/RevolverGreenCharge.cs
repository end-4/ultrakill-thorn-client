using System;
using ThornClient.Core.DataTypes;
using ThornClient.HUD;
using ThornClient.Managers;
using UnityEngine;

namespace ThornClientModules.HUD;

/// <summary>
/// Shows number of available coins
/// </summary>
public class RevolverGreenCharge : BoundedValueHudModule {
    /// <inheritdoc />
    public override Sprite Icon => AssetManager.Get<Sprite>(HudManager.BundleKey, "cube");

    /// <inheritdoc />
    public override string[] Tags => ["cooldown"];

    /// <inheritdoc />
    public RevolverGreenCharge() : base("thorn.revolverCoins", "Revolver: <color=#44ff45>Coins</color>",
        "Shows number of coins ready on the green revolver",
        bound: 4, displayName: "Coins", defaultValueColor: new EnhancedColor(0.27f, 1f, 0.27f)
    ) {
    }

    /// <inheritdoc />
    public override void OnUpdate() {
        var wc = WeaponCharges.Instance;
        if (wc == null) return;
        var scaled = wc.rev1charge / 100f;
        ChargeValue = scaled;
        Value = Mathf.Floor(scaled);
    }
}
