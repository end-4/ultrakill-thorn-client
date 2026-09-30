using System;
using ThornClient.Core.ConfigurableElements;
using ThornClient.Core.DataTypes;
using ThornClient.HUD;
using ThornClient.Managers;
using UnityEngine;

namespace ThornClientModules.HUD;

/// <summary>
/// HUD widget that shows stamina
/// </summary>
public class StaminaDisplay : BoundedValueHudModule {
    /// <inheritdoc />
    public override Sprite Icon => AssetManager.Get<Sprite>(HudManager.BundleKey, "dash");

    /// <inheritdoc />
    public override string[] Tags => ["dash", "boost", "fast", "shift"];

    /// <inheritdoc />
    public StaminaDisplay() : base("thorn.staminaHud", "Stamina", "Shows stamina", 3,
        defaultValueColor: new EnhancedColor(0, 0.77f, 1)) {
    }

    /// <inheritdoc />
    public override void OnUpdate() {
        var nm = NewMovement.Instance;
        if (nm == null) return;
        var scaled = nm.boostCharge / 100f;
        Value = Mathf.Floor(scaled);
        ChargeValue = scaled;
    }
}
