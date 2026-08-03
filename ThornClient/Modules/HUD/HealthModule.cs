using ThornClient.HUD;
using ThornClient.Managers;
using ThornClient.System;
using UnityEngine;

namespace ThornClient.Modules.HUD;

public class HealthModule : BoundedValueHudModule {
    public override Sprite Icon => AssetManager.Get<Sprite>(HudManager.BundleKey, "plus_thick");
    public override string[] Tags => ["hp", "hit points", "blood"];

    public HealthModule() : base("thorn.healthHud", "Health", "Shows health") {
    }

    protected override void OnEnable() {
        base.OnEnable();
        // TODO nicer API for this
        DisplayName = "Health";
        Bound = 100;
        DecimalPlaces = 0;
        DisplayIcon = AssetManager.Get<Sprite>(HudManager.BundleKey, "plus_thick");
    }

    public override void OnUpdate() {
        var nm = NewMovement.Instance;
        if (nm != null) {
            SoftBound = Bound - nm.antiHp;
            Value = nm.hp;
        }
    }
}
