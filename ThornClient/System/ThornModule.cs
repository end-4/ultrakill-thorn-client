using Notiffy.API;
using ThornClient.Core;
using ThornClient.Core.DataTypes;
using UnityEngine;

namespace ThornClient.System;

internal class ThornModule : SystemModule {
    internal static ThornModule? Instance;

    public Setting<Keybind> OpenClickGUI { get; }
    public Setting<Color> Accent { get; }
    public Setting<bool> Testia { get; }
    public override bool IsEnabled => true;
    public override string IconName => "settings";

    public ThornModule() : base("thorn.thorn", "Thorn", "General settings") {
        if (Instance != null) return;
        Instance = this;
        OpenClickGUI = RegisterSetting("openClickGui", "Open GUI keybind", "Keybind to open Thorn's ClickGUI interface",
            new Keybind(KeyCode.RightShift));
        OpenClickGUI.OnPress += () => {
            // Plugin.Log.LogInfo("Opening ClickGUI");
            if (ClickGUI.Instance == null) return;
            ClickGUI.Instance.Toggle();
        };

        Accent = RegisterSetting("accentColor", "Accent Color", "Color used for highlighting certain elements, preferably a bright one",
            new Color(0.65f, 0.95f, 0.89f));
    }

    public override void OnUpdate() {
    }
}
