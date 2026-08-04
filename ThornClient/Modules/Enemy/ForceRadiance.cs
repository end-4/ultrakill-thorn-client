using UnityEngine;
using ThornClient.Core;
using ThornClient.Core.ConfigurableElements;
using ThornClient.Managers;
using ThornClient.System;
using Object = UnityEngine.Object;

namespace ThornClient.Modules.Enemy;

/// <summary>
/// Module that forces radiance on enemies.
/// </summary>
public class ForceRadiance : Module {
    /// <summary>
    /// How much to buff enemies
    /// </summary>
    public Setting<float> RadianceTier { get; }

    /// <summary>
    /// Icon in the menu
    /// </summary>
    public override Sprite Icon => AssetManager.Get<Sprite>(ClickGUI.BundleKey, "rainbow");

    /// <summary>
    /// Tags for searching
    /// </summary>
    public override string[] Tags => ["buff", "strong", "rainbow", "gay"];

    /// <summary>
    /// Constructor
    /// </summary>
    public ForceRadiance() : base("thorn.forceRadiance", "Force Radiance", "Makes all enemies radiant, buffing them",
        ModuleCategory.Enemy) {
        RadianceTier = CreateSetting("radianceTier", "Radiance tier", "How much to buff enemies", 1f);
    }

    /// <summary>
    /// Why this is cheaty
    /// </summary>
    public override string? CheatReason => IsEnabled ? "Enables non-standard gameplay" : "";

    /// <summary>
    /// Run when enabled
    /// </summary>
    protected override void OnEnable() {
        UpdateRadiance(true, RadianceTier.Value);
        CheatManager.UpdateCheatiness();
        RadianceTier.OnValueChanged += UpdateRadiance;
    }

    /// <summary>
    /// Run when disabled
    /// </summary>
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
