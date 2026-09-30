using ThornClient.Core.DataTypes;
using ThornClient.HUD;
using ThornClient.Managers;
using UnityEngine;

namespace ThornClientModules.HUD;

/// <summary>
/// Shows green rocket launcher cannonballs
/// </summary>
public class RocketGreenCharge : BoundedValueHudModule {
    /// <inheritdoc />
    public override Sprite Icon => AssetManager.Get<Sprite>(HudManager.BundleKey, "ball");

    /// <inheritdoc />
    public override string[] Tags => ["srs", "ball", "cerberus", "sentry", "virtue", "cooldown"];

    /// <inheritdoc />
    public RocketGreenCharge() : base("thorn.rocketGreenCharge", "Rocket: <color=#44ff45>Cannonball</color>",
        "Shows availability of the green rocket launcher's cannonball", bound: 1, displayName: "Cannonball",
        defaultValueColor: new EnhancedColor(0.27f, 1f, 0.27f)
    ) {
    }

    /// <inheritdoc />
    public override void OnUpdate() {
        var wc = WeaponCharges.Instance;
        if (wc == null) return;
        ChargeValue = wc.rocketCannonballCharge;
        Value = Mathf.Floor(wc.rocketCannonballCharge);
    }
}
