using ThornClient.Core;

namespace ThornClient.Managers;

public static class InputManager {
    public static bool BlockInput { get; set; } = false;

    public static void Update() {
        if (BlockInput) return;

        foreach (var module in ModuleManager.Modules) {
            foreach (var setting in module.Settings) {
                if (setting.GetValue() is Keybind dynamicKeybind) {
                    dynamicKeybind.CheckInput();
                }
            }
        }
    }
}
