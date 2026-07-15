using System;
using System.Collections.Generic;
using UnityEngine;
using ThornClient.Core;
using ThornClient.Managers;
using ThornClient.Core;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace ThornClient.Modules.Enemy;

public class ForceSand : Module {
    public override string IconName => "sand";
    public ForceSand() : base("Force Sand", "Makes enemies sanded, removing bleeding. You'll have to rely on parries for healing.", ModuleCategory.Enemy) {
    }

    public override string? CheatReason => IsEnabled ? "Enables non-standard gameplay" : "";

    protected override void OnEnable() {
        UpdateSand(true);
        CheatManager.UpdateCheatiness();
    }

    protected override void OnDisable() {
        UpdateSand(false);
    }

    private void UpdateSand(bool enabled) {
        OptionsManager.forceSand = enabled;

        foreach(EnemyIdentifier enemy in Object.FindObjectsOfType<EnemyIdentifier>()) {
            enemy.UpdateBuffs();
        }
    }
}
