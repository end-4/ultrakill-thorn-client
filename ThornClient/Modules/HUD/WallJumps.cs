using ThornClient.HUD;
using ThornClient.Managers;
using UnityEngine;

namespace ThornClient.Modules.HUD;

public class WallJumps : BoundedValueHudModule {
    public override Sprite Icon => AssetManager.Get<Sprite>(HudManager.BundleKey, "wall_jump");
    public override string[] Tags => ["jump", "cling", "kick", "hang"];

    public static readonly int MaxWallJumps = 3;

    public WallJumps() : base("thorn.wallJumps", "Wall Jumps", "Shows remaining wall jumps before you have to land",
        MaxWallJumps,
        displayName: "Wall Jumps", defaultValueColor: new Color(0.78f, 0.77f, 0.38f)) {
    }

    protected override void OnHudModuleEnable() {
        DecimalPlaces = 0;
    }

    public override void OnUpdate() {
        var nm = NewMovement.Instance;
        if (nm != null) {
            Value = MaxWallJumps - nm.currentWallJumps;
        }
    }
}
