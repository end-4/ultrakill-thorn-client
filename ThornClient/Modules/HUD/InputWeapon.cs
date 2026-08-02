using System;
using NukeLib.UI;
using NukeLib.Utils;
using ThornClient.Core.ConfigurableElements;
using ThornClient.Core.DataTypes;
using ThornClient.Managers;
using UnityEngine;
using ThornClient.HUD;
using ThornClient.HUD.HUDComponents;
using ThornClient.Modules.Player;
using ThornClient.System;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ThornClient.Modules.HUD;

public class InputWeapon : FramedHudModule {
    public override Sprite Icon => AssetManager.Get<Sprite>(ClickGUI.BundleKey, "weapon_wheel");
    public override string[] Tags => ["keyboard", "mouse", "weapon", "controller"];

    public InputWeapon() : base("thorn.inputWeapon", "Input - Weapons",
        "Shows weapon switching inputs. Variant binds will only show for Thorn's Weapon Variant Binds, not for the standalone mod.") {
    }

    protected override GameObject CreateContentObject() {
        var obj = Object.Instantiate(AssetManager.Get<GameObject>(HudManager.BundleKey, "KeyLayoutWeapon"));
        obj.AddComponent<InputWeaponController>();
        return obj;
    }

    private class InputWeaponController : MonoBehaviour {
        private void HookButton(string path, int index) {
            var btn = gameObject.FindRecursive(path);
            if (btn == null) return;
            btn.AddComponent<WeaponKeyController>().WeaponIndex = index;
        }

        private void Start() {
            for (int i = 0; i < 5; i++) {
                HookButton($"{i}/KeyWeapon", i);
            }
        }
    }

    private class WeaponKeyController : MonoBehaviour {
        public int WeaponIndex = 0;
        private Image? _bg;
        private Image? _fg;
        private InputAction? _vanillaInputAction;
        private bool[] _variantHeld = [false, false, false];

        private void Start() {
            _bg = gameObject.GetComponent<Image>();
            _fg = gameObject.FindRecursive("Image")?.GetComponent<Image>();
            var thornVariantBinds = WeaponVariantBinds.Instance?.Binds;
            if (thornVariantBinds != null) {
                thornVariantBinds[WeaponIndex, 0].OnPress += OnPressVariant0;
                thornVariantBinds[WeaponIndex, 0].OnRelease += OnReleaseVariant0;
                thornVariantBinds[WeaponIndex, 1].OnPress += OnPressVariant1;
                thornVariantBinds[WeaponIndex, 1].OnRelease += OnReleaseVariant1;
                thornVariantBinds[WeaponIndex, 2].OnPress += OnPressVariant2;
                thornVariantBinds[WeaponIndex, 2].OnRelease += OnReleaseVariant2;
            }
        }

        private void OnDestroy() {
            var thornVariantBinds = WeaponVariantBinds.Instance?.Binds;
            if (thornVariantBinds != null) {
                thornVariantBinds[WeaponIndex, 0].OnPress -= OnPressVariant0;
                thornVariantBinds[WeaponIndex, 0].OnRelease -= OnReleaseVariant0;
                thornVariantBinds[WeaponIndex, 1].OnPress -= OnPressVariant1;
                thornVariantBinds[WeaponIndex, 1].OnRelease -= OnReleaseVariant1;
                thornVariantBinds[WeaponIndex, 2].OnPress -= OnPressVariant2;
                thornVariantBinds[WeaponIndex, 2].OnRelease -= OnReleaseVariant2;
            }
        }

        private void OnPressVariant0() { _variantHeld[0] = true; }
        private void OnReleaseVariant0() { _variantHeld[0] = false; }
        private void OnPressVariant1() { _variantHeld[1] = true; }
        private void OnReleaseVariant1() { _variantHeld[1] = false; }
        private void OnPressVariant2() { _variantHeld[2] = true; }
        private void OnReleaseVariant2() { _variantHeld[2] = false; }

        private bool IsVanillaSlotHeld() {
            var inputSource = MonoSingleton<InputManager>.Instance?.InputSource;
            if (inputSource != null) {
                _vanillaInputAction = WeaponIndex switch {
                    0 => inputSource.Actions.Weapon.Revolver,
                    1 => inputSource.Actions.Weapon.Shotgun,
                    2 => inputSource.Actions.Weapon.Nailgun,
                    3 => inputSource.Actions.Weapon.Railcannon,
                    4 => inputSource.Actions.Weapon.RocketLauncher,
                    5 => inputSource.Actions.Weapon.SpawnerArm,
                    _ => null
                };
            }
            return _vanillaInputAction != null && _vanillaInputAction.IsPressed();
        }

        private void Update() {
            for (int i = 0; i < _variantHeld.Length; i++) {
                if (!_variantHeld[i]) continue;
                var baseColor = ColorUtils.GetWeaponVariantColor(i);
                var iconColor = baseColor.GetContrastedColor();
                UpdateColors(baseColor, iconColor, true);
                return;
            }
            if (IsVanillaSlotHeld()) UpdateColors(Color.white, Color.black, true);
            else UpdateColors(Color.white, Color.white, false);
        }

        private void UpdateColors(Color baseColor, Color iconColor, bool baseFill) {
            var targetBaseSprite = AssetManager.Get<Sprite>(HudManager.BundleKey, baseFill ? "Round_FillLarge" : "Round_BorderLarge");
            if (_bg != null && _bg.sprite != targetBaseSprite) _bg.sprite = targetBaseSprite;
            if (_bg != null && _bg.color != baseColor) _bg.color = baseColor;
            if (_fg != null && _fg.color != iconColor) _fg.color = iconColor;
        }
    }
}
