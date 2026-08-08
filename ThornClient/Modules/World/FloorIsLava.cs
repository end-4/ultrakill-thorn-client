using System;
using ThornClient.Managers;
using ThornClient.System;
using UnityEngine;
using System.Collections.Generic;
using NukeLib.Utils;
using ThornClient.Core;
using ThornClient.Core.ConfigurableElements;
using ThornClient.Core.DataTypes;
using UnityEngine.Rendering;

namespace ThornClient.Modules.World;

/// <summary>
/// Module that draws lines from the player to enemies
/// </summary>
public class FloorIsLava : Module {
    /// <inheritdoc />
    public override Sprite Icon => AssetManager.Get<Sprite>(ClickGUI.BundleKey, "lava");

    /// <inheritdoc />
    public override string[] Tags => ["hazard", "hot"];

    /// <summary>
    /// We hide this as a soft standardization and to avoid overwhelming
    /// </summary>
    public SettingGroup DamageGroup;

    public Setting<float> DamageInterval;
    public Setting<int> DamageAmount;

    /// <summary>
    /// Constructor
    /// </summary>
    public FloorIsLava() : base("thorn.floorIsLava", "Floor is Lava", "..well, magma to be accurate",
        ModuleCategory.World) {
        DamageGroup = CreateGroup("damageGroup", "Damage settings", "Configure damage frequency & amount");
        DamageInterval = CreateSetting("damageInterval", "Damage interval",
            "How far apart two damage ticks are, in seconds", 1f, DamageGroup);
        DamageAmount = CreateSetting("damageAmount", "Damage amount", "How much damage to do per tick", 10,
            DamageGroup);
        DamageAmount.Hints = new InterfaceHints {
            Range = Tuple.Create(0f, 100f)
        };
    }

    private static AmbientMode DefaultAmbientMode = AmbientMode.Flat;

    protected override void OnEnable() {
        ResetTimer();
    }

    private static NewMovement nm => NewMovement.Instance;
    private static StatsManager sman => StatsManager.Instance;

    private float _onFloorDuration = 0;

    public override void OnUpdate() {
        if (nm == null || sman == null || !sman.timer) return;

        if (!nm.gc.touchingGround || nm.touchingWaters.Count > 0) {
            ResetTimer();
            return;
        }

        _onFloorDuration += Time.deltaTime;

        if (_onFloorDuration >= DamageInterval.Value) {
            TickDamage();
            _onFloorDuration %= DamageInterval.Value;
        }
    }

    private void TickDamage() {
        if (nm == null) return;
        nm.GetHurt(DamageAmount.Value, false, 1);
    }

    private void ResetTimer() {
        // Because we wanna damage the player immediately when they touch the ground
        _onFloorDuration = DamageInterval.Value;
    }
}
