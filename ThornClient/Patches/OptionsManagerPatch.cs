using HarmonyLib;

namespace ThornClient.Patches;

/// <summary>
/// Patches the OptionsManager to support input blocking
/// </summary>
[HarmonyPatch(typeof(OptionsManager), nameof(OptionsManager.UnPause))]
public class UnpausePatch {
    static bool Prefix(OptionsManager __instance) {
        try {
            return !Managers.InputManager.BlockInput;
        } catch {
            return true;
        }
    }
}

/// <summary>
/// Patches the OptionsManager to support input blocking
/// </summary>
[HarmonyPatch(typeof(OptionsManager), nameof(OptionsManager.Pause))]
public class PausePatch {
    static bool Prefix(OptionsManager __instance) {
        try {
            return !Managers.InputManager.BlockInput;
        } catch {
            return true;
        }
    }
}

/// <summary>
/// Patches the OptionsManager to support input blocking
/// </summary>
[HarmonyPatch(typeof(OptionsManager), nameof(OptionsManager.UnFreeze))]
public class UnfreezePatch {
    static bool Prefix(OptionsManager __instance) {
        try {
            return !Managers.InputManager.BlockInput;
        } catch {
            return true;
        }
    }
}

/// <summary>
/// Patches the OptionsManager to support input blocking
/// </summary>
[HarmonyPatch(typeof(OptionsManager), nameof(OptionsManager.Freeze))]
public class FreezePatch {
    static bool Prefix(OptionsManager __instance) {
        try {
            return !Managers.InputManager.BlockInput;
        } catch {
            return true;
        }
    }
}
