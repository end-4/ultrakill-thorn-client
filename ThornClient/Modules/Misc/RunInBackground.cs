using NukeLib.Utils;
using ThornClient.Core;
using ThornClient.Core.ConfigurableElements;
using ThornClient.Managers;
using ThornClient.System;
using UnityEngine;

namespace ThornClient.Modules.Misc;

/// <summary>
/// Module that disables score submissions
/// </summary>
public class RunInBackground : Module {
    public override Sprite Icon => AssetManager.Get<Sprite>(ClickGUI.BundleKey, "window_play_background");

    /// <inheritdoc />
    public override string[] Tags => ["inactive", "afk", "background", "naruto"];

    public Setting<bool> OnlyWhenPaused;

    /// <inheritdoc />
    public RunInBackground() : base("thorn.runInBackground", "Run in Background",
        "Keeps the game running when the window is not focused",
        ModuleCategory.Misc) {
        OnlyWhenPaused = CreateSetting("onlyWhenPaused", "Only when paused/not in game", "Useful for example when waiting for an Angry download", false);
    }

    /// <inheritdoc />
    protected override void OnEnable() {
        Application.runInBackground = true;
        OnlyWhenPaused.OnChanged += UpdateBackgroundRunning;
        SceneUtils.SafeSceneLoadedNoParam += UpdateBackgroundRunning;
    }

    /// <inheritdoc />
    protected override void OnDisable() {
        OnlyWhenPaused.OnChanged -= UpdateBackgroundRunning;
        SceneUtils.SafeSceneLoadedNoParam -= UpdateBackgroundRunning;
        Application.runInBackground = false;
    }

    private bool IsInMenus() {
        return !SceneUtils.IsInGame() || (OptionsManager.Instance?.paused ?? true);
    }

    private void UpdateBackgroundRunning() {
        Application.runInBackground = IsEnabled && (!OnlyWhenPaused.Value || IsInMenus());
    }
}
