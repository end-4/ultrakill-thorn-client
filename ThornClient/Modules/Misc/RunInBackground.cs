using ThornClient.Core;
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

    /// <inheritdoc />
    public RunInBackground() : base("thorn.runInBackground", "Run in Background",
        "Keeps the game running when the window is not focused",
        ModuleCategory.Misc) {
    }

    /// <inheritdoc />
    protected override void OnEnable() {
        Application.runInBackground = true;
    }

    /// <inheritdoc />
    protected override void OnDisable() {
        Application.runInBackground = false;
    }
}
