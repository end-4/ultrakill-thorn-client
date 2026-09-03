using System.Reflection;
using NukeLib.Utils;
using ThornClient.HUD;
using ThornClient.Managers;
using ThornClient.System;
using UnityEngine;

namespace ThornClient.Modules.HUD;

/// <summary>
/// HUD module that shows style multiplier
/// </summary>
public class StylePoints : BoundedValueHudModule {
    private static readonly FieldInfo? CurrentMeterField =
        typeof(StyleHUD).GetField("currentMeter", BindingFlags.NonPublic | BindingFlags.Instance);

    /// <inheritdoc />
    public override Sprite Icon => AssetManager.Get<Sprite>(ClickGUI.BundleKey, "style_rank_up");

    /// <inheritdoc />
    public override string[] Tags => ["decay"];

    /// <inheritdoc />
    public StylePoints() : base("thorn.stylePoints", "Style Points",
        "Shows points on the current style rank",
        defaultValueColor: 0xFFFFFF.ToColor(), displayName: "Current rank") {
    }

    /// <inheritdoc />
    public override void OnUpdate() {
        var shud = StyleHUD.Instance;
        if (shud == null || CurrentMeterField == null) return;

        float currentMeter = (float)CurrentMeterField.GetValue(shud);
        Value = Mathf.Max(0, currentMeter);

        if (shud.currentRank != null) Bound = shud.currentRank.maxMeter;
    }
}
