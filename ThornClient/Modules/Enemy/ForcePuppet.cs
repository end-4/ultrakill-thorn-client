using UnityEngine;
using ThornClient.Core;
using ThornClient.Managers;
using ThornClient.System;
using Object = UnityEngine.Object;

namespace ThornClient.Modules.Enemy;

/// <summary>
/// A module that forces enemies to be puppeted.
/// </summary>
public class ForcePuppet : Module {
    /// <summary>
    /// The icon
    /// </summary>
    public override Sprite Icon => AssetManager.Get<Sprite>(ClickGUI.BundleKey, "puppet");
    /// <summary>
    /// Tags for searching
    /// </summary>
    public override string[] Tags => ["blood", "zombie"];
    /// <summary>
    /// Constructor
    /// </summary>
    public ForcePuppet() : base("thorn.forcePuppet", "Force Puppet", "Makes enemies puppeted (become blood bois)\nWarning: This breaks Cybergrind", ModuleCategory.Enemy) {
    }

    /// <summary>
    /// Why this module is considered a cheat
    /// </summary>
    public override string? CheatReason => IsEnabled ? "Enables non-standard gameplay" : "";

    /// <summary>
    /// Called when the module is enabled
    /// </summary>
    protected override void OnEnable() {
        UpdatePuppet(true);
        CheatManager.UpdateCheatiness();
    }

    /// <summary>
    /// Called when the module is disabled
    /// </summary>
    protected override void OnDisable() {
        UpdatePuppet(false);
    }

    private void UpdatePuppet(bool enabled) {
        OptionsManager.forcePuppet = enabled;

        foreach(EnemyIdentifier enemy in Object.FindObjectsOfType<EnemyIdentifier>()) {
            enemy.UpdateBuffs();
        }
    }
}
