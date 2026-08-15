using ThornClient.Core;
using ThornClient.Core.ConfigurableElements;
using ThornClient.Core.DataTypes;
using ThornClient.Managers;
using UnityEngine;

namespace ThornClient.System;

public class ThornModule : SystemModule {
    internal static ThornModule? Instance;
    internal static Color AccentColor => Instance?.Accent.Value ?? Color.white;

    public Setting<Keybind> OpenClickGUI { get; }
    public Setting<Keybind> SwitchTabLeft { get; }
    public Setting<Keybind> SwitchTabRight { get; }
    public Setting<Color> Accent { get; }
    public Setting<bool> MenuPausesGame { get; }
    public Setting<bool> MenuButtonPositionForceConfiggyNicePosition { get; }
    public Setting<string> LastVersion { get; }
    public override bool IsEnabled => true;
    public override Sprite Icon => AssetManager.Get<Sprite>(ClickGUI.BundleKey, "settings");

    public ThornModule() : base("thorn.thorn", "Thorn", "General settings") {
        if (Instance != null) return;
        Instance = this;

        UserHints.Initialize();

        OpenClickGUI = CreateSetting("openClickGui", "Open menu keybind", "Keybind to open Thorn's ClickGUI interface",
            new Keybind(KeyCode.RightShift));
        OpenClickGUI.OnPress += () => {
            // Plugin.Log.LogInfo("Opening ClickGUI");
            if (ClickGUI.Instance == null) return;
            ClickGUI.Instance.Toggle();
        };

        var otherBinds = CreateGroup("otherBinds", "Other keybinds", "Less important keybinds are here");
        SwitchTabLeft = CreateSetting("switchTabLeft", "Switch to tab on the left", "Switch to tab on the left",
            new Keybind(KeyCode.PageUp, modifier: KeyCode.LeftControl), otherBinds);
        SwitchTabLeft.OnPress += () => ClickGUI.CycleTab(-1);
        SwitchTabRight = CreateSetting("switchTabRight", "Switch to tab on the right", "Switch to tab on the right",
            new Keybind(KeyCode.PageDown, modifier: KeyCode.LeftControl), otherBinds);
        SwitchTabRight.OnPress += () => ClickGUI.CycleTab(+1);

        MenuPausesGame = CreateSetting(
            "menuPausesGame", "Menu pauses game",
            "Makes the game paused when you open the ClickGUI", true
        );
        MenuButtonPositionForceConfiggyNicePosition = CreateSetting(
            "menuButtonPosition", "Force nice menu button position",
            "Tries to move the Configgy button a bit so it has proper spacing",
            false
        );
        MenuButtonPositionForceConfiggyNicePosition.OnChanged += PauseMenuButton.UpdateAppearance;
        Accent = CreateSetting("accentColor", "Accent Color",
            "Color used for highlighting certain elements, preferably a bright one",
            new Color(0.65f, 0.95f, 0.89f));

        var devGroup = CreateGroup("devGroup", "Developer", "Developer options, not so interesting");
        LastVersion = CreateSetting("lastVersion", "Last Thorn Version", "The version of Thorn run in the previous/current session", "0.0.0", devGroup);

        var about = CreateHeader(
            "about", "About",
            "Thorn is an open-source, extensible utility mod for ULTRAKILL with a powerful configuration system."
        );
        var buttons = CreateButtonRow(
            "engagementButtons", "Links",
            "Links to Thorn stuff",
            ["GitHub", "Docs 4 mod devs", "Donations"]
        );
        buttons.OnClick += (i) => {
            switch (i) {
                case 0:
                    Application.OpenURL("https://github.com/end-4/ultrakill-thorn-client");
                    break;
                case 1:
                    Application.OpenURL("https://github.com/end-4/ultrakill-thorn-client/wiki");
                    break;
                case 2:
                    Application.OpenURL("https://github.com/sponsors/end-4");
                    break;
            }
        };

        PauseMenuButton.Initialize();
    }

    public override void OnUpdate() {
    }
}
