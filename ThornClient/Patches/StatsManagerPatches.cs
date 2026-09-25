using HarmonyLib;
using ThornClient.Managers;
using ThornClient.System;

namespace ThornClient.Patches;

[HarmonyPatch(typeof(StatsManager))]
internal static class StatsManagerPatches {
    [HarmonyPostfix]
    [HarmonyPatch(nameof(StatsManager.UnhideShit))]
    internal static void UnhideShit_Postfix(StatsManager __instance) {
        try {
            Plugin.Log.LogInfo("SHOW shit");
            HudManager.ShowHud();
        } catch {
            Plugin.Log.LogWarning("[StatsManagerPatches] Failed to un-hide shit");
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(StatsManager.HideShit))]
    internal static void HideShit_Postfix(StatsManager __instance) {
        try {
            Plugin.Log.LogInfo("HIDE shit");
            HudManager.HideHud();
        } catch {
            Plugin.Log.LogWarning("[StatsManagerPatches] Failed to hide shit");
        }
    }
}
