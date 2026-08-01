namespace ThornClient.HUD.HUDComponents;

public class VanillaKeyInputSyncer : KeyInputController {
    public InputActionState? TargetInput;

    private void Update() {
        UpdateState();
    }

    private void UpdateState() {
        if (TargetInput == null) return;
        IsPressed = TargetInput.IsPressed;
    }
}
