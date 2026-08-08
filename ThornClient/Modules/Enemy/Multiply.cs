using NukeLib.Game;
using UnityEngine;
using ThornClient.Core;
using ThornClient.Core.ConfigurableElements;
using ThornClient.Managers;
using ThornClient.System;
using Object = UnityEngine.Object;

namespace ThornClient.Modules.Enemy;

/// <summary>
/// Module that forces enemies to be sanded.
/// </summary>
public class Multiply : Module {
    /// <summary>
    /// Icon of the module
    /// </summary>
    public override Sprite Icon => AssetManager.Get<Sprite>(ClickGUI.BundleKey, "copy");

    /// <summary>
    /// Tags for searching
    /// </summary>
    public override string[] Tags => ["buff", "mitosis", "duplicate", "many"];

    public Setting<int> Multiplier { get; }

    public Multiply() : base("thorn.multiplyEnemies", "Multiply",
        "Love me some Pain Atrophy (makes enemies duplicate)",
        ModuleCategory.Enemy) {
        Multiplier = CreateSetting("multiplier", "How many?", "1 -> x, where x=", 2);
    }

    /// <summary>
    /// Why this is cheaty
    /// </summary>
    public override string? CheatReason => IsEnabled ? "Enables non-standard gameplay" : "";

    /// <inheritdoc />
    protected override void OnEnable() {
        CheatManager.UpdateCheatiness();
        EnemyEvents.OnSpawn += Dupe;
    }

    /// <inheritdoc />
    protected override void OnDisable() {
        EnemyEvents.OnSpawn -= Dupe;
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
