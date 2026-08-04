using NukeLib.UI;
using ThornClient.Managers;
using UnityEngine;
using ThornClient.HUD;
using ThornClient.HUD.HUDComponents;
using ThornClient.System;
using Object = UnityEngine.Object;

namespace ThornClient.Modules.HUD;

/// <summary>
/// A HUD module that shows general input information, including movement, fist, and variant cycle inputs.
/// </summary>
public class InputGeneral : FramedHudModule {
    /// <summary>
    /// The icon for this module, which is a WASD key layout.
    /// </summary>
    public override Sprite Icon => AssetManager.Get<Sprite>(ClickGUI.BundleKey, "wasd");
    /// <summary>
    /// Tags for searching
    /// </summary>
    public override string[] Tags => ["keyboard", "movement", "punch", "controller"];

    /// <summary>
    /// Constructor
    /// </summary>
    public InputGeneral() : base("thorn.inputGeneral", "Input - General",
        "Shows movement-related, fist, and variant cycle inputs") {
    }

    /// <summary>
    /// Creates the content object that sits on the frame
    /// </summary>
    /// <returns>The content object</returns>
    protected override GameObject CreateContentObject() {
        var obj = Object.Instantiate(AssetManager.Get<GameObject>(HudManager.BundleKey, "KeyLayoutGeneral"));
        obj.AddComponent<InputGeneralController>();
        return obj;
    }

    private class InputGeneralController : MonoBehaviour {
        private void HookButtonVanilla(string path, InputActionState targetInput) {
            var btn = this.gameObject.FindRecursive(path);
            if (btn == null) return;
            var comp = btn.GetOrAddComponent<VanillaKeyInputSyncer>();
            comp.TargetInput = targetInput;
        }

        private void UpdateKeyPressed(KeyInputController? keyInputController, bool pressed) {
            if (keyInputController == null) return;
            keyInputController.IsPressed = pressed;
        }

        private KeyInputController? _u;
        private KeyInputController? _d;
        private KeyInputController? _l;
        private KeyInputController? _r;
        private KeyInputController? _feedbacker;
        private KeyInputController? _knuckleblaster;
        private KeyInputController? _fistSwap;

        private void Start() {
            var inputSource = InputManager.Instance?.InputSource;
            if (inputSource == null) return;

            // Movement
            HookButtonVanilla("Col/General/MovementCol/KeyDash", inputSource.Dodge);
            HookButtonVanilla("Col/General/MovementCol/KeySlam", inputSource.Slide);
            HookButtonVanilla("Col/General/MainCol/KeyJump", inputSource.Jump);
            HookButtonVanilla("Col/General/FistCol/KeyWhiplash", inputSource.Hook);

            // Variant switching
            HookButtonVanilla("Col/General/MainCol/TopRow/VariantSwitch/Prev", inputSource.PreviousVariation);
            HookButtonVanilla("Col/General/MainCol/TopRow/VariantSwitch/Next", inputSource.NextVariation);

            // Punch. Controlled in Update()
            _feedbacker = gameObject.FindRecursive("Col/General/FistCol/PunchRow/KeyFeedbacker")?.AddComponent<KeyInputController>();
            _knuckleblaster = gameObject.FindRecursive("Col/General/FistCol/PunchRow/KeyKnuckleblaster")?.AddComponent<KeyInputController>();
            _fistSwap = gameObject.FindRecursive("Col/General/FistCol/SwapRow/KeySwap")?.AddComponent<KeyInputController>();

            // WASD. Controlled in Update()
            _u = gameObject.FindRecursive("Col/General/MainCol/TopRow/KeyUp")?.AddComponent<KeyInputController>();
            _d = gameObject.FindRecursive("Col/General/MainCol/BottomRow/KeyDown")?.AddComponent<KeyInputController>();
            _l = gameObject.FindRecursive("Col/General/MainCol/BottomRow/KeyLeft")?.AddComponent<KeyInputController>();
            _r = gameObject.FindRecursive("Col/General/MainCol/BottomRow/KeyRight")?.AddComponent<KeyInputController>();
        }

        private void Update() {
            var inputSource = MonoSingleton<InputManager>.Instance?.InputSource;
            if (inputSource == null) return;

            Vector2 moveVector = inputSource.Move.ReadValue<Vector2>();
            UpdateKeyPressed(_u, moveVector.y > 0f);
            UpdateKeyPressed(_d, moveVector.y < 0f);
            UpdateKeyPressed(_l, moveVector.x < 0f);
            UpdateKeyPressed(_r, moveVector.x > 0f);

            bool punching = inputSource.Punch.IsPressed;
            bool swapping = inputSource.ChangeFist.IsPressed;
            int currentFist = PlayerPrefs.GetInt("CurArm", 0);
            bool feedbacking = inputSource.Actions.Fist.PunchFeedbacker.IsPressed() || (punching && currentFist == 0);
            bool knuckleblasting = inputSource.Actions.Fist.PunchKnuckleblaster.IsPressed() || (punching && currentFist == 1);

            UpdateKeyPressed(_feedbacker, feedbacking);
            UpdateKeyPressed(_knuckleblaster, knuckleblasting);
            UpdateKeyPressed(_fistSwap, swapping);
        }
    }
}
