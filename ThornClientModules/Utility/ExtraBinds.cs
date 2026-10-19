using UnityEngine;
using ThornClient.Core;
using ThornClient.Core.DataTypes;
using ThornClient.Core.ConfigurableElements;
using ThornClient.Managers;
using ThornClient.System;

namespace ThornClientModules.Utility;

/// <summary>
/// Use this shortcut to restart your mission. Good for speedruns.
/// </summary>
public class ExtraBinds : Module {
    /// <summary>
    /// The settings for the keybinds
    /// </summary>
    public Setting<Keybind> RestartKeybind { get; }
    public Setting<Keybind> QuitKeybind { get; }

    public Setting<Keybind> NextlevelKeybind { get; }
    public Setting<Keybind> PreviouslevelKeybind { get; }

    public Setting<Keybind> CheckpointKeybind { get; }
    public Setting<Keybind> AltPauseKeybind { get; }

    /// <summary>
    /// Icon of the module
    /// </summary>
    public override Sprite Icon => AssetManager.Get<Sprite>(ClickGUI.BundleKey, "keyboard");

    /// <summary>
    /// Tags for search
    /// </summary>
    public override string[] Tags => ["utility", "speedrun", "restart", "checkpoint", "pause"];

    /// <summary>
    /// Constructor
    /// </summary>
    public ExtraBinds() : base("thorn.extraBinds", "Extra Binds",
        "Some useful keybinds for (almost) everything", ModuleCategory.Utility) {
        RestartKeybind = CreateSetting("restartMission", "Restart mission", "Keybind to restart mission",
            new Keybind(KeyCode.None));
        QuitKeybind = CreateSetting("quitMission", "Quit mission", "Quits the mission",
            new Keybind(KeyCode.None));
        NextlevelKeybind = CreateSetting("nextLevelKeybind", "Next level", "Changes the level to the next one",
            new Keybind(KeyCode.None));
        PreviouslevelKeybind = CreateSetting("previousLevelKeybind", "Previous level", "Changes the level to the previous one",
            new Keybind(KeyCode.None));
        CheckpointKeybind = CreateSetting("restartToCheckpoint", "Checkpoint keybind", "Goes to the latest checkpoint",
            new Keybind(KeyCode.None));
        AltPauseKeybind = CreateSetting("pauseKey", "Alternative pause", "Pauses the game",
            new Keybind(KeyCode.None));
    }

    /// <inheritdoc />
    protected override void OnEnable() {
        RestartKeybind.OnPress += Restart;
        QuitKeybind.OnPress += QuitMission;
        NextlevelKeybind.OnPress += NextLevel;
        PreviouslevelKeybind.OnPress += PreviousLevel;
        CheckpointKeybind.OnPress += Checkpoint;
        AltPauseKeybind.OnPress += TogglePause;
    }

    /// <inheritdoc />
    protected override void OnDisable() {
        RestartKeybind.OnPress -= Restart;
        QuitKeybind.OnPress -= QuitMission;
        NextlevelKeybind.OnPress -= NextLevel;
        PreviouslevelKeybind.OnPress -= PreviousLevel;
        CheckpointKeybind.OnPress -= Checkpoint;
        AltPauseKeybind.OnPress -= TogglePause;
    }

    private OptionsManager? opts => OptionsManager.Instance;

    private void Restart() {
        opts?.RestartMission();
    }

    private void QuitMission() {
        opts?.QuitMission();
    }

    private void NextLevel() {
        if (opts == null) return;
        IncrementLevel(1);
    }

    private void PreviousLevel() {
        if (opts == null) return;
        IncrementLevel(-1);
    }

    private void IncrementLevel(int diff) {
        var nextLevel = SceneHelper.CurrentLevelNumber + diff;
        var name = GetMissionName.GetSceneName(nextLevel);

        opts?.ChangeLevel(name);
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
