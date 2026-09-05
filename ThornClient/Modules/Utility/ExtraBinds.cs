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
    public Setting<Keybind> AltPauseKeybind { get; }

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
        RestartKeybind = CreateSetting("restartMission", "Restart mission", "Keybind to restart mission",
            new Keybind(KeyCode.None));
        CheckpointKeybind = CreateSetting("restartToCheckpoint", "Checkpoint Keybind", "Goes to the latest checkpoint",
            new Keybind(KeyCode.None));
        AltPauseKeybind = CreateSetting("pauseKey", "Alternative pause key", "Pauses the game",
            new Keybind(KeyCode.None));
    }

    /// <inheritdoc />
    protected override void OnEnable() {
        RestartKeybind.OnPress += Restart;
        CheckpointKeybind.OnPress += Checkpoint;
        AltPauseKeybind.OnPress += TogglePause;
    }

    /// <inheritdoc />
    protected override void OnDisable() {
        RestartKeybind.OnPress -= Restart;
        CheckpointKeybind.OnPress -= Checkpoint;
        AltPauseKeybind.OnPress -= TogglePause;
    }

    private OptionsManager? opts => OptionsManager.Instance;

    private void Restart() {
        opts?.RestartMission();
    }

    private void Checkpoint() {
        opts?.RestartCheckpoint();
    }

    private void TogglePause() {
        if (opts == null) return;
        if (opts.paused) opts.UnPause();
        else opts.Pause();
    }
}
