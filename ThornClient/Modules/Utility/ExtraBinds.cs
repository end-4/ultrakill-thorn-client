using UnityEngine;
using ThornClient.Core;
using ThornClient.Core.DataTypes;
using ThornClient.Core.ConfigurableElements;
using ThornClient.Managers;
using ThornClient.System;

namespace ThornClient.Modules.Utility;

/// <summary>
/// Use this shortcut to restart your mission. Good for speedruns.
/// </summary>
public class ExtraBinds : Module {
    /// <summary>
    /// The settings for the keybinds
    /// </summary>
    public Setting<Keybind> RestartKeybind { get; }
    public Setting<Keybind> CheckpointKeybind { get; }

    /// <summary>
    /// Icon of the module
    /// </summary>
    public override Sprite Icon => AssetManager.Get<Sprite>(ClickGUI.BundleKey, "plus");

    /// <summary>
    /// Tags for search
    /// </summary>
    public override string[] Tags => ["utility", "speedrun"];

    /// <summary>
    /// Constructor
    /// </summary>
    public ExtraBinds() : base("thorn.extraBinds", "Extra Binds",
        "Some useful keybinds for (almost) everything", ModuleCategory.Utility) {
        RestartKeybind = CreateSetting("restartMission", "Restart mission", "yo what do you think it does :sob:",
            new Keybind(KeyCode.None));
        RestartKeybind.OnPress += () => OptionsManager.Instance?.RestartMission();

        CheckpointKeybind = CreateSetting("restartToCheckpoint", "Goes to the latest checkpoint", "yo what do you think it does :sob:",
            new Keybind(KeyCode.None));
        CheckpointKeybind.OnPress += () => OptionsManager.Instance?.RestartCheckpoint();
    }
}
