using System;
using ThornClient.Core.DataTypes;
using ThornClient.HUD;
using ThornClient.Managers;
using UnityEngine;

namespace ThornClientModules.HUD;

/// <summary>
/// Shows blue nailgun nails
/// </summary>
public class NailgunBlueAmmo : BoundedValueHudModule {
    /// <inheritdoc />
    public override Sprite Icon => AssetManager.Get<Sprite>(HudManager.BundleKey, "nail_silver");

    /// <inheritdoc />
    public override string[] Tags => ["cooldown"];

    /// <inheritdoc />
    public NailgunBlueAmmo() : base("thorn.nailgunBlueAmmo", "Nailgun: <color=#40e7ff>Silver Nails</color>",
        "Shows nails (ammo) for the blue nailgun",
        bound: 100, displayName: "Silver Nails", defaultValueColor: new EnhancedColor(0.25f, 0.91f, 1f)
    ) {
    }

    /// <inheritdoc />
    public override void OnUpdate() {
        var wc = WeaponCharges.Instance;
        if (wc == null) return;
        ChargeValue = wc.naiAmmo;
        Value = Mathf.Floor(wc.naiAmmo);
    }
}
