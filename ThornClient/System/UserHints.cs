using System;
using System.IO;
using Notiffy.API;
using UnityEngine.SceneManagement;

namespace ThornClient.System;

internal static class UserHints {
    internal static void SendLaunchNotifsIfNecessary() {
        var thorn = ThornModule.Instance;
        if (thorn == null) return;
        Version lastVersion = new Version(thorn.LastVersion.Value ?? "999.999.999");

        // First run hint
        Version firstRunVersion = new Version("0.1.0");
        if (firstRunVersion.CompareTo(lastVersion) == 1) {
            thorn.LastVersion.Value = firstRunVersion.ToString();
            NotificationSystem.NotifySend("Thorn::<color=#a5f2e2>Hello</color>",
                $"To access the config menu, press {thorn.OpenClickGUI.Value.ToString(true)}",
                expireTime: 10000, iconFilePath: Path.Combine(Plugin.workingDir, "icon.png"));
        }

        SceneManager.sceneLoaded -= SendLaunchNotifsIfNecessary;
    }

    internal static void SendLaunchNotifsIfNecessary(Scene _, LoadSceneMode __) {
        SendLaunchNotifsIfNecessary();
    }

    internal static void Initialize() { }

    static UserHints() {
        SceneManager.sceneLoaded += SendLaunchNotifsIfNecessary;
    }
}
