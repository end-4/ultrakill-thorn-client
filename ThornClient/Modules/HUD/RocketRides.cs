using ThornClient.Core.ConfigurableElements;
using ThornClient.HUD;
using ThornClient.Managers;
using UnityEngine;

namespace ThornClient.Modules.HUD;

public class RocketRides : BoundedValueHudModule {
    public override Sprite Icon => AssetManager.Get<Sprite>(HudManager.BundleKey, "rocket_ride");
    public override string[] Tags => ["fly", "rocket", "travel", "fuel"];

    public Setting<int> EffectiveRides;

    public RocketRides() : base("thorn.rocketRides", "Rocket Rides", "Shows number of remaining effective rocket rides",
        5, displayIcon: AssetManager.Get<Sprite>(HudManager.BundleKey, "rocket_ride"), displayName: "Rides",
        defaultValueColor: new Color(1, 0.5f, 0.23f)) {
        EffectiveRides = CreateSetting("effectiveRides", "Effective rides",
            "Number of effective rocket rides. There is no strict value, but the 6th one droops immediately and noticeably.",
            5);
        EffectiveRides.OnValueChanged += SetBound;
    }

    private void SetBound(int value) {
        Bound = value;
    }

    protected override void OnHudModuleEnable() {
        DecimalPlaces = 0;
    }

    public override void OnUpdate() {
        var nm = NewMovement.Instance;
        if (nm != null) {
            Value = EffectiveRides.Value - nm.rocketRides;
        }
    }
}
