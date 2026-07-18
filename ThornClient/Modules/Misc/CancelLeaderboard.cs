using ThornClient.Core;
using ThornClient.Managers;

namespace ThornClient.Modules.Misc;

public class CancelLeaderboard : Module {
    public override string IconName => "no_trophy";
    public CancelLeaderboard() : base("thorn.cancelLeaderboard", "Cancel Leaderboard", "Voluntarily exclude scores from the leaderboard. Useful when you're using cheaty mods that don't do this themselves", ModuleCategory.Misc) {
    }

    public override string CheatReason => IsEnabled ? "Obviously" : "";

    protected override void OnEnable() {
        CheatManager.UpdateCheatiness();
    }
}
