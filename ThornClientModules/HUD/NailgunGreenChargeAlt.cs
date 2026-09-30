using System.Reflection;
using NukeLib.Utils;
using ThornClient.Core.DataTypes;
using ThornClient.HUD;
using ThornClient.Managers;
using UnityEngine;

namespace ThornClientModules.HUD;

/// <summary>
/// Shows green sawgun overheat saws
/// </summary>
public class NailgunGreenChargeAlt : BoundedValueHudModule {
    /// <inheritdoc />
    public override Sprite Icon => AssetManager.Get<Sprite>(HudManager.BundleKey, "saw_overheat");

    /// <inheritdoc />
    public override string[] Tags => ["cooldown"];

    /// <inheritdoc />
    public NailgunGreenChargeAlt() : base("thorn.nailgunGreenChargeAlt",
        "Nailgun: <color=#44ff45>Overheat Saw</color>",
        "Shows availability of the overheat saw for the green alt nailgun", bound: 1, displayName: "Heat Saw",
        defaultValueColor: new EnhancedColor(0.27f, 1f, 0.27f)
    ) {
        Value = 1;
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
            _nai = Object.FindObjectsOfType<Nailgun>().FirstOrDefault(n => n.variation == 0 && n.altVersion);
        }

        if (_nai == null) return;

        var rawVal = gc.currentWeapon == _nai.gameObject
            ? (NailgunHeatsinkField?.GetValue(_nai) as float? ?? 0)
            : wc.naiSawHeatsinks;
        ChargeValue = rawVal;
        Value = Mathf.Floor(rawVal);
    }
}
