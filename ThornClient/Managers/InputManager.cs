using System.Collections.Generic;
using ThornClient.Core;
using ThornClient.Core.ConfigurableElements;
using ThornClient.Core.DataTypes;
using ThornClient.System;
using UnityEngine;

namespace ThornClient.Managers;

/// <summary>
/// The central manager for Thorn's inputs including keybinds
/// </summary>
public static class InputManager {
    /// <summary>
    /// Whether to prevent keybinds from triggering
    /// </summary>
    public static bool BlockInput {
        get => field;
        set {
            field = value;
            var opt = OptionsManager.Instance;
            if (field) {
                if (opt != null) opt.dontUnpause = true;
            } else {
                if (opt != null) opt.dontUnpause = false;
            }
        }
    } = false;

    private static readonly List<Setting<Keybind>> KeybindSettings = [];

    /// <summary>
    /// Registers a keybind setting to receive input event triggers.
    /// </summary>
    /// <param name="setting">The keybind setting to register</param>
    public static void RegisterKeybindSetting(Setting<Keybind> setting) {
        if (!KeybindSettings.Contains(setting)) {
            KeybindSettings.Add(setting);
        }
    }

    /// <summary>
    /// Key listening loop
    /// </summary>
    public static void Update() {
        if (BlockInput) return;

        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(3)) {
            var gui = ClickGUI.Instance;
            if (gui != null && gui.IsEnabled) ClickGUI.NavigateBack();
        }

        foreach (var setting in KeybindSettings) {
            var keybind = setting.Value;
            if (keybind.Key == KeyCode.None) continue;

            bool modifierSatisfied = (keybind.Modifier == KeyCode.None) || Input.GetKey(keybind.Modifier);
            if (!modifierSatisfied) continue;

            if (Input.GetKeyDown(keybind.Key)) {
                setting.RaiseOnPress();
            }

            if (Input.GetKeyUp(keybind.Key)) {
                setting.RaiseOnRelease();
            }
        }
    }
}
