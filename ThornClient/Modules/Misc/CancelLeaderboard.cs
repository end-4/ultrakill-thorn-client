using ThornClient.Core;
using ThornClient.Managers;
using ThornClient.System;
using UnityEngine;

namespace ThornClient.Modules.Misc;

/// <summary>
/// Module that disables score submissions
/// </summary>
public class CancelLeaderboard : Module {
    /// <summary>
    /// Icon of the module
    /// </summary>
    public override Sprite Icon => AssetManager.Get<Sprite>(ClickGUI.BundleKey, "no_trophy");
    /// <summary>
    /// Tags for search
    /// </summary>
    public override string[] Tags => ["cheat", "surrender", "fair play", "score", "rank"];
    /// <summary>
    /// Constructor
    /// </summary>
    public CancelLeaderboard() : base("thorn.cancelLeaderboard", "Cancel Leaderboard", "Voluntarily exclude scores from the leaderboard. Useful when you're using cheaty mods that don't do this themselves", ModuleCategory.Misc) {
    }

    /// <summary>
    /// Why this disables leaderboard
    /// </summary>
    public override string CheatReason => IsEnabled ? "Obviously" : "";

    /// <summary>
    /// Stuff run when this module is enabled
    /// </summary>
    protected override void OnEnable() {
        CheatManager.UpdateCheatiness();
    }
}
