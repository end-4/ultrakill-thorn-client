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
public class Label : ContinuousCumulatedStatHudModule {
    /// <summary>
    /// Setting: whether to show "FPS" text on the HUD element
    /// </summary>
    public Setting<string> CustomText;

    /// <summary>
    /// Icon of the module
    /// </summary>
    public override Sprite Icon => AssetManager.Get<Sprite>(ClickGUI.BundleKey, "cube");

    /// <summary>
    /// Tags for search
    /// </summary>
    public override string[] Tags => ["bonus"];

    /// <summary>
    /// Constructor
    /// </summary>
    public Label() : base("thorn.label", "Label", "Shows a custom text") {
        CustomText = CreateSetting("customText", "Text",
            "Adds an text to the HUD", "placeholder (change this in settings!)");
    }

    /// <inheritdoc />
    protected override float CollectUpdate() {
        return 1f;
    }

    /// <inheritdoc />
    protected override string FormatStat(float value) {
        return CustomText.Value;
    }
}
