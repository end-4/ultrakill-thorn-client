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
    public override string[] Tags => ["overheat", "cooldown", "fire", "burn", "bounce", "ricochet"];

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
        if (comp is { variation: 0, altVersion: true }) _nai = comp;
    }

    /// <inheritdoc />
    public override void OnUpdate() {
        var gc = GunControl.Instance;
        var wc = WeaponCharges.Instance;
        if (gc == null || wc == null || _nai == null) return;

        var rawVal = gc.currentWeapon == _nai.gameObject
            ? (NailgunHeatsinkField?.GetValue(_nai) as float? ?? 0)
            : wc.naiSawHeatsinks;
        ChargeValue = rawVal;
        Value = Mathf.Floor(rawVal);
    }
}
