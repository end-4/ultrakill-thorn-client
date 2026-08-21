using System;
using System.Collections.Generic;
using System.Linq;
using NukeLib.Game;
using NukeLib.UI;
using NukeLib.Utils;
using ThornClient.Core.ConfigurableElements;
using ThornClient.HUD;
using ThornClient.HUD.HUDComponents;
using ThornClient.Managers;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ThornClient.Modules.HUD;

public class HammerStats : FramedHudModule {
    public override Sprite Icon => AssetManager.Get<Sprite>(HudManager.BundleKey, "jackhammer");
    public override string[] Tags => ["jackhammer", "alternative shotgun", "impact hammer"];

    public Setting<bool> ShowYellowHeatBar;
    public Setting<Color> YellowHeatBarColor;
    public Setting<Color> OverheatBarColor;

    /// <summary>
    /// Constructor
    /// </summary>
    public HammerStats() : base("thorn.hammerStats", "Hammer Stats", "Shows Jackhammer heat for individual variants and global cooldown") {
        ShowYellowHeatBar = CreateSetting("showYellowHeatBar", "Show global heat bar",
            "Shows the number of hits before the next one overheats any hammer", true);
        YellowHeatBarColor = CreateSetting("yellowHeatBarColor", "Heat bar color",
            "The color of the heat bar in its normal state", new Color(1f, 0.9f, 0.3f));
        OverheatBarColor = CreateSetting("overheatBarColor", "Overheat color",
            "The color of the heat bar in its normal state", new Color(1f, 0f, 0f));
    }

    private GameObject _contentObject;

    /// <summary>
    /// Creates the content object that goes on the frame.
    /// </summary>
    /// <returns>The content object</returns>
    protected override GameObject CreateContentObject() {
        _contentObject = Object.Instantiate(AssetManager.Get<GameObject>(HudManager.BundleKey, "HammerLayout"));
        var comp = _contentObject.GetOrAddComponent<HammerCooldownController>();
        comp.ParentModule = this;
        return _contentObject;
    }

    private static GunControl? gc => GunControl.Instance;
    private static WeaponCharges? wc => WeaponCharges.Instance;
    private static PrefsManager? prefs => PrefsManager.Instance;
    public static readonly int MaxYellowHits = 3;

    private class HammerCooldownController : MonoBehaviour {
        public HammerStats? ParentModule;
        private GameObject?[] _bgs = new GameObject[3];
        private GameObject?[] _fgs = new GameObject[3];
        private Image?[] _fgis = new Image[3];
        private HudVariantColorSyncer?[] _bgCols = new HudVariantColorSyncer?[3];
        private HudVariantColorSyncer?[] _fgCols = new HudVariantColorSyncer?[3];
        private int[] _variantMap = [0, 1, 2];
        private BatchBoolSettingVisibilitySyncer? _visibilitySyncer;
        private Slider? _heatBar;
        private Image? _heatFill;

        private void Start() {
            if (ParentModule == null) return;
            _visibilitySyncer = gameObject.GetOrAddComponent<BatchBoolSettingVisibilitySyncer>();
            _visibilitySyncer.SyncPairs = new Dictionary<Setting<bool>, string> {
                { ParentModule.ShowYellowHeatBar, "YellowHeatBar" }
            };
        }

        private void OnEnable() {
            for (int i = 0; i < 3; i++) {
                _bgs[i] = gameObject.FindRecursive($"Variants/{i}");
                _bgCols[i] = _bgs[i]?.GetOrAddComponent<HudVariantColorSyncer>();
                _fgs[i] = _bgs[i]?.FindRecursive("Fill");
                _fgCols[i] = _fgs[i]?.GetOrAddComponent<HudVariantColorSyncer>();
                _fgis[i] = _fgs[i]?.GetOrAddComponent<Image>();
                if (_bgCols[i] != null) _bgCols[i].ColorMultiplier = 0.2f;
            }

            _heatBar = gameObject.FindRecursive("YellowHeatBar")?.GetComponent<Slider>();
            _heatFill = gameObject.FindRecursive("YellowHeatBar/Fill Area/Fill")?.GetComponent<Image>();
            SyncVariantLayout();
            PrefsHelper.Subscribe("weapon.sho.order", SyncVariantLayout);
            PrefsHelper.Subscribe("weapon.sho0", SyncVariantLayout);
            PrefsHelper.Subscribe("weapon.sho1", SyncVariantLayout);
            PrefsHelper.Subscribe("weapon.sho2", SyncVariantLayout);
        }

        private void OnDisable() {
            PrefsHelper.Unsubscribe("weapon.sho.order", SyncVariantLayout);
            PrefsHelper.Unsubscribe("weapon.sho0", SyncVariantLayout);
            PrefsHelper.Unsubscribe("weapon.sho1", SyncVariantLayout);
            PrefsHelper.Unsubscribe("weapon.sho2", SyncVariantLayout);
        }

        private static float FullVariantHeatCooldown = 7f;
        private static float GlobalYellowCooldown = 3f;

        private void Update() {
            if (wc == null) return;

            for (int i = 0; i < 3; i++) {
                if (!_bgs[i]?.activeSelf ?? false) continue;
                float cooldown = wc.shoaltcooldowns[_variantMap[i]];
                float perc = 1f - cooldown / FullVariantHeatCooldown;
                if (_fgis[i] != null && !Mathf.Approximately(_fgis[i].fillAmount, perc)) {
                    _fgis[i].fillAmount = perc;
                }
            }

            if (ParentModule != null && ParentModule.ShowYellowHeatBar.Value && _heatBar != null && _heatFill != null) {
                var newVal = wc.shoAltYellows;
                bool overheat = newVal >= MaxYellowHits;
                float hitPerc = 0;
                if (overheat) {
                    if (!_heatFill.color.Approximately(ParentModule.OverheatBarColor.Value)) {
                        _heatFill.color = ParentModule.OverheatBarColor.Value;
                    }

                    hitPerc = wc.shoAltYellowsTimer / GlobalYellowCooldown;
                } else {
                    if (!_heatFill.color.Approximately(ParentModule.YellowHeatBarColor.Value)) {
                        _heatFill.color = ParentModule.YellowHeatBarColor.Value;
                    }

                    hitPerc = (float)newVal / (float)MaxYellowHits;
                }
                if (!Mathf.Approximately(hitPerc, _heatBar.value)) {
                    _heatBar.value = hitPerc;
                }
            }
        }

        private void SyncVariantLayout() {
            if (prefs == null) return;
            string order = prefs.GetString("weapon.sho.order", "1234").Replace("4", "");
            bool isThreeDigits = order?.Length == 3 && order.All(char.IsDigit);
            if (!isThreeDigits) order = "123";
            for (int i = 0; i < 3; i++) {
                var thisVariation = Math.Clamp(int.Parse($"{order[i]}") - 1, 0, 2);
                _variantMap[i] = thisVariation;
                if (_bgCols[i] != null) _bgCols[i].Variation = thisVariation;
                if (_fgCols[i] != null) _fgCols[i].Variation = thisVariation;
                bool isHammer = prefs.GetInt($"weapon.sho{thisVariation}", 0) == 2;
                if (_bgs[i].activeSelf != isHammer) {
                    _bgs[i]?.SetActive(isHammer);
                    gameObject.UnfuckLayoutHack();
                }
            }
        }
    }
}
