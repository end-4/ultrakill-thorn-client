using System;
using System.Linq;
using System.Reflection;
using NukeLib.Utils;
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
    public override Sprite Icon => AssetManager.Get<Sprite>(HudManager.BundleKey, "nail_overheat");

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
    protected override void OnHudModuleEnable() {
        if (GunControl.Instance != null) GunControl.Instance.OnWeaponChange += TryRecordWeapon;
        SceneUtils.SafeSceneLoadedNoParam += ResubWeaponChange;
    }

    /// <inheritdoc />
    protected override void OnHudModuleDisable() {
        if (GunControl.Instance != null) GunControl.Instance.OnWeaponChange -= TryRecordWeapon;
        SceneUtils.SafeSceneLoadedNoParam -= ResubWeaponChange;
    }

    private void ResubWeaponChange() {
        if (GunControl.Instance == null) return;
        GunControl.Instance.OnWeaponChange -= TryRecordWeapon;
        GunControl.Instance.OnWeaponChange += TryRecordWeapon;
    }

    private void TryRecordWeapon(GameObject weapon) {
        if (_nai != null || weapon == null) return;
        var comp = weapon.GetComponent<Nailgun>();
        if (comp == null) return;
        // Note that green is 0 somehow
        if (comp is { variation: 0, altVersion: false }) _nai = comp;
    }

    /// <inheritdoc />
    public override void OnUpdate() {
        var gc = GunControl.Instance;
        var wc = WeaponCharges.Instance;
        if (gc == null || wc == null || _nai == null) return;

        var rawVal = gc.currentWeapon == _nai.gameObject
            ? (NailgunHeatsinkField?.GetValue(_nai) as float? ?? 0)
            : wc.naiHeatsinks;
        ChargeValue = rawVal;
        Value = Mathf.Floor(rawVal);
    }
}
