namespace ThornClient.HUD.HUDComponents;

/// <summary>
/// A component that syncs the pressed state of a InputActionState fromt the base game to the appearance of a key in the HUD.
/// </summary>
public class VanillaKeyInputSyncer : KeyInputController {
    /// <summary>
    /// The InputActionState to sync the pressed state of the key to
    /// </summary>
    public InputActionState? TargetInput;

    private void Update() {
        UpdateState();
    }

    private void UpdateState() {
        if (TargetInput == null) return;
        IsPressed = TargetInput.IsPressed;
    }
}
