using UnityEngine;
using ThornClient.Core;

namespace ThornClient.Managers;

public static class InputManager {
    public static void Update() {
        foreach (var element in ModuleManager.Modules) {
            KeyCode primaryKey = element.Keybind.Value;
            KeyCode modifierKey = element.KeybindModifier.Value;

            if (primaryKey == KeyCode.None) continue;
            bool modifierSatisfied = (modifierKey == KeyCode.None) || Input.GetKey(modifierKey);

            if (element.ToggleOnRelease.Value) {
                bool isComboHeld = modifierSatisfied && Input.GetKey(primaryKey);

                if (isComboHeld != element.IsEnabled) {
                    element.Toggle();
                }
            } else {
                if (modifierSatisfied && Input.GetKeyDown(primaryKey)) {
                    element.Toggle();
                }
            }
        }
    }
}
