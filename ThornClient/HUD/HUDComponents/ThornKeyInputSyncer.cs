using ThornClient.Core.ConfigurableElements;
using ThornClient.Core.DataTypes;

namespace ThornClient.HUD.HUDComponents;

public class ThornKeyInputSyncer : KeyInputController {
    public Setting<Keybind>? TargetBind;

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
