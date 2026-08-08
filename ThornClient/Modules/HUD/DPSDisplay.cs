using System;
using NukeLib.Game;
using ThornClient.Core.ConfigurableElements;
using ThornClient.Managers;
using ThornClient.System;
using UnityEngine;
using ThornClient.HUD;

namespace ThornClient.Modules.HUD;

/// <summary>
/// Module that shows FPS
/// </summary>
public class DPSDisplay : ContinuousCumulatedStatHudModule {
    /// <summary>
    /// Setting: whether to show "FPS" text on the HUD element
    /// </summary>
    public Setting<bool> ShowDpsText;

    /// <inheritdoc />
    public override Sprite Icon => AssetManager.Get<Sprite>(ClickGUI.BundleKey, "fire");

    /// <inheritdoc />
    public override string[] Tags => ["damage", "burst", "firepower"];

    /// <inheritdoc />
    public DPSDisplay() : base("thorn.dpsDisplay", "DPS", "Shows damage per second") {
        ShowDpsText = CreateSetting("showDpsText", "Show \"DPS\" text",
            "Makes the indicator say \"DPS:20\" instead of \"20\"", true);
    }

    private float _cum = 0; // it's cumulative, okay?!

    protected override void OnHudModuleEnable() {
        EnemyEvents.OnDamageTaken += CumulateDamage;
    }

    protected override void OnHudModuleDisable() {
        EnemyEvents.OnDamageTaken -= CumulateDamage;
    }

    private void CumulateDamage(EnemyIdentifier eid, float dmg) {
        _cum += dmg;
    }

    protected override float CollectUpdate() {
        var thisBatch = _cum;
        _cum = 0;
        return thisBatch;
    }

    protected override string FormatStat(float value) {
        string prefix = ShowDpsText.Value ? "DPS:" : "";
        return $"{prefix}{Math.Round(value, 1)}";
    }
}
