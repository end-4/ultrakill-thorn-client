using NukeLib.UI;
using ThornClient.Managers;
using UnityEngine;
using ThornClient.HUD;
using ThornClient.HUD.HUDComponents;
using Object = UnityEngine.Object;

namespace ThornClient.Modules.HUD;

public class InputMouse : FramedHudModule {
    public override string[] Tags => ["keyboard", "mouse", "fire", "input overlay", "controller"];

    public InputMouse() : base("thorn.inputMouse", "Input - Mouse",
        "Shows fire and alt fire input indicators") {
    }

    protected override GameObject CreateContentObject() {
        var obj = Object.Instantiate(AssetManager.Get<GameObject>(HudManager.BundleKey, "MouseLayout"));
        obj.AddComponent<InputMouseController>();
        return obj;
    }

    private class InputMouseController : MonoBehaviour {
        private void HookButtonVanilla(string path, InputActionState targetInput, string spriteName, string spriteNameFilled) {
            var btn = this.gameObject.FindRecursive(path);
            if (btn == null) return;
            var comp = btn.GetOrAddComponent<VanillaKeyInputSyncer>();
            comp.TargetInput = targetInput;
            comp.baseSprite = AssetManager.Get<Sprite>(HudManager.BundleKey, spriteName);
            comp.baseSpritePressed = AssetManager.Get<Sprite>(HudManager.BundleKey, spriteNameFilled);
        }

        private void Start() {
            var inputSource = InputManager.Instance?.InputSource;
            if (inputSource == null) return;
            HookButtonVanilla("Col/ButtonRow/LMB", inputSource.Fire1, "Round_BorderLargeTopLeft", "Round_FillLargeTopLeft");
            HookButtonVanilla("Col/ButtonRow/RMB", inputSource.Fire2, "Round_BorderLargeTopRight", "Round_FillLargeTopRight");
        }
    }
}
