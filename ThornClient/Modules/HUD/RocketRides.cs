using NukeLib.Utils;
using ThornClient.Core.ConfigurableElements;
using ThornClient.HUD;
using ThornClient.Managers;
using ThornClient.System;
using UnityEngine;

namespace ThornClient.Modules.HUD;

/// <summary>
/// The number of remaining rocket rides
/// </summary>
public class RocketRides : BoundedValueHudModule {
    /// <inheritdoc />
    public override Sprite Icon => AssetManager.Get<Sprite>(ClickGUI.BundleKey, "rocket_ride");

    /// <inheritdoc />
    public override string[] Tags => ["fly", "rocket", "travel", "fuel"];

    /// <summary>
    /// The total number of rides practically available
    /// </summary>
    public Setting<int> EffectiveRides;

    /// <inheritdoc />
    public RocketRides() : base("thorn.rocketRides", "Rocket Rides", "Shows number of remaining effective rocket rides",
        5, displayIcon: AssetManager.Get<Sprite>(ClickGUI.BundleKey, "rocket_ride"), displayName: "Rides",
        defaultValueColor: 0xFF5900.ToColor(), decimalPlaces: 0) {
        EffectiveRides = CreateSetting("effectiveRides", "Effective rides",
            "Number of effective rocket rides. There is no strict value, but the 6th one droops immediately and noticeably.",
            5);
        EffectiveRides.OnValueChanged += SetBound;
    }

    private void SetBound(int value) {
        Bound = value;
    }

    /// <inheritdoc />
    public override void OnUpdate() {
        var nm = NewMovement.Instance;
        if (nm == null) return;
        Value = EffectiveRides.Value - nm.rocketRides;
    }
}
