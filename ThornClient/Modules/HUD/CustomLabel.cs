using System;
using ThornClient.Core.ConfigurableElements;
using ThornClient.Managers;
using ThornClient.System;
using UnityEngine;
using ThornClient.HUD;

namespace ThornClient.Modules.HUD;

/// <summary>
/// Module that shows a custom text.
/// </summary>
public class CustomLabel : TextHudModule {
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
    public CustomLabel() : base("thorn.customLabel", "Custom Label", "Shows a custom text") {
        CustomText = CreateSetting("customText", "Text",
            "Adds an text to the HUD", "placeholder (change this in settings!)");
        CustomText.OnChanged += ChangeText;
    }

    /// <summary>
    /// Changes the value of the label.
    /// </summary>
    public void ChangeText() {
        Text = CustomText.Value;
    }

    protected override void OnHudModuleEnable() {
        ChangeText();
    }
}
