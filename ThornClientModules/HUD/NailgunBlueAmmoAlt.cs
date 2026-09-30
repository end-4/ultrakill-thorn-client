using System;
using ThornClient.Core.DataTypes;
using ThornClient.HUD;
using ThornClient.Managers;
using UnityEngine;

namespace ThornClientModules.HUD;

/// <summary>
/// Shows blue sawgun saws
/// </summary>
public class NailgunBlueAmmoAlt : BoundedValueHudModule {
    /// <inheritdoc />
    public override Sprite Icon => AssetManager.Get<Sprite>(HudManager.BundleKey, "cube");

    /// <inheritdoc />
    public override string[] Tags => ["cooldown"];

    /// <inheritdoc />
    public NailgunBlueAmmoAlt() : base("thorn.nailgunBlueAmmoAlt", "Nailgun: <color=#40e7ff>Silver Saws</color>",
        "Shows saws (ammo) for the blue sawgun (alt nailgun)",
        bound: 10, displayName: "Silver Saws", defaultValueColor: new EnhancedColor(0.25f, 0.91f, 1f)
    ) {
    }

    /// <inheritdoc />
    public override void OnUpdate() {
        var wc = WeaponCharges.Instance;
        if (wc == null) return;
        ChargeValue = wc.naiSaws;
        Value = Mathf.Floor(wc.naiSaws);
    }
}
