using ThornClient.Core.ConfigurableElements;
using ThornClient.Core.DataTypes;

namespace ThornClient.HUD.HUDComponents;

/// <summary>
/// A component that syncs the pressed state of a Thorn Keybind setting to the appearance of a key in the HUD.
/// </summary>
public class ThornKeyInputSyncer : KeyInputController {
    /// <summary>
    /// The keybind setting to sync the pressed state of the key to
    /// </summary>
    public Setting<Keybind>? TargetBind;

    /// <summary>
    /// Standard Unity OnEnable declaration
    /// </summary>
    protected void OnEnable() {
        if (TargetBind == null) return;
        TargetBind.OnPress += OnPress;
        TargetBind.OnRelease += OnRelease;
    }

    private void OnDisable() {
        if (TargetBind == null) return;
        TargetBind.OnPress -= OnPress;
        TargetBind.OnRelease -= OnRelease;
    }

    private void OnPress() {
        IsPressed = true;
    }

    private void OnRelease() {
        IsPressed = false;
    }
}
