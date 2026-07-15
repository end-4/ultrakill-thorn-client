using System.Collections.Generic;
using System.Linq;
using Notiffy.API;
using NukeLib.Game.Scores;
using ThornClient.Core;
using UnityEngine.SceneManagement;

namespace ThornClient.Managers;

public static class CheatManager {
    private static bool _cheating = false;

    private static string _hexColor = "#44ff45";

    static CheatManager() {
        SceneManager.sceneLoaded += ResetCheatinessAndUpdate;
    }

    private static void ResetCheatinessAndUpdate(Scene scene, LoadSceneMode mode) {
        ResetCheatiness();
        UpdateCheatiness();
    }

    private static void ResetCheatiness() {
        _cheating = false;
    }

    public static List<Module> GetActiveCheatyModules() {
        return ModuleManager.Modules
            .Where(module => (module.CheatReason != null && module.CheatReason.Length > 0))
            .ToList();
    }

    public static void UpdateCheatiness() {
        // Plugin.Log.LogInfo(
        //     $"Checking in scene '{SceneHelper.CurrentScene}', length = {SceneHelper.CurrentScene?.Length ?? 0}");
        if (SceneHelper.CurrentScene == "Main Menu" || SceneHelper.CurrentScene == "Bootstrap" ||
            !((SceneHelper.CurrentScene?.Length ?? 0) > 0)) return;
        bool lastCheaty = _cheating;
        var cheaties = GetActiveCheatyModules();
        if (cheaties.Count > 0) {
            _cheating = true;
            LeaderboardHelper.DisableLeaderboards();
            // Plugin.Log.LogInfo("Cheaty!");
        }

        if (!lastCheaty && _cheating) {
            NotificationSystem.NotifySend(
                $"Thorn: <color={_hexColor}>Cheats</color>",
                "Leaderboard disabled for this run\n" +
                string.Join("\n",
                    cheaties.Select(
                        module => $"- <color={_hexColor}><u>{module.Name}</u></color>: {module.CheatReason}")),
                iconFilePath: Plugin.PluginIconPath
            );
        }
    }
}
