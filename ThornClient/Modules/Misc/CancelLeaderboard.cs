using ThornClient.Core;
using ThornClient.Managers;
using ThornClient.System;
using UnityEngine;

namespace ThornClient.Modules.Misc;

/// <summary>
/// Module that disables score submissions
/// </summary>
public class CancelLeaderboard : Module {
    /// <inheritdoc />
    public override Sprite Icon => AssetManager.Get<Sprite>(ClickGUI.BundleKey, "no_trophy");

    /// <inheritdoc />
    public override string[] Tags => ["cheat", "surrender", "fair play", "score", "rank"];

    /// <inheritdoc />
    public CancelLeaderboard() : base("thorn.cancelLeaderboard", "Cancel Leaderboard",
        "Voluntarily exclude scores from the leaderboard. Useful when you're using cheaty mods that don't do this themselves",
        ModuleCategory.Misc) {
    }

    /// <inheritdoc />
    public override string CheatReason => IsEnabled ? "Obviously" : "";

    /// <inheritdoc />
    protected override void OnEnable() {
        CheatManager.UpdateCheatiness();
    }
}
