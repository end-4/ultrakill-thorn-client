using System;
using System.Linq;
using System.Reflection;
using ThornClient.Core.DataTypes;
using ThornClient.HUD;
using ThornClient.Managers;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ThornClientModules.HUD;

/// <summary>
/// Shows green nailgun heatsinks
/// </summary>
public class NailgunGreenCharge : BoundedValueHudModule {
    /// <inheritdoc />
    public override Sprite Icon => AssetManager.Get<Sprite>(HudManager.BundleKey, "cube");

    /// <inheritdoc />
    public override string[] Tags => ["cooldown"];

    /// <inheritdoc />
    public NailgunGreenCharge() : base("thorn.nailgunGreenCharge", "Nailgun: <color=#44ff45>Nail Heatsinks</color>",
        "Shows number of available overheat nail bursts for the green nailgun", bound: 2, displayName: "Heatsinks",
        defaultValueColor: new EnhancedColor(0.27f, 1f, 0.27f)
    ) {
        Value = 2;
    }

    private Nailgun? _nai;

    private static readonly FieldInfo? NailgunHeatsinkField = typeof(Nailgun).GetField(
        "heatSinks", BindingFlags.NonPublic | BindingFlags.Instance);

    /// <inheritdoc />
    public override void OnUpdate() {
        var gc = GunControl.Instance;
        var wc = WeaponCharges.Instance;
        if (gc == null || wc == null) return;

        if (_nai == null) {
            // Green is 0 somehow
            _nai = Object.FindObjectsOfType<Nailgun>().FirstOrDefault(n => n.variation == 0 && !n.altVersion);
        }

        if (_nai == null) return;

        var rawVal = gc.currentWeapon == _nai.gameObject
            ? (NailgunHeatsinkField?.GetValue(_nai) as float? ?? 0)
            : wc.naiHeatsinks;
        ChargeValue = rawVal;
        Value = Mathf.Floor(rawVal);
    }
}
