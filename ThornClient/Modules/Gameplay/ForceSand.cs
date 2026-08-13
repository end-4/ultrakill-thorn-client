using NukeLib.Game;
using NukeLib.Utils;
using UnityEngine;
using ThornClient.Core;
using ThornClient.Managers;
using ThornClient.System;
using Object = UnityEngine.Object;

namespace ThornClient.Modules.Gameplay;

/// <summary>
/// Module that forces enemies to be sanded.
/// </summary>
public class ForceSand : Module {
    /// <summary>
    /// Icon of the module
    /// </summary>
    public override Sprite Icon => AssetManager.Get<Sprite>(ClickGUI.BundleKey, "sand");
    /// <summary>
    /// Tags for searching
    /// </summary>
    public override string[] Tags => ["buff", "bleed"];
    public ForceSand() : base("thorn.forceSand", "Force Sand", "Makes enemies sanded, removing bleeding. You'll have to rely on parries for healing.", ModuleCategory.Gameplay) {
    }

    /// <summary>
    /// Why this is cheaty
    /// </summary>
    public override string? CheatReason => IsEnabled ? "Enables non-standard gameplay" : "";

    /// <summary>
    /// Run when enabled
    /// </summary>
    protected override void OnEnable() {
        UpdateSand(true);
        CheatManager.UpdateCheatiness();
        SceneUtils.SafeSceneLoadedNoParam += AddInfoLine;
        AddInfoLine();
    }

    /// <summary>
    /// RUn when disabled
    /// </summary>
    protected override void OnDisable() {
        UpdateSand(false);
        SceneUtils.SafeSceneLoadedNoParam -= AddInfoLine;
        FinalRankHelper.RemoveInfoLine(InfoLine);
    }

    private static string InfoLine = "<color=#fff>+ <color=#fffaa1>DESERTED</color>";
    private void AddInfoLine() {
        FinalRankHelper.AddInfoLine(InfoLine);
    }

    private void UpdateSand(bool enabled) {
        OptionsManager.forceSand = enabled;

        foreach(EnemyIdentifier enemy in Object.FindObjectsOfType<EnemyIdentifier>()) {
            enemy.UpdateBuffs();
        }
    }
}
