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
public class RestartShortcut : Module {
    /// <summary>
    /// The FOV that is applied when zooming
    /// </summary>
    public Setting<Keybind> RestartKeybind { get; }
    public Setting<Keybind> RestartCheckpoint { get; }

    /// <summary>
    /// Icon of the module
    /// </summary>
    public override Sprite Icon => AssetManager.Get<Sprite>(ClickGUI.BundleKey, "dash");

    /// <summary>
    /// Tags for search
    /// </summary>
    public override string[] Tags => ["utility", "speedrun"];

    /// <summary>
    /// Constructor
    /// </summary>
    public RestartShortcut() : base("thorn.extraKeybinds", "Extra keybinds",
        "Some useful keybinds for (almost) everything! (right now its only speedruns things)", ModuleCategory.Utility) {
        RestartKeybind = CreateSetting("restartMission", "Restart mission.", "yo what do you think it does :sob:",
            new Keybind(KeyCode.H));
        RestartKeybind.OnPress += () => OptionsManager.Instance?.RestartMission();

        RestartCheckpoint = CreateSetting("restartToCheckpoint", "Go to checkpoint.", "yo what do you think it does :sob:",
            new Keybind(KeyCode.B));
        RestartCheckpoint.OnPress += () => OptionsManager.Instance?.RestartCheckpoint();
    }
}
