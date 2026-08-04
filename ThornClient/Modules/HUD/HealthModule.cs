using ThornClient.HUD;
using ThornClient.Managers;
using UnityEngine;

namespace ThornClient.Modules.HUD;

/// <summary>
/// Module that shows HP
/// </summary>
public class HealthModule : BoundedValueHudModule {
    /// <summary>
    /// The icon of the module
    /// </summary>
    public override Sprite Icon => AssetManager.Get<Sprite>(HudManager.BundleKey, "plus_thick");

    /// <summary>
    /// Tags for search
    /// </summary>
    public override string[] Tags => ["hp", "hit points", "blood"];

    /// <summary>
    /// Constructor
    /// </summary>
    public HealthModule() : base("thorn.healthHud", "Health", "Shows health") {
    }

    /// <summary>
    /// Stuff that run when the module is enabled
    /// </summary>
    protected override void OnEnable() {
        base.OnEnable();
        // TODO nicer API for this
        DisplayName = "Health";
        Bound = 100;
        DecimalPlaces = 0;
        DisplayIcon = AssetManager.Get<Sprite>(HudManager.BundleKey, "plus_thick");
    }

    /// <summary>
    /// Stuff that run every frame
    /// </summary>
    public override void OnUpdate() {
        var nm = NewMovement.Instance;
        if (nm != null) {
            SoftBound = Bound - nm.antiHp;
            Value = nm.hp;
        }
    }
}
