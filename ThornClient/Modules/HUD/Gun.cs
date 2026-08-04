using NukeLib.UI;
using ThornClient.HUD;
using ThornClient.HUD.HUDComponents;
using ThornClient.Managers;
using ThornClient.System;
using UnityEngine;
using UnityEngine.UI;

namespace ThornClient.Modules.HUD;

/// <summary>
/// Module that shows currently equipped weapon
/// </summary>
public class Gun : FramedHudModule {
    /// <summary>
    /// Icon of the module
    /// </summary>
    public override Sprite Icon => AssetManager.Get<Sprite>(ClickGUI.BundleKey, "gun");

    /// <summary>
    /// Tags for search
    /// </summary>
    public override string[] Tags => ["variant", "gun", "weapon"];

    /// <summary>
    /// Constructor
    /// </summary>
    public Gun() : base("thorn.gun", "Gun", "Shows currently equipped icon") {
    }

    /// <summary>
    /// Creates the content GameObject that sits on the frame
    /// </summary>
    /// <returns>The content object</returns>
    protected override GameObject CreateContentObject() {
        var obj = Object.Instantiate(AssetManager.Get<GameObject>(HudManager.BundleKey, "GunPanel"));
        obj.AddComponent<GunIconController>();
        return obj;
    }

    private class GunIconController : MonoBehaviour {
        private Image? _icon;
        private HudVariantColorSyncer? _iconColSyncer;

        private GunControl? Gci => GunControl.Instance;

        private void OnEnable() {
            _icon = gameObject.FindRecursive("Icon")?.GetComponent<Image>();
            _iconColSyncer = _icon.GetOrAddComponent<HudVariantColorSyncer>();
            if (_icon == null) return;
            if (Gci != null) Gci.OnWeaponChange += UpdateIcon;
            UpdateIcon();
        }

        private void OnDisable() {
            if (Gci != null) Gci.OnWeaponChange -= UpdateIcon;
        }

        private void UpdateIcon() {
            UpdateIcon(Gci?.currentWeapon);
        }

        private void UpdateIcon(GameObject? gob) {
            if (gob == null) return;
            var weaponIconComp = gob.GetComponent<WeaponIcon>();
            if (weaponIconComp == null) return;
            var weaponIcon = weaponIconComp.weaponDescriptor.icon;
            var variationColor = (int)weaponIconComp.weaponDescriptor.variationColor;

            if (_icon != null) _icon.sprite = weaponIcon;
            if (_iconColSyncer != null) _iconColSyncer.Variation = variationColor;
        }
    }
}
