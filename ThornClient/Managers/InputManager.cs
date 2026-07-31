using System.Collections.Generic;
using ThornClient.Core;
using ThornClient.Core.ConfigurableElements;
using ThornClient.Core.DataTypes;
using ThornClient.System;
using UnityEngine;

namespace ThornClient.Managers;

public static class InputManager {
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

    public static void RegisterKeybindSetting(Setting<Keybind> setting) {
        if (!KeybindSettings.Contains(setting)) {
            KeybindSettings.Add(setting);
        }
    }

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
