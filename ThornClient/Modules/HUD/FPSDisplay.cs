using System;
using UnityEngine;
using ThornClient.HUD;

namespace ThornClient.Modules.HUD;

public class FPSDisplay : TextHudModule {
    public override string IconName => "fps";

    public FPSDisplay() : base("thorn.fpsDisplay", "FPS", "Shows framerate") {
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
            Text = $"FPS:{fps}";

            // Reset buffers
            _accumulatedTime = 0f;
            _frameCount = 0;
        }
    }
}
