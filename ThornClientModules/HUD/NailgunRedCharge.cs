using System;
using ThornClient.Core.DataTypes;
using ThornClient.HUD;
using ThornClient.Managers;
using UnityEngine;

namespace ThornClientModules.HUD;

/// <summary>
/// Shows jumpstart (red) nailgun charge
/// </summary>
public class NailgunRedCharge : BoundedValueHudModule {
    /// <inheritdoc />
    public override Sprite Icon => AssetManager.Get<Sprite>(HudManager.BundleKey, "plug");

    /// <inheritdoc />
    public override string[] Tags => ["shock", "zap", "electric", "cooldown"];

    /// <inheritdoc />
    public NailgunRedCharge() : base("thorn.nailgunRedCharge", "Nailgun: <color=#f00>Jumpstart cable</color> charge",
        "Shows zapper cable availability",
        bound: 1, displayName: "Cable", defaultValueColor: new EnhancedColor(1f, 0.235f, 0.235f)
    ) {
    }

    /// <inheritdoc />
    public override void OnUpdate() {
        var wc = WeaponCharges.Instance;
        if (wc == null) return;
        var scaled = wc.naiZapperRecharge / 5f;
        ChargeValue = scaled;
        Value = Mathf.Floor(scaled);
    }
}
