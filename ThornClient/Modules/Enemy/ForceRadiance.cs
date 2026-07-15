using System;
using System.Collections.Generic;
using UnityEngine;
using ThornClient.Core;
using ThornClient.Managers;
using ThornClient.Core;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace ThornClient.Modules.Enemy;

public class ForceRadiance : Module {
    public Setting<float> RadianceTier { get; }

    public override string IconName => "rainbow";

    public ForceRadiance() : base("Force Radiance", "Makes all enemies radiant, buffing them", ModuleCategory.Enemy) {
        RadianceTier = RegisterSetting("Radiance tier", "How much to buff enemies", 1f);
    }

    public override string? CheatReason => IsEnabled ? "Enables non-standard gameplay" : "";

    protected override void OnEnable() {
        UpdateRadiance(true, RadianceTier.Value);
        CheatManager.UpdateCheatiness();
        RadianceTier.OnValueChanged += UpdateRadiance;
    }

    protected override void OnDisable() {
        RadianceTier.OnValueChanged -= UpdateRadiance;
        UpdateRadiance(false, RadianceTier.Value);
    }

    private void UpdateRadiance(float value) {
        UpdateRadiance(IsEnabled, value);
    }

    private void UpdateRadiance(bool enabled, float value) {
        OptionsManager.forceRadiance = enabled;
        OptionsManager.radianceTier = value;

        foreach (EnemyIdentifier enemy in Object.FindObjectsOfType<EnemyIdentifier>()) {
            enemy.UpdateBuffs();
        }
    }
}
