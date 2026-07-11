using UnityEngine;
using ThornClient.Core;
using ThornClient.Settings;

namespace ThornClient.Modules.Render;

public class Zoom : Module {
    public Setting<float> ZoomFov { get; }

    public Zoom() : base("Zoom", "Look more closely", ModuleCategory.Render, KeyCode.C, KeyCode.LeftAlt, true) {
        ZoomFov = RegisterSetting("Zoom FOV", "Zoom amount. Lower = more zoomed in", 30.0f);
    }

    protected override void OnEnable() {
        UpdateZoom(ZoomFov.Value);
        ZoomFov.OnValueChanged += UpdateZoom;
    }

    public override void OnUpdate() {
    }

    private void UpdateZoom(float value) {
        var cc = CameraController.Instance;
        if (cc == null) return;
        cc.Zoom(ZoomFov.Value);
    }

    protected override void OnDisable() {
        ZoomFov.OnValueChanged -= UpdateZoom;
        var cc = CameraController.Instance;
        if (cc == null) return;
        cc.StopZoom();
    }
}
