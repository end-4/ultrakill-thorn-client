using ThornClient.Core;
using ThornClient.Core.ConfigurableElements;
using ThornClient.Core.DataTypes;
using ThornClient.Managers;
using UnityEngine;

namespace ThornClient.System;

/// <summary>
/// The module that houses general settings of Thorn
/// </summary>
public class ThornModule : SystemModule {
    /// <summary>
    /// The instance of this module
    /// </summary>
    public static ThornModule? Instance;

    /// <summary>
    /// Convenient getter for the accent color
    /// </summary>
    public static Color AccentColor => Instance?.Accent.Value ?? Color.white;

    /// <summary>
    /// Keybind to open ClickGUI
    /// </summary>
    public Setting<Keybind> OpenClickGUI { get; }

    /// <summary>
    /// Keybind to open the ClickGUI without pausing
    /// </summary>
    public Setting<Keybind> OpenClickGUIUnpaused { get; }

    /// <summary>
    /// Keybind to switch to the tab on the left
    /// </summary>
    public Setting<Keybind> SwitchTabLeft { get; }

    /// <summary>
    /// Keybind to switch to the tab on the right
    /// </summary>
    public Setting<Keybind> SwitchTabRight { get; }

    /// <summary>
    /// Setting for the accent color
    /// </summary>
    public Setting<Color> Accent { get; }

    /// <summary>
    /// Whether opening the ClickGUI should pause the game
    /// </summary>
    public Setting<bool> MenuPausesGame { get; }

    /// <summary>
    /// Whether to force the Configgy button to have proper spacing
    /// </summary>
    public Setting<bool> MenuButtonPositionForceConfiggyNicePosition { get; }

    /// <summary>
    /// Last loaded version
    /// </summary>
    public Setting<string> LastVersion { get; }

    /// <inheritdoc />
    public override bool IsEnabled => true;
    public override Sprite Icon => AssetManager.Get<Sprite>(ClickGUI.BundleKey, "settings");

    /// <inheritdoc />
    public ThornModule() : base("thorn.thorn", "Thorn", "General settings") {
        if (Instance != null) return;
        Instance = this;

        UserHints.Initialize();

        OpenClickGUI = CreateSetting("openClickGui", "Open menu keybind", "Keybind to open Thorn's ClickGUI interface",
            new Keybind(KeyCode.RightShift));
        OpenClickGUI.OnPress += () => {
            if (ClickGUI.Instance == null) return;
            ClickGUI.Instance.Toggle();
        };
        OpenClickGUIUnpaused = CreateSetting("openClickGuiPaused", "Open menu (no pausing)",
            "Keybind to open Thorn's ClickGUI interface without pausing the game",
            new Keybind(KeyCode.None));
        OpenClickGUIUnpaused.OnPress += () => {
            if (ClickGUI.Instance == null) return;
            var tmp = MenuPausesGame!.Value;
            MenuPausesGame.Value = false;
            ClickGUI.Instance.Toggle();
            MenuPausesGame.Value = tmp;
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
        LastVersion = CreateSetting("lastVersion", "Last Thorn Version",
            "The version of Thorn run in the previous/current session", "0.0.0", devGroup);

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
