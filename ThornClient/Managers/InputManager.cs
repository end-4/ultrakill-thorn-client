using ThornClient.Core;

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
