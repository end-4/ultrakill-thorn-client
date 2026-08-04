using ThornClient.Core.ConfigurableElements;
using ThornClient.Managers;
using ThornClient.System;
using UnityEngine;
using ThornClient.HUD;

namespace ThornClient.Modules.HUD;

/// <summary>
/// Module that shows FPS
/// </summary>
public class FPSDisplay : TextHudModule {
    /// <summary>
    /// Setting: whether to show the FPS icon on the HUD element
    /// </summary>
    public Setting<bool> ShowIcon;
    /// <summary>
    /// Setting: whether to show "FPS" text on the HUD element
    /// </summary>
    public Setting<bool> ShowFpsText;

    /// <summary>
    /// Icon of the module
    /// </summary>
    public override Sprite Icon => AssetManager.Get<Sprite>(ClickGUI.BundleKey, "fps");
    
    /// <summary>
    /// Tags for search
    /// </summary>
    public override string[] Tags => ["performance", "frame"];

    /// <summary>
    /// Constructor
    /// </summary>
    public FPSDisplay() : base("thorn.fpsDisplay", "FPS", "Shows framerate") {
        ShowIcon = CreateSetting("showIcon", "Show icon", "Shows an icon next to the text", true);
        ShowFpsText = CreateSetting("showFpsText", "Show \"FPS\" text", "Makes the indicator say \"FPS:60\" instead of \"60\"", true);

        UpdateDisplayIcon();
        ShowIcon.OnChanged += UpdateDisplayIcon;
    }

    private const float RefreshInterval = 1f;
    private float _accumulatedTime = 0f;
    private int _frameCount = 0;

    /// <summary>
    /// Stuff that run every frame
    /// </summary>
    public override void OnUpdate() {
        _accumulatedTime += Time.unscaledDeltaTime;
        _frameCount++;

        if (_accumulatedTime >= RefreshInterval) {
            // Calculate frames divided by elapsed time
            int fps = Mathf.RoundToInt(_frameCount / _accumulatedTime);

            // Update UI text string
            string prefix = ShowFpsText.Value ? "FPS:" : "";
            Text = $"{prefix}{fps}";

            // Reset buffers
            _accumulatedTime = 0f;
            _frameCount = 0;
        }
    }

    private void UpdateDisplayIcon() {
        DisplayIcon = ShowIcon.Value ? AssetManager.Get<Sprite>(ClickGUI.BundleKey, "fps") : null;
    }
}
