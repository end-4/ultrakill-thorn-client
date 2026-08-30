using System.Collections.Generic;
using System.Runtime.Serialization;
using NukeLib.Utils;
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
    public enum TimeHourFormat {
        Twelve,
        TwentyFour
    }

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
    /// Whether opening the ClickGUI should pause the game
    /// </summary>
    public Setting<bool> MenuPausesGame { get; }

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
    /// The time format in the ClickGUI, either 12h or 24h
    /// </summary>
    public Setting<TimeHourFormat> TimeFormat;

    /// <summary>
    /// Whether to force the Configgy button to have proper spacing
    /// </summary>
    public Setting<bool> MenuButtonPositionForceConfiggyNicePosition { get; }

    /// <summary>
    /// Whether to use snapping for HUD widgets
    /// </summary>
    public Setting<bool> SnapEnabled;

    /// <summary>
    /// The gap at which HUD widgets should be away from each other when snapped
    /// </summary>
    public Setting<int> SnapGap;

    /// <summary>
    /// At most how far away should HUD widgets be from a snap target to be snapped (for facing edge snaps)
    /// </summary>
    public Setting<int> SnapActivationDistance;

    /// <summary>
    /// At most how far away should HUD widgets be from a snap target to be snapped (for alignment snaps)
    /// </summary>
    public Setting<int> SnapActivationDistanceAlignment;

    /// <summary>
    /// Last loaded version
    /// </summary>
    public Setting<string> LastVersion { get; }

    public override Sprite Icon => AssetManager.Get<Sprite>(ClickGUI.BundleKey, "settings");

    /// <inheritdoc />
    public ThornModule() : base("thorn.thorn", "Thorn", "General settings") {
        if (Instance != null) return;
        Instance = this;

        UserHints.Initialize();

        // -- KEYBINDS --
        CreateHeader("binds", "Keybinds");

        OpenClickGUI = CreateSetting("openClickGui", "Open menu keybind", "Keybind to open Thorn's ClickGUI interface",
            new Keybind(KeyCode.RightShift));
        OpenClickGUI.OnPress += () => {
            if (ClickGUI.Instance == null) return;
            ClickGUI.Instance.Toggle();
            PreventSoftlock();
        };

        MenuPausesGame = CreateSetting(
            "menuPausesGame", "Menu pauses game",
            "Makes the game paused when you open the ClickGUI", true
        );

        var otherBinds = CreateGroup("otherBinds", "More keybinds", "Less important keybinds are here");
        OpenClickGUIUnpaused = CreateSetting("openClickGuiPaused", "Open menu (no pausing)",
            "Keybind to open Thorn's ClickGUI interface without pausing the game",
            new Keybind(KeyCode.None), otherBinds);
        OpenClickGUIUnpaused.OnPress += () => {
            if (ClickGUI.Instance == null) return;
            var tmp = MenuPausesGame!.Value;
            MenuPausesGame.Value = false;
            ClickGUI.Instance.Toggle();
            MenuPausesGame.Value = tmp;
            PreventSoftlock();
        };
        SwitchTabLeft = CreateSetting("switchTabLeft", "Switch to tab on the left", "Switch to tab on the left",
            new Keybind(KeyCode.PageUp, modifier: KeyCode.LeftControl), otherBinds);
        SwitchTabLeft.OnPress += () => ClickGUI.CycleTab(-1);
        SwitchTabRight = CreateSetting("switchTabRight", "Switch to tab on the right", "Switch to tab on the right",
            new Keybind(KeyCode.PageDown, modifier: KeyCode.LeftControl), otherBinds);
        SwitchTabRight.OnPress += () => ClickGUI.CycleTab(+1);


        // -- INTERFACE --
        CreateHeader("general", "Interface");
        Accent = CreateSetting("accentColor", "Accent color",
            "Color used for highlighting certain elements, preferably a bright one",
            new Color(0.65f, 0.95f, 0.89f));

        var uiGroup = CreateGroup("uiGroup", "More interface settings", "Stuff related to the user interface");
        CreateHeader("uiGeneral", "General", "", parent: uiGroup);
        TimeFormat = CreateSetting("timeFormat", "Time format", "Used on the menu's top bar", TimeHourFormat.Twelve,
            uiGroup);
        TimeFormat.Hints = new InterfaceHints {
            EnumSubstitutions = new Dictionary<string, string> {
                ["Twelve"] = "12h",
                ["TwentyFour"] = "24h",
            }
        };
        MenuButtonPositionForceConfiggyNicePosition = CreateSetting(
            "menuButtonPosition", "Force nice spacing",
            "Tries to move the Configgy button on the pause menu a bit so it has proper spacing",
            false, uiGroup
        );
        MenuButtonPositionForceConfiggyNicePosition.OnChanged += PauseMenuButton.UpdateAppearance;
        CreateHeader("uiHud", "HUD", "", parent: uiGroup);
        CreateHeader("uiHud", "Snapping", "", HeaderType.H2, parent: uiGroup);
        SnapEnabled = CreateSetting("snapEnabled", "Enable",
            "Whether widgets snap to others when dragging", true,
            uiGroup
        );
        SnapGap = CreateSetting(
            "snapGap", "Gap", "Gap between two widgets when snapped",
            1, uiGroup
        );
        SnapActivationDistance = CreateSetting(
            "snapActivationDistance", "Activation distance (facing edges)",
            "At most how far away should HUD widgets be from a snap target to be snapped (for facing edge snaps)",
            8, uiGroup
        );
        SnapActivationDistanceAlignment = CreateSetting(
            "snapActivationDistanceAlignment", "Activation distance (alignment)",
            "At most how far away should HUD widgets be from a snap target to be snapped (for alignment snaps)",
            9999, uiGroup
        );

        // -- ABOUT --
        var about = CreateHeader(
            "about", "About",
            "Thorn is an open-source, extensible utility mod for ULTRAKILL with a powerful configuration system."
        );
        var buttons = CreateButtonRow(
            "engagementButtons", "Links",
            "Links to Thorn stuff",
            ["GitHub", "API Docs", "Donations"]
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

        var devGroup = CreateGroup("devGroup", "Developer", "Developer options, not so interesting");
        LastVersion = CreateSetting("lastVersion", "Last Thorn version",
            "The version of Thorn run in the previous/current session", "0.0.0", devGroup);

        PauseMenuButton.Initialize();
    }

    private void PreventSoftlock() {
        ExecutionUtils.RunNextFrame(() => {
            if (OpenClickGUI.Value.Key == KeyCode.Mouse0 || OpenClickGUI.Value.Modifier == KeyCode.Mouse0)
                OpenClickGUI.Reset();
            if (OpenClickGUIUnpaused.Value.Key == KeyCode.Mouse0 ||
                OpenClickGUIUnpaused.Value.Modifier == KeyCode.Mouse0)
                OpenClickGUIUnpaused.Reset();
        });
    }
}
