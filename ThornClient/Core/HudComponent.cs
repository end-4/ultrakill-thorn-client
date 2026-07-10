using Newtonsoft.Json;
using ThornClient.Settings;

namespace ThornClient.Core;

public abstract class HudComponent : Configurable {
    [JsonIgnore] public Setting<float> OffsetX { get; }
    [JsonIgnore] public Setting<float> OffsetY { get; }

    protected HudComponent(string name, string description, float defaultOffsetX = 0, float defaultOffsetY = 0)
        : base(name, description, UnityEngine.KeyCode.None) {
        OffsetX = RegisterSetting("Offset X", "Horizontal position relative to center", defaultOffsetX);
        OffsetY = RegisterSetting("Offset Y", "Vertical position relative to center", defaultOffsetY);
    }

    public UnityEngine.Vector2 GetAbsolutePosition() {
        float screenWidth = UnityEngine.Screen.width;
        float screenHeight = UnityEngine.Screen.height;
        float centerX = screenWidth / 2f;
        float centerY = screenHeight / 2f;
        return new UnityEngine.Vector2(centerX + OffsetX.Value, centerY + OffsetY.Value);
    }

    public abstract void OnRender();
}
