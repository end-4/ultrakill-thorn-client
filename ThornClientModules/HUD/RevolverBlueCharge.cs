using System;
using System.Linq;
using NukeLib.Utils;
using ThornClient;
using ThornClient.Core.DataTypes;
using ThornClient.HUD;
using ThornClient.Managers;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ThornClientModules.HUD;

/// <summary>
/// Shows piercer (blue) revolver charge
/// </summary>
public class RevolverBlueCharge : BoundedValueHudModule {
    /// <inheritdoc />
    public override Sprite Icon => AssetManager.Get<Sprite>(HudManager.BundleKey, "piercer");

    /// <inheritdoc />
    public override string[] Tags => ["cooldown", "ultraricoshot", "blue"];

    /// <inheritdoc />
    public RevolverBlueCharge() : base("thorn.revolverBlueCharge", "Revolver: <color=#40e7ff>Piercer</color> charge",
        "Shows alt fire charge for the blue revolver",
        bound: 1, displayName: "Piercer", defaultValueColor: new EnhancedColor(0.25f, 0.91f, 1f)
    ) {
        Value = 1;
    }

    private Revolver? _rev;

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
        if (_rev != null || weapon == null) return;
        var comp = weapon.GetComponent<Revolver>();
        if (comp == null) return;
        if (comp.gunVariation == 0) _rev = comp;
    }

    /// <inheritdoc />
    public override void OnUpdate() {
        var gc = GunControl.Instance;
        var wc = WeaponCharges.Instance;
        if (gc == null || wc == null) return;

        if (_rev == null) return;

        var scaled = (gc.currentWeapon == _rev.gameObject ? _rev.pierceCharge : wc.rev0charge) / 100f;
        ChargeValue = scaled;
        Value = Mathf.Floor(scaled);
    }
}
