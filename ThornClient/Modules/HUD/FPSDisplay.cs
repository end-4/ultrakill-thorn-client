using System;
using ThornClient.Core;
using ThornClient.Managers;
using ThornClient.System;
using UnityEngine;
using ThornClient.HUD;

namespace ThornClient.Modules.HUD;

public class FPSDisplay : TextHudModule {
    public Setting<bool> ShowIcon;
    public Setting<bool> ShowFpsText;

    public override Sprite Icon => AssetManager.Get<Sprite>(ClickGUI.BundleKey, "fps");
    public override string[] Tags => ["performance", "frame"];

    public FPSDisplay() : base("thorn.fpsDisplay", "FPS", "Shows framerate") {
        ShowIcon = RegisterSetting("showIcon", "Show icon", "Shows an icon next to the text", true);
        ShowFpsText = RegisterSetting("showFpsText", "Show \"FPS\" text", "Makes the indicator say \"FPS:60\" instead of \"60\"", true);

        UpdateDisplayIcon();
        ShowIcon.OnChanged += UpdateDisplayIcon;
    }

    private const float RefreshInterval = 1f;
    private float _accumulatedTime = 0f;
    private int _frameCount = 0;

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
