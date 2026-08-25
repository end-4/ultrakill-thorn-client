using System;
using System.IO;
using Notiffy.API;
using UnityEngine.SceneManagement;

namespace ThornClient.System;

internal static class UserHints {
    internal static void SendLaunchNotifsIfNecessary() {
        var thorn = ThornModule.Instance;
        if (thorn == null) return;
        Version neverRun = new Version("0.0.0");
        Version lastVersion = new Version(thorn.LastVersion.Value ?? "999.999.999");

        // First run hint
        Version firstRunVersion = new Version("0.1.0");
        if (firstRunVersion.CompareTo(lastVersion) == 1) {
            NotificationSystem.NotifySend("Thorn::<color=#a5f2e2>Hello</color>",
                $"To access the config menu, press {thorn.OpenClickGUI.Value.ToString(true)}",
                expireTime: 10000, iconFilePath: Path.Combine(Plugin.workingDir, "icon.png"));
        }

        Version shiftedPositionVersion = new Version("0.1.5");
        if (shiftedPositionVersion.CompareTo(lastVersion) == 1 && lastVersion.CompareTo(neverRun) == 1) {
            NotificationSystem.NotifySend("Thorn::<color=#a5f2e2>Updated</color>",
                $"<b>0.1.5 note</b>: Position of HUD widgets might've shifted a bit (again); really sorry about that.\nBut we have profiles now!\nIf further issues arise, yell at @end_4 on Discord.",
                expireTime: 16000, iconFilePath: Path.Combine(Plugin.workingDir, "icon.png"));
        }

        thorn.LastVersion.Value = Plugin.PluginVersion;
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
