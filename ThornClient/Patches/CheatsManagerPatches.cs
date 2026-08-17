using HarmonyLib;
using ThornClient.System;

namespace ThornClient.Patches;

[HarmonyPatch(typeof(CheatsManager))]
internal static class CheatsManagerPatches {
    private static bool AllowPauseActions => !Managers.InputManager.BlockInput && !(ClickGUI.Instance?.IsEnabled ?? false);

    [HarmonyPrefix]
    [HarmonyPatch("HandleCheatBind")]
    internal static bool UnPause_Prefix(CheatsManager __instance, string identifier) {
        try {
            return AllowPauseActions;
        } catch {
            return true;
        }
    }
}
