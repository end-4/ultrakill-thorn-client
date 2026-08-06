using NukeLib.UI;
using ThornClient.Managers;
using ThornClient.System;
using UnityEngine;
using ThornClient.HUD;
using ThornClient.HUD.HUDComponents;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ThornClient.Modules.HUD;

/// <summary>
/// Module that shows active fist and punch stamina
/// </summary>
public class Fist : FramedHudModule {
    /// <summary>
    /// Icon of the module
    /// </summary>
    public override Sprite Icon => AssetManager.Get<Sprite>(ClickGUI.BundleKey, "feedbacker");
    /// <summary>
    /// Tags for search
    /// </summary>
    public override string[] Tags => ["hand", "punch", "feedbacker", "knuckleblaster"];

    /// <summary>
    /// Constructor
    /// </summary>
    public Fist() : base("thorn.fist", "Fist", "Shows punch cooldowns") {

    }

    /// <summary>
    /// Creates the content of the HUD element
    /// </summary>
    /// <returns>The content GameObject</returns>
    protected override GameObject CreateContentObject() {
        var obj = Object.Instantiate(AssetManager.Get<GameObject>(HudManager.BundleKey, "FistPanel"));
        obj.AddComponent<FistController>();
        return obj;
    }

    private class FistController : MonoBehaviour {
        private Image? _bg;
        private Image? _fg;
        private HudVariantColorSyncer? _bgCol;
        private HudVariantColorSyncer? _fgCol;
        private void Start() {
            _bg = gameObject.FindRecursive("Background")?.GetComponent<Image>();
            _fg = gameObject.FindRecursive("Panel")?.GetComponent<Image>();
            if (_bg == null || _fg == null) return;
            _bgCol = _bg.gameObject.GetOrAddComponent<HudVariantColorSyncer>();
            _bgCol.ColorMultiplier = 0.2f;
            _fgCol = _fg.gameObject.GetOrAddComponent<HudVariantColorSyncer>();
            if (FistControl.Instance != null) FistControl.Instance.FistIconUpdated += UpdateIcon;
            UpdateIcon();
        }

        private void OnDestroy() {
            if (FistControl.Instance != null) FistControl.Instance.FistIconUpdated -= UpdateIcon;
        }

        private void Update() {
            if (_fg == null || WeaponCharges.Instance == null) return;
            var newFill = WeaponCharges.Instance.punchStamina / 2f;
            if (!Mathf.Approximately(newFill, _fg.fillAmount)) _fg.fillAmount = newFill;
        }

        private void UpdateIcon() {
            UpdateIcon(PlayerPrefs.GetInt("CurArm", 0));
        }

        private void UpdateIcon(int fistVariant) {
            // 0 = Feedbacker, 1 = Knuckleblaster
            if (fistVariant != 0 && fistVariant != 1) return;
            var colVariant = fistVariant * 2; // 0 -> 0, 1 -> 2
            if (_bgCol != null) _bgCol.Variation = colVariant;
            if (_fgCol != null) _fgCol.Variation = colVariant;
            if (_bg == null || _fg == null) return;
            var iconName = fistVariant == 1 ? "knuckleblaster" : "feedbacker";
            _bg.sprite = AssetManager.Get<Sprite>(HudManager.BundleKey, iconName);
            _fg.sprite = AssetManager.Get<Sprite>(HudManager.BundleKey, iconName);
        }
    }
}
