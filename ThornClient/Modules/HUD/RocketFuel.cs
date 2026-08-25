using HarmonyLib;
using NukeLib.Utils;
using ThornClient.Core.ConfigurableElements;
using ThornClient.HUD;
using ThornClient.Managers;
using ThornClient.System;
using UnityEngine;

namespace ThornClient.Modules.HUD;

/// <summary>
/// The remaining rocket fuel
/// </summary>
public class RocketFuel : BoundedValueHudModule {
    /// <inheritdoc />
    public override Sprite Icon => AssetManager.Get<Sprite>(ClickGUI.BundleKey, "fuel");

    /// <inheritdoc />
    public override string[] Tags => ["fly", "rocket", "travel", "fuel", "velocity", "droop", "fall"];

    /// <summary>
    /// Threshold to consider the rocket has run out of fuel
    /// </summary>
    public Setting<float> Overstay;

    /// <inheritdoc />
    public RocketFuel() : base("thorn.rocketFuel", "Rocket Fuel",
        "Shows remaining rocket ride time before the rocket droops down",
        1, displayIcon: AssetManager.Get<Sprite>(ClickGUI.BundleKey, "fuel"), displayName: "Fuel",
        defaultValueColor: 0xFF5900.ToColor(), decimalPlaces: 1) {
        Overstay = CreateSetting("overstay", "Overstay",
            "Difference between the point of practical \"fuel emptiness\" and the mathematical point of emptiness",
            0.1f);
    }

    /// <inheritdoc />
    public override void OnUpdate() {
        var nm = NewMovement.Instance;
        if (nm == null) return;
        var rocket = nm.ridingRocket;
        if (rocket == null) Value = 1;
        else {
            float dp = Traverse.Create(nm.ridingRocket).Field<float>("downpull").Value;
            Value = Mathf.InverseLerp(Overstay.Value, -0.5f, dp);
        }
    }
}
