using System;
using ThornClient.Core.ConfigurableElements;
using ThornClient.Managers;
using ThornClient.System;
using UnityEngine;
using ThornClient.HUD;

namespace ThornClient.Modules.HUD;

/// <summary>
/// Module that shows an custom text.
/// </summary>
public class Label : TextHudModule {
    /// <summary>
    /// Setting: The custom text that will appear on the HUD
    /// </summary>
    public Setting<string> CustomText;

    /// <summary>
    /// Icon of the module
    /// </summary>
    public override Sprite Icon => AssetManager.Get<Sprite>(ClickGUI.BundleKey, "cube");

    /// <summary>
    /// Tags for search
    /// </summary>
    public override string[] Tags => ["text"];

    /// <summary>
    /// Constructor
    /// </summary>
    public Label() : base("thorn.customLabel", "Custom Label", "Shows a custom text") {
        CustomText = CreateSetting("customText", "Text",
            "Adds an text to the HUD", "placeholder (change this in settings!)");
    }

    /// <summary>
    /// Changes the value of the label.
    /// </summary>
    public override void OnUpdate() {
        Text = CustomText.Value;
    }
}
