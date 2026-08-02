using Notiffy.API;
using ThornClient.Core;
using ThornClient.Core.ConfigurableElements;
using ThornClient.Core.DataTypes;
using ThornClient.Managers;
using UnityEngine;

namespace ThornClient.System;

internal class ThornModule : SystemModule {
    internal static ThornModule? Instance;
    internal static Color AccentColor => Instance?.Accent.Value ?? Color.white;

    public Setting<Keybind> OpenClickGUI { get; }
    public Setting<Keybind> SwitchTabLeft { get; }
    public Setting<Keybind> SwitchTabRight { get; }
    public Setting<Color> Accent { get; }
    public Setting<bool> MenuPausesGame { get; }
    public override bool IsEnabled => true;
    public override Sprite Icon => AssetManager.Get<Sprite>(ClickGUI.BundleKey, "settings");

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

        var otherBinds = CreateGroup("otherBinds", "Other keybinds", "Less important keybinds are here");
        SwitchTabLeft = RegisterSetting("switchTabLeft", "Switch to tab on the left", "Switch to tab on the left",
            new Keybind(KeyCode.PageUp, modifier: KeyCode.LeftControl), otherBinds);
        SwitchTabLeft.OnPress += () => ClickGUI.CycleTab(-1);
        SwitchTabRight = RegisterSetting("switchTabRight", "Switch to tab on the right", "Switch to tab on the right",
            new Keybind(KeyCode.PageDown, modifier: KeyCode.LeftControl), otherBinds);
        SwitchTabRight.OnPress += () => ClickGUI.CycleTab(+1);

        MenuPausesGame = RegisterSetting("menuPausesGame", "Menu pauses game",
            "Makes the game paused when you open the ClickGUI", true);

        Accent = RegisterSetting("accentColor", "Accent Color",
            "Color used for highlighting certain elements, preferably a bright one",
            new Color(0.65f, 0.95f, 0.89f));
    }

    public override void OnUpdate() {
    }
}
