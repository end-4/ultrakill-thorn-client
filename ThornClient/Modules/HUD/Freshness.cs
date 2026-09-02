using System.Collections.Generic;
using NukeLib.Utils;
using ThornClient.Core.ConfigurableElements;
using ThornClient.HUD;
using ThornClient.Managers;
using ThornClient.System;
using UnityEngine;

namespace ThornClient.Modules.HUD;

public class Freshness : BoundedValueHudModule {
    public enum BarDisplayMode {
        CurrentTier,
        Full,
        StyleMultiplier
    }

    public static float MaxFreshness = 10f;

    /// <inheritdoc />
    public override Sprite Icon => AssetManager.Get<Sprite>(ClickGUI.BundleKey, "compass");

    /// <inheritdoc />
    public override string[] Tags => ["style", "gun swap"];

    public Setting<BarDisplayMode> BarDisplay;

    /// <inheritdoc />
    public Freshness() : base("thorn.freshnessHud", "Freshness", "Shows freshness", MaxFreshness,
        defaultValueColor: 0xE7C900.ToColor()) {
        BarDisplay = CreateSetting("barDisplay", "Bar display mode", "What the progress bar should show",
            BarDisplayMode.CurrentTier);
        BarDisplay.Hints = new InterfaceHints {
            EnumSubstitutions = new Dictionary<string, string> {
                ["CurrentTier"] = "Current tier",
                ["Full"] = "Full",
                ["StyleMultiplier"] = "Style multiplier"
            }
        };
    }

    public override void OnUpdate() {
        if (!SceneUtils.IsInGame()) return;
        var shud = StyleHUD.Instance;
        var gc = GunControl.Instance;
        if (shud == null || gc == null || gc.currentWeapon == null) return;
        var rawFreshness = shud.GetFreshness(gc.currentWeapon);
        var state = shud.GetFreshnessState(gc.currentWeapon);
        var stateData = shud.freshnessStateData.Find(data => data.state == state);

        DisplayName = $"{state.ToString()} [{stateData.scoreMultiplier}x]";
        switch (BarDisplay.Value) {
            case BarDisplayMode.Full:
                Value = rawFreshness;
                Bound = MaxFreshness;
                break;
            case BarDisplayMode.CurrentTier:
                Value = rawFreshness - stateData.min;
                Bound = stateData.max - stateData.min;
                break;
            case BarDisplayMode.StyleMultiplier:
                Value = stateData.scoreMultiplier;
                Bound = shud.freshnessStateData[^1].scoreMultiplier; // Should be 1.5
                break;
        }
    }
}
