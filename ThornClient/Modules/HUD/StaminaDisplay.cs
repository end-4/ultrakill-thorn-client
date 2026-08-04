using System;
using ThornClient.Core.ConfigurableElements;
using ThornClient.HUD;
using ThornClient.Managers;
using UnityEngine;

namespace ThornClient.Modules.HUD;

public class StaminaDisplay : BoundedValueHudModule {
    public override Sprite Icon => AssetManager.Get<Sprite>(HudManager.BundleKey, "dash");
    public override string[] Tags => ["dash", "boost", "fast", "shift"];

    public Setting<bool> Continuous;

    public StaminaDisplay() : base("thorn.staminaHud", "Stamina", "Shows stamina", 3) {
        Continuous = CreateSetting("continuousDisplay", "Continuous Display", "Whether to show the value as continuous or discrete (dash count)", false);
        ValueColor.DefaultValue = new Color(0, 0.93f, 1);
    }

    public override void OnUpdate() {
        var nm = NewMovement.Instance;
        if (nm != null) {
            var scaled = nm.boostCharge / 100f;
            Value = Continuous.Value ? scaled : (float)Math.Floor(scaled);
        }
    }
}
