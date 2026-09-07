using NukeLib.Game;
using NukeLib.Utils;
using UnityEngine;
using ThornClient.Core;
using ThornClient.Core.ConfigurableElements;
using ThornClient.Managers;
using ThornClient.System;
using Object = UnityEngine.Object;

namespace ThornClient.Modules.Gameplay;

/// <summary>
/// Module that forces enemies to be sanded.
/// </summary>
public class GravityTweak : Module {
    /// <inheritdoc />
    public override Sprite Icon => AssetManager.Get<Sprite>(ClickGUI.BundleKey, "axes");

    /// <inheritdoc />
    public override string[] Tags => ["fall", "direction", "upside down"];

    public Setting<float> XDir;
    public Setting<float> YDir;
    public Setting<float> ZDir;
    public Setting<float> Multiplier;

    private static readonly float BaseGravityMultiplier = 40f;
    private static readonly Vector3 DefaultGravityVector = new Vector3(0f, -40f, 0f);
    private static readonly float UpdateInterval = 1f;

    /// <inheritdoc />
    public GravityTweak() : base("thorn.gravityTweak", "Gravity Tweak", "Changes gravity direction",
        ModuleCategory.Gameplay) {
        CreateHeader("direction", "Direction vector", "These three values form a normalized vector");
        // Note normal is y=-1 but we default to 1 so the tweak does an upside down by default
        XDir = CreateSetting("xDir", "X", "X value of the direction vector", 0f);
        YDir = CreateSetting("yDir", "Y", "Y value of the direction vector", 1f);
        ZDir = CreateSetting("zDir", "Z", "Z value of the direction vector", 0f);
        XDir.Hints = InterfaceHints.RangeHint(-1f, 1f);
        YDir.Hints = InterfaceHints.RangeHint(-1f, 1f);
        ZDir.Hints = InterfaceHints.RangeHint(-1f, 1f);
        CreateHeader("strength", "Gravity strength");
        Multiplier = CreateSetting("multiplier", "Multiplier", "Higher makes you fall faster", 1f);
    }

    /// <inheritdoc />
    public override string CheatReason => IsEnabled ? "Enables non-standard gameplay" : "";

    /// <inheritdoc />
    protected override void OnEnable() {
        CheatManager.UpdateCheatiness();
        SceneUtils.SafeSceneLoadedNoParam += DelayGravityUpdate;
    }

    /// <inheritdoc />
    protected override void OnDisable() {
        SceneUtils.SafeSceneLoadedNoParam -= DelayGravityUpdate;
        ResetGravity();
    }

    private float _cumulatedTime = 0f;

    /// <inheritdoc />
    public override void OnUpdate() {
        // if (!(StatsManager.Instance?.timer ?? false)) return;
        _cumulatedTime += Time.deltaTime;
        if (_cumulatedTime >= UpdateInterval) {
            _cumulatedTime %= UpdateInterval;
            UpdateGravity();
        }
    }

    private void DelayGravityUpdate() {
        _cumulatedTime = 0f;
    }

    private void UpdateGravity() {
        var dir = (new Vector3(XDir.Value, YDir.Value, ZDir.Value)).normalized;
        var scaled = dir * (BaseGravityMultiplier * Multiplier.Value);
        NewMovement.Instance?.SwitchGravity(scaled, transformCamera: false);
    }

    private void ResetGravity() {
        NewMovement.Instance?.SwitchGravity(DefaultGravityVector, transformCamera: false);
    }
}
