using System;
using ThornClient.Core.ConfigurableElements;
using ThornClient.Managers;
using ThornClient.System;
using UnityEngine;
using ThornClient.HUD;

namespace ThornClient.Modules.HUD;

/// <summary>
/// Module that shows IRL time
/// </summary>
public class ClockWidget : TextHudModule {
    /// <inheritdoc />
    public override Sprite Icon => AssetManager.Get<Sprite>(ClickGUI.BundleKey, "clock");

    /// <inheritdoc />
    public override Sprite DisplayIcon => Icon;

    /// <inheritdoc />
    public override string[] Tags => ["time", "tick tock"];

    /// <summary>
    /// Constructor
    /// </summary>
    public ClockWidget() : base("thorn.clockWidget", "Clock", "Shows real-life time") {
        
    }

    /// <inheritdoc />
    public override void OnUpdate() {
        var now = DateTime.Now;
        bool use24h = ThornModule.Instance?.TimeFormat.Value == ThornModule.TimeHourFormat.TwentyFour;
        string format = use24h ? "HH:mm" : "hh:mm";
        string newNumber = now.ToString(format);
        string newAmPm = use24h ? string.Empty : now.ToString("tt");
        if (!use24h) newAmPm = $"<size=6> </size><size=10>{newAmPm}</size>";
        Text = $"{newNumber}{newAmPm}";
    }
}
