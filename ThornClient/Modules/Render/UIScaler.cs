using System.Linq;
using UnityEngine;
using ThornClient.Core;
using ThornClient.Core;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ThornClient.Modules.Render;

public class UIScaler : Module {
    private float _savedDefaultScale;
    public Setting<float> Scale { get; }

    public override string IconName => "screen_scale";
    public UIScaler() : base("UI Scaler", "Changes the scale of the user interface", ModuleCategory.Render) {
        Scale = RegisterSetting("Scale", "Smaller = smaller UI elements", 1.0f);
    }

    protected override void OnEnable() {
        Plugin.Log.LogInfo("Enabled ui scaler");
        UpdateScale(Scale.Value);
        SceneManager.sceneLoaded += UpdateScale;
        Scale.OnValueChanged += UpdateScale;
    }

    private void UpdateScale(float value) {
        // Find stuff to work on
        var canCon = CanvasController.Instance;
        if (canCon == null) return;
        var canvas = canCon.gameObject;
        var scalerComp = canvas.GetComponent<CanvasScaler>();

        // Save defaults
        if (scalerComp.uiScaleMode == CanvasScaler.ScaleMode.ScaleWithScreenSize)
            _savedDefaultScale = scalerComp.scaleFactor;

        // Do the tweak
        scalerComp.uiScaleMode = Mathf.Approximately(value, 1) ? CanvasScaler.ScaleMode.ScaleWithScreenSize : CanvasScaler.ScaleMode.ConstantPixelSize;
        // UltraTweaker does this weird hardcoding-ish thing and somehow it works
        // Before someone cries about copyright, two numbers do not meet the threshold of originality
        var prefsMan = PrefsManager.Instance;
        float baseWidth = (prefsMan == null ? 1920f : prefsMan.GetFloatLocal("resolutionWidth"));
        if (scalerComp.uiScaleMode == CanvasScaler.ScaleMode.ConstantPixelSize) {
            scalerComp.scaleFactor = baseWidth / Screen.width * 1.5f * value;
        }
        // After all that and things might still be weird for the lower resolutions.
        // I fucking hate working with screen sizes. Surely everyone plays on FHD in 2026.
    }

    private void UpdateScale(Scene scene, LoadSceneMode mode) {
        UpdateScale(Scale.Value);
    }

    protected override void OnDisable() {
        Scale.OnValueChanged -= UpdateScale;
        SceneManager.sceneLoaded -= UpdateScale;
        UpdateScale(1f);
    }
}
