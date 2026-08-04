using ThornClient.HUD;
using ThornClient.Managers;
using UnityEngine;

namespace ThornClient.Modules.HUD;

/// <summary>
/// Module that shows HP
/// </summary>
public class HealthDisplay : BoundedValueHudModule {
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
    public HealthDisplay() : base("thorn.healthHud", "Health", "Shows health", 100) {
        ValueColor.DefaultValue = new Color(1, 0, 0);
    }

    /// <summary>
    /// Stuff that run when the module is enabled
    /// </summary>
    protected override void OnEnable() {
        base.OnEnable();
        DecimalPlaces = 0;
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
