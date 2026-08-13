using System;
using ThornClient.HUD;
using ThornClient.Managers;
using UnityEngine;

namespace ThornClient.Modules.HUD;

public class RailCharge : BoundedValueHudModule {
    public override Sprite Icon => AssetManager.Get<Sprite>(HudManager.BundleKey, "bolt");
    public override string[] Tags => ["charge", "ultimate", "shock", "electric", "thunderbolt", "shot"];

    public RailCharge() : base("thorn.railCharge", "Railcannon Charge", "Shows the railcannon charge", 1,
        displayName: "Railcannon", defaultValueColor: new Color(0.44f, 0.52f, 1f)) {
    }

    /// <inheritdoc />
    protected override void OnHudModuleEnable() {
        DecimalPlaces = 1;
    }

    public override void OnUpdate() {
        var wc = WeaponCharges.Instance;
        if (wc != null) {
            var scaled = wc.raicharge / 5f;
            Value = scaled;
        }
    }
}
