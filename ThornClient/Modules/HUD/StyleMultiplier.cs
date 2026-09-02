using NukeLib.Utils;
using ThornClient.HUD;
using ThornClient.Managers;
using ThornClient.System;
using UnityEngine;

namespace ThornClient.Modules.HUD;

/// <summary>
/// HUD module that shows style multiplier
/// </summary>
public class StyleMultiplier : BoundedValueHudModule {
    /// <inheritdoc />
    public override Sprite Icon => AssetManager.Get<Sprite>(ClickGUI.BundleKey, "style");

    /// <inheritdoc />
    public override string[] Tags => ["style", "sick ass moves", "air time"];

    /// <inheritdoc />
    public StyleMultiplier() : base("thorn.styleMultiplier", "Style Multiplier",
        "Shows the multiplier for air/slide time", 3,
        defaultValueColor: 0xff0000.ToColor(), displayName: "Style multiplier") {
    }

    public override void OnUpdate() {
        if (!SceneUtils.IsInGame()) return;
        var scal = StyleCalculator.Instance;
        if (scal == null) return;
        Value = scal.airTime;
    }
}
