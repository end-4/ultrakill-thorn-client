using ThornClient.HUD;
using ThornClient.Managers;
using UnityEngine;

namespace ThornClient.Modules.HUD;

public class HealthDisplay : BoundedValueHudModule {
    public override Sprite Icon => AssetManager.Get<Sprite>(HudManager.BundleKey, "plus_thick");
    public override string[] Tags => ["hp", "hit points", "blood"];

    public HealthDisplay() : base("thorn.healthHud", "Health", "Shows health", 100) {
        ValueColor.DefaultValue = new Color(1, 0, 0);
    }

    protected override void OnEnable() {
        base.OnEnable();
        DecimalPlaces = 0;
    }

    public override void OnUpdate() {
        var nm = NewMovement.Instance;
        if (nm != null) {
            SoftBound = Bound - nm.antiHp;
            Value = nm.hp;
        }
    }
}
