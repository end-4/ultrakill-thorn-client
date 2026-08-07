using System;
using ThornClient.HUD;
using ThornClient.Managers;
using UnityEngine;

namespace ThornClient.Modules.HUD;

public class HammerYellowHits : BoundedValueHudModule {
    public override Sprite Icon => AssetManager.Get<Sprite>(HudManager.BundleKey, "jackhammer");
    public override string[] Tags => ["jackhammer", "alternative shotgun", "impact hammer"];

    public static readonly int MaxYellowHits = 3;

    public HammerYellowHits() : base("thorn.hammerYellowHits", "Hammer Yellow Hits", "Basically Hammer Stats without per-variant cooldowns", 3) {
        ValueColor.DefaultValue = new Color(0.820f, 0.698f, 0f);
    }

    public override void OnUpdate() {
        var wc = WeaponCharges.Instance;
        if (wc == null) return;
        Value = Math.Max(MaxYellowHits - wc.shoAltYellows, 0);
        if (Value > 0) {
            BoundReduction = 0;
        } else {
            BoundReduction = (wc.shoAltYellowsTimer / 3f) * MaxYellowHits;
        }
    }
}
