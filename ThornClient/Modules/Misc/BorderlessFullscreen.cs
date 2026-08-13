using ThornClient.Core;
using UnityEngine;

namespace ThornClient.Modules.Misc;

/// <summary>
/// Module that disables score submissions
/// </summary>
public class BorderlessFullscreen : Module {
    // public override Sprite Icon => AssetManager.Get<Sprite>(ClickGUI.BundleKey, "no_trophy");

    /// <inheritdoc />
    public override string[] Tags => ["no minimize", "desktop", "alt tab"];

    /// <inheritdoc />
    public BorderlessFullscreen() : base("thorn.borderlessFulscreen", "Borderless Fullscreen",
        "Applies borderless fullscreen",
        ModuleCategory.Misc) {
    }

    /// <inheritdoc />
    protected override void OnEnable() {
        Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
    }

    /// <inheritdoc />
    protected override void OnDisable() {
        if (PrefsManager.Instance != null) {
            var fullScreen = PrefsManager.Instance.GetBoolLocal("fullscreen");
            Screen.fullScreenMode = fullScreen ? FullScreenMode.ExclusiveFullScreen : FullScreenMode.Windowed;
        }
    }
}
