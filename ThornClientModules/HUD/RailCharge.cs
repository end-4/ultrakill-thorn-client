using System;
using ThornClient.Core.DataTypes;
using ThornClient.HUD;
using ThornClient.Managers;
using UnityEngine;

namespace ThornClientModules.HUD;

/// <summary>
/// HUD widget that shows railcannon charge
/// </summary>
public class RailCharge : BoundedValueHudModule {
    /// <inheritdoc />
    public override Sprite Icon => AssetManager.Get<Sprite>(HudManager.BundleKey, "bolt");

    /// <inheritdoc />
    public override string[] Tags => ["charge", "ultimate", "shock", "electric", "thunderbolt", "shot"];

    /// <inheritdoc />
    public RailCharge() : base("thorn.railCharge", "Railcannon: <color=#40e7ff>Charge</color>", "Shows the railcannon charge",
        bound: 1, displayName: "Railcannon", defaultValueColor: new EnhancedColor(0.44f, 0.52f, 1f)
    ) {
    }

    /// <inheritdoc />
    public override void OnUpdate() {
        var wc = WeaponCharges.Instance;
        if (wc == null) return;
        var scaled = Mathf.Clamp01(wc.raicharge / 4f);
        ChargeValue = scaled;
        Value = Mathf.Approximately(scaled, 1) ? 1 : 0;
    }
}
