using HarmonyLib;
using ThornClient.System;

namespace ThornClient.Patches;

[HarmonyPatch(typeof(CheatsManager))]
internal static class CheatsManagerPatches {
    private static bool AllowCheatBinds => !Managers.InputManager.BlockInput && !(ClickGUI.Instance?.IsEnabled ?? false);

    [HarmonyPrefix]
    [HarmonyPatch("HandleCheatBind")]
    internal static bool HandleCheatBind_Prefix(CheatsManager __instance, string identifier) {
        try {
            return AllowCheatBinds;
        } catch {
            return true;
        }
    }
}
