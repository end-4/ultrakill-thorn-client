using ThornClient.Core;
using UnityEngine;

namespace ThornClient.System;

internal class ThornModule : SystemModule {
    internal static ThornModule? Instance;

    public Setting<Keybind> OpenClickGUI { get; }
    public override bool IsEnabled => true;

    public ThornModule() : base("thorn.thorn", "Thorn", "Configuration for Thorn") {
        OpenClickGUI = RegisterSetting("openClickGui", "Open GUI keybind", "Keybind to open Thorn's ClickGUI interface", new Keybind(KeyCode.RightShift));
        OpenClickGUI.Value.OnPress = () => {
            Plugin.Log.LogInfo("Opening ClickGUI");
            if (ClickGUI.Instance == null) return;
            ClickGUI.Instance.Toggle();
        };
    }

    public override void OnUpdate() {}
}
