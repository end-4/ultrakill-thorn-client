using NukeLib.UI;
using ThornClient.Managers;
using ThornClient.System;
using UnityEngine;
using ThornClient.HUD;
using ThornClient.HUD.HUDComponents;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ThornClient.Modules.HUD;

// --------------------------------------------------------------------------------------
// This file was especially well-commented to serve as an example recommended on the wiki
// It's not minimal, but it's somewhat simple.
// --------------------------------------------------------------------------------------

/// <summary>
/// Module that shows active fist (Feedbacker/Knuckleblaster) and punch stamina
/// </summary>
public class Fist : FramedHudModule {
    /// <inheritdoc />
    public override Sprite Icon => AssetManager.Get<Sprite>(ClickGUI.BundleKey, "hand");

    /// <inheritdoc />
    public override string[] Tags => ["hand", "punch", "feedbacker", "knuckleblaster"];

    /// <inheritdoc />
    public Fist() : base("thorn.fist", "Fist", "Shows punch cooldowns") {
    }

    /// <inheritdoc />
    protected override GameObject CreateContentObject() {
        // This is the method we must implement and return the UI element
        // First, we instantiate the UI content from a prefab
        // Looks a bit scary, but this is just instantiating a prefab from a bundle.
        // You don't have to set a parent transform.
        var obj = Object.Instantiate(AssetManager.Get<GameObject>(HudManager.BundleKey, "FistPanel"));
        // This controller component is to actually script that object. See the class below
        obj.AddComponent<FistController>();
        return obj;
    }

    /// <summary>
    /// Controller for the fist indicator
    /// </summary>
    protected class FistController : MonoBehaviour {
        // We have these for caching the components
        private Image? _bg;
        private Image? _fg;
        private HudVariantColorSyncer? _bgCol;
        private HudVariantColorSyncer? _fgCol;

        // When the module is enabled...
        private void OnEnable() {
            // ...We cache the components
            _bg = gameObject.FindRecursive("Background")?.GetComponent<Image>();
            _fg = gameObject.FindRecursive("Panel")?.GetComponent<Image>();
            // Probably won't be null; this is just for safety
            if (_bg == null || _fg == null) return;
            // HudVariantColorSyncer is a Thorn component that applies the variant color to a GameObject's Image
            _bgCol = _bg.gameObject.GetOrAddComponent<HudVariantColorSyncer>();
            // This color multiplier here makes the background image dimmed
            _bgCol.ColorMultiplier = 0.2f;
            _fgCol = _fg.gameObject.GetOrAddComponent<HudVariantColorSyncer>();

            // FistControl is from the base game. We do this hook to update icon when the fist changes
            if (FistControl.Instance != null) FistControl.Instance.FistIconUpdated += UpdateIcon;
            // Remember to also update the first time!
            UpdateIcon();
        }

        private void OnDisable() {
            // And remember to unhook when disabling!
            if (FistControl.Instance != null) FistControl.Instance.FistIconUpdated -= UpdateIcon;
        }

        // This runs every frame...
        // Here we update the punch stamina, as it's constantly changing
        private void Update() {
            if (_fg == null || WeaponCharges.Instance == null) return;
            var newFill = WeaponCharges.Instance.punchStamina / 2f;
            if (!Mathf.Approximately(newFill, _fg.fillAmount)) _fg.fillAmount = newFill;
        }

        // Overload to make the update call on enable look nice. The real method is the next one
        private void UpdateIcon() {
            UpdateIcon(PlayerPrefs.GetInt("CurArm", 0));
        }

        private void UpdateIcon(int fistVariant) {
            // 0 = Feedbacker, 1 = Knuckleblaster. We're trying to be absolutely sure here
            if (fistVariant != 0 && fistVariant != 1) return;

            // This math transforms 0 -> 0, 1 -> 2, because
            // 0 = Blue variant = Feedbacker, 2 = Red variant = Knuckleblaster
            var colVariant = fistVariant * 2;

            // We update the variation in the HudVariantColorSyncer, and it'll update the actual color
            if (_bgCol != null) _bgCol.Variation = colVariant;
            if (_fgCol != null) _fgCol.Variation = colVariant;

            // And we update the icon to be of the right fist
            // The image in the base game is black and is handled a bit differently, so I cloned
            // it and made it white
            if (_bg == null || _fg == null) return;
            var iconName = fistVariant == 1 ? "knuckleblaster" : "feedbacker";
            _bg.sprite = AssetManager.Get<Sprite>(HudManager.BundleKey, iconName);
            _fg.sprite = AssetManager.Get<Sprite>(HudManager.BundleKey, iconName);
        }
    }
}
