using HarmonyLib;

namespace ThornClient.Patches;

[HarmonyPatch(typeof(OptionsManager))]
internal static class OptionsManagerPatches {
    [HarmonyPrefix]
    [HarmonyPatch("UnPause")]
    internal static bool UnPause_Prefix(OptionsManager __instance) {
        try {
            return !Managers.InputManager.BlockInput;
        } catch {
            return true;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch("Pause")]
    internal static bool Pause_Prefix(OptionsManager __instance) {
        try {
            return !Managers.InputManager.BlockInput;
        } catch {
            return true;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch("Freeze")]
    internal static bool Freeze_Prefix(OptionsManager __instance) {
        try {
            return !Managers.InputManager.BlockInput;
        } catch {
            return true;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch("UnFreeze")]
    internal static bool UnFreeze_Prefix(OptionsManager __instance) {
        try {
            return !Managers.InputManager.BlockInput;
        } catch {
            return true;
        }
    }
}
