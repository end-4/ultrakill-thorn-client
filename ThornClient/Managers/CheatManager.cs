using System.Collections.Generic;
using System.Linq;
using Notiffy.API;
using NukeLib.Scores;
using ThornClient.Core;
using UnityEngine.SceneManagement;

namespace ThornClient.Managers;

public static class CheatManager {
    private static bool _cheating = false;

    static CheatManager() {
        SceneManager.sceneLoaded += ResetCheatiness;
    }

    private static void ResetCheatiness(Scene scene, LoadSceneMode mode) {
        ResetCheatiness();
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
        bool lastCheaty = _cheating;
        var cheaties = GetActiveCheatyModules();
        if (cheaties.Count > 0) {
            _cheating = true;
            LeaderboardHelper.DisableLeaderboards();
        }

        if (!lastCheaty && _cheating) {
            NotificationSystem.NotifySend(
                "Thorn: Cheat manager",
                "Leaderboard disabled for this run:\n" +
                string.Join("\n", cheaties.Select(module => $"- <u>{module.Name}</u>: {module.CheatReason}")),
                iconFilePath: Plugin.PluginIconPath
            );
        }
    }
}
