using System;
using NukeLib.Game;
using NukeLib.Utils;
using UnityEngine;
using ThornClient.Core;
using ThornClient.Core.ConfigurableElements;
using ThornClient.Managers;
using ThornClient.System;
using Object = UnityEngine.Object;

namespace ThornClient.Modules.Enemy;

/// <summary>
/// Module that Multiplies enemies
/// </summary>
public class Multiply : Module {
    /// <inheritdoc />
    public override Sprite Icon => AssetManager.Get<Sprite>(ClickGUI.BundleKey, "copy");

    /// <inheritdoc />
    public override string[] Tags => ["buff", "mitosis", "duplicate", "many"];

    /// <summary>
    /// Multiply to how many enemies
    /// </summary>
    public Setting<int> Multiplier { get; }

    public Multiply() : base("thorn.multiplyEnemies", "Multiply",
        "Love me some Pain Atrophy (makes enemies duplicate)",
        ModuleCategory.Enemy) {
        Multiplier = CreateSetting("multiplier", "How many?", "1 -> x, where x=", 2);
        Multiplier.Hints = new InterfaceHints {
            Range = Tuple.Create(2f, 10f)
        };
    }

    /// <summary>
    /// Why this is cheaty
    /// </summary>
    public override string? CheatReason => IsEnabled ? "Enables non-standard gameplay" : "";

    /// <inheritdoc />
    protected override void OnEnable() {
        CheatManager.UpdateCheatiness();
        EnemyEvents.OnSpawn += Dupe;
        SceneUtils.SafeSceneLoadedNoParam += AddInfoLine;
        AddInfoLine();
    }

    /// <inheritdoc />
    protected override void OnDisable() {
        EnemyEvents.OnSpawn -= Dupe;
        SceneUtils.SafeSceneLoadedNoParam -= AddInfoLine;
        RemoveInfoLine();
    }

    private static string GetInfoLine(int multiplier) {
        return $"<color=#fff>+ <color=orange>ENEMIES x{multiplier}";
    }

    private string? _registeredInfoLine = null;

    private void AddInfoLine() {
        RemoveInfoLine();
        _registeredInfoLine = GetInfoLine(Multiplier.Value);
        FinalRankHelper.AddInfoLine(_registeredInfoLine);
    }

    private void RemoveInfoLine() {
        if (_registeredInfoLine != null && FinalRankHelper.ExtraInfoLines.Contains(_registeredInfoLine)) {
            FinalRankHelper.RemoveInfoLine(_registeredInfoLine);
            _registeredInfoLine = null;
        }
    }

    private const string MultipliedTag = "[ThornMultiplied]";

    private void Dupe(EnemyIdentifier eid) {
        if (eid.gameObject.name.Contains(MultipliedTag)) return;

        for (int i = 0; i < Multiplier.Value - 1; i++) {
            var dupe = Object.Instantiate(eid.gameObject, eid.transform.parent);
            dupe.name = eid.gameObject.name + " " + MultipliedTag;
            dupe.transform.position = eid.transform.position;
        }
    }
}
