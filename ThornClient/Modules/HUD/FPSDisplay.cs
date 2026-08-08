using System;
using ThornClient.Core.ConfigurableElements;
using ThornClient.Managers;
using ThornClient.System;
using UnityEngine;
using ThornClient.HUD;

namespace ThornClient.Modules.HUD;

/// <summary>
/// Module that shows FPS
/// </summary>
public class FPSDisplay : ContinuousCumulatedStatHudModule {
    /// <summary>
    /// Setting: whether to show "FPS" text on the HUD element
    /// </summary>
    public Setting<bool> ShowFpsText;

    /// <summary>
    /// Icon of the module
    /// </summary>
    public override Sprite Icon => AssetManager.Get<Sprite>(ClickGUI.BundleKey, "fps");

    /// <summary>
    /// Tags for search
    /// </summary>
    public override string[] Tags => ["performance", "frame"];

    /// <summary>
    /// Constructor
    /// </summary>
    public FPSDisplay() : base("thorn.fpsDisplay", "FPS", "Shows framerate") {
        ShowFpsText = CreateSetting("showFpsText", "Show \"FPS\" text",
            "Makes the indicator say \"FPS:60\" instead of \"60\"", true);
    }

    protected override float CollectUpdate() {
        return 1f;
    }

    protected override string FormatStat(float value) {
        string prefix = ShowFpsText.Value ? "FPS:" : "";
        return $"{prefix}{Math.Round(value)}";
    }
}
