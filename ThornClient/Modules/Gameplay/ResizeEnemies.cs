using System.Collections.Generic;
using NukeLib.Game;
using NukeLib.Utils;
using UnityEngine;
using ThornClient.Core;
using ThornClient.Core.ConfigurableElements;
using ThornClient.Managers;
using ThornClient.System;

namespace ThornClient.Modules.Gameplay;

/// <summary>
/// Module that modifies the size of enemies
/// </summary>
public class ResizeEnemies : Module {
    // /// <inheritdoc />
    public override Sprite Icon =>
        AssetManager.Get<Sprite>(ClickGUI.BundleKey, "axes"); // TODO use a different icon from viewmodel tweaks

    /// <inheritdoc />ac
    public override string[] Tags => ["scale", "oops all size 2"];

    public Setting<float> EnemyScale;

    /// <summary>
    /// Why this is cheaty
    /// </summary>
    public override string? CheatReason => IsEnabled ? "Enables non-standard gameplay" : "";

    /// <inheritdoc />
    public ResizeEnemies() : base("thorn.resizeEnemies", "Resize Enemies", "Changes the size of enemies",
        ModuleCategory.Gameplay) {
        EnemyScale = CreateSetting("enemyScale", "Enemy scale", "The scale of enemies", 2f);
    }

    /// <inheritdoc />
    protected override void OnEnable() {
        UpdateAllEnemies();
        EnemyScale.OnChanged += UpdateAllEnemies;
        EnemyEvents.OnSpawn += ApplyScale;
    }

    /// <inheritdoc />
    protected override void OnDisable() {
        EnemyEvents.OnSpawn -= ApplyScale;
        EnemyScale.OnChanged -= UpdateAllEnemies;
        UpdateAllEnemies(1f);
    }

    private void UpdateAllEnemies() {
        UpdateAllEnemies(EnemyScale.Value);
    }

    /// <summary>
    /// Updates the scale on all enemies
    /// </summary>
    /// <param name="scale">The scale to use</param>
    protected virtual void UpdateAllEnemies(float scale) {
        foreach (EnemyIdentifier enemy in Object.FindObjectsOfType<EnemyIdentifier>()) {
            ApplyScale(enemy, scale);
        }
    }

    private static Dictionary<EnemyType, float> DefaultScaleCache = [];
    private const string ScaledTagName = "ThornScaled";

    private void ApplyScale(EnemyIdentifier eid) {
        ApplyScale(eid, EnemyScale.Value);
    }

    private void ApplyScale(EnemyIdentifier eid, float scaleMultiplier) {
        if (eid == null) return;
        var type = eid.enemyType;
        var obj = eid.gameObject;
        if (obj == null) return;
        var trans = obj.transform;
        Plugin.Log.LogInfo($"scaling {obj.name} to {scaleMultiplier}");

        if (!obj.name.HasTag(ScaledTagName) && !DefaultScaleCache.ContainsKey(type)) {
            DefaultScaleCache.Add(type, trans.localScale.x);
        }

        float defaultScale = 1f;
        if (DefaultScaleCache.TryGetValue(type, out float f)) defaultScale = f;
        trans.localScale = Vector3.one * defaultScale * scaleMultiplier;

        obj.name = Mathf.Approximately(scaleMultiplier, 1f)
            ? obj.name.Untag(ScaledTagName)
            : obj.name.Tag(ScaledTagName);
    }
}
