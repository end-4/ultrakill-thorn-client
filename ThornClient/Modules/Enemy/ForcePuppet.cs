using System;
using System.Collections.Generic;
using UnityEngine;
using ThornClient.Core;
using ThornClient.Managers;
using ThornClient.Core;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace ThornClient.Modules.Enemy;

public class ForcePuppet : Module {
    public override string IconName => "puppet";
    public ForcePuppet() : base("Force Puppet", "Makes enemies puppeted (become blood bois)", ModuleCategory.Enemy) {
    }

    public override string? CheatReason => IsEnabled ? "Enables non-standard gameplay" : "";

    protected override void OnEnable() {
        UpdatePuppet(true);
        CheatManager.UpdateCheatiness();
    }

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
