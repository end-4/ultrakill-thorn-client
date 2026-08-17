using HarmonyLib;
using ThornClient.System;

namespace ThornClient.Patches;

[HarmonyPatch(typeof(OptionsManager))]
internal static class OptionsManagerPatches {
    private static bool AllowPauseActions => !Managers.InputManager.BlockInput && !(ClickGUI.Instance?.IsEnabled ?? false);

    [HarmonyPrefix]
    [HarmonyPatch("UnPause")]
    internal static bool UnPause_Prefix(OptionsManager __instance) {
        try {
            return AllowPauseActions;
        } catch {
            return true;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch("Pause")]
    internal static bool Pause_Prefix(OptionsManager __instance) {
        try {
            return AllowPauseActions;
        } catch {
            return true;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch("Freeze")]
    internal static bool Freeze_Prefix(OptionsManager __instance) {
        try {
            return AllowPauseActions;
        } catch {
            return true;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch("UnFreeze")]
    internal static bool UnFreeze_Prefix(OptionsManager __instance) {
        try {
            return AllowPauseActions;
        } catch {
            return true;
        }
    }
}
